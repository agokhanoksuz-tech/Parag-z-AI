using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Services;
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
                await store.RecordSnapshotAsync(query, outcome.Results, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Fiyat takibi güncellenemedi: {Query}", query);
            }
        }
    }
}
