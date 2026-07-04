using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Services;
using PriceFinderAI.Core.Models;
using PriceFinderAI.Infrastructure.Providers;

namespace PriceFinderAI.Infrastructure.BackgroundJobs;

public sealed class PriceTrackingBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<PriceTrackingBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = configuration.GetValue("PriceTracking:RefreshInterval", TimeSpan.FromHours(24));
        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fiyat takibi taraması başarısız oldu");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    internal async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IPriceHistoryStore>();
        var favoriteStore = scope.ServiceProvider.GetRequiredService<IFavoriteStore>();
        var userStore = scope.ServiceProvider.GetRequiredService<IUserAccountStore>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var refreshInterval = configuration.GetValue("PriceTracking:RefreshInterval", TimeSpan.FromHours(24));
        var maxBatch = configuration.GetValue("PriceTracking:MaxBatchPerRun", 3);

        var dueQueries = await store.GetQueriesDueForRefreshAsync(refreshInterval, maxBatch, cancellationToken);

        var apiKey = configuration["SearchApi:ApiKey"];
        var baseUrl = configuration["SearchApi:BaseUrl"];
        var pipeline = new PriceSearchPipeline();

        foreach (var query in dueQueries)
        {
            try
            {
                IReadOnlyList<IPriceProvider> providers =
                [
                    new WebSearchPriceProvider(apiKey, baseUrl),
                    new TeknosaProvider()
                ];

                var outcome = await pipeline.RunAsync(query, providers, logger, cancellationToken);
                var trackedProductId = await store.RecordSnapshotAsync(query, outcome.Results, cancellationToken);

                if (trackedProductId is int id)
                {
                    await NotifyFavoritesOfPriceDropsAsync(
                        favoriteStore, userStore, emailSender, id, outcome.Results, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Fiyat takibi güncellenemedi: {Query}", query);
            }
        }
    }

    // Ayrı bir metot olarak çıkarıldı: RunOnceAsync'in geri kalanı gerçek ağ çağrısı
    // yapan sağlayıcıları içerdiği için testte çalıştırılamıyor (yukarıdaki foreach'e bakın) —
    // bu metot ise fake IFavoriteStore/IEmailSender ile doğrudan test edilebilir.
    internal async Task NotifyFavoritesOfPriceDropsAsync(
        IFavoriteStore favoriteStore,
        IUserAccountStore userStore,
        IEmailSender emailSender,
        int trackedProductId,
        IReadOnlyList<PriceResult> currentResults,
        CancellationToken cancellationToken)
    {
        try
        {
            var favorites = await favoriteStore.GetForTrackedProductAsync(trackedProductId, cancellationToken);
            var drops = FavoritePriceDropDetector.DetectDrops(favorites, currentResults);

            foreach (var (favorite, newPrice) in drops)
            {
                var user = await userStore.FindByIdAsync(favorite.UserId, cancellationToken);
                if (user is null)
                    continue;

                var (subject, htmlBody) = PriceDropEmailTemplate.Build(favorite, newPrice);
                await emailSender.SendAsync(user.Email, subject, htmlBody, cancellationToken);

                // Sadece gönderim onaylandıktan sonra işaretlenir — başarısız gönderim
                // bir sonraki taramada tekrar denenir.
                await favoriteStore.MarkNotifiedAsync(favorite.Id, newPrice, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Favori bildirimi başarısız: {TrackedProductId}", trackedProductId);
        }
    }
}
