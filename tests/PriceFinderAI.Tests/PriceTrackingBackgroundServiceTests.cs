using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Options;
using PriceFinderAI.Core.Models;
using PriceFinderAI.Infrastructure.BackgroundJobs;
using PriceFinderAI.Infrastructure.Data;

namespace PriceFinderAI.Tests;

public class PriceTrackingBackgroundServiceTests
{
    private static PriceTrackingBackgroundService CreateSut(out IServiceProvider provider)
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddScoped<IPriceHistoryStore>(sp =>
            new EfPriceHistoryStore(sp.GetRequiredService<AppDbContext>(), maxTrackedProducts: 200));

        services.AddScoped<IFavoriteStore>(sp =>
            new EfFavoriteStore(sp.GetRequiredService<AppDbContext>(), Options.Create(new FavoritesOptions())));

        services.AddScoped<IUserAccountStore>(sp =>
            new EfUserAccountStore(sp.GetRequiredService<AppDbContext>()));

        services.AddScoped<IEmailSender, FakeEmailSender>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PriceTracking:RefreshInterval"] = "1.00:00:00",
                ["PriceTracking:MaxBatchPerRun"] = "3"
            })
            .Build();

        provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        return new PriceTrackingBackgroundService(
            scopeFactory,
            configuration,
            NullLogger<PriceTrackingBackgroundService>.Instance);
    }

    private static PriceResult Result(string store, decimal price) =>
        new(store, "iPhone 15", price, 0, 0, "https://example.com");

    // RunOnceAsync's per-query loop constructs the real WebSearchPriceProvider/TeknosaProvider
    // internally (no injection seam, by design — see PriceSearchPipeline usage in Program.cs),
    // so it can't be exercised here without hitting the real network. This test only verifies
    // the "nothing due" path: DI wiring + scope creation work, and an empty tracked list means
    // no provider is ever touched.
    [Fact]
    public async Task RunOnceAsync_CompletesWithoutError_WhenNoProductsAreTracked()
    {
        var sut = CreateSut(out _);

        await sut.RunOnceAsync(CancellationToken.None);
    }

    [Fact]
    public async Task NotifyFavoritesOfPriceDropsAsync_SendsEmailAndMarksNotified_WhenPriceDrops()
    {
        var sut = CreateSut(out var provider);
        using var scope = provider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var favoriteStore = scope.ServiceProvider.GetRequiredService<IFavoriteStore>();
        var userStore = scope.ServiceProvider.GetRequiredService<IUserAccountStore>();
        var emailSender = (FakeEmailSender)scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var user = new User
        {
            Id = "user-1",
            Email = "alici@example.com",
            NormalizedEmail = "alici@example.com",
            PasswordHash = "irrelevant",
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var (_, favorite) = await favoriteStore.AddAsync(new Favorite
        {
            UserId = user.Id,
            TrackedProductId = 1,
            StoreName = "Teknosa",
            ProductName = "iPhone 15",
            Url = "https://example.com",
            PriceAtFavoriteTime = 30000,
            CreatedAt = DateTime.UtcNow
        });

        await sut.NotifyFavoritesOfPriceDropsAsync(
            favoriteStore, userStore, emailSender, trackedProductId: 1, [Result("Teknosa", 27000)], CancellationToken.None);

        Assert.Single(emailSender.SentEmails);
        Assert.Equal("alici@example.com", emailSender.SentEmails[0].ToEmail);

        var reloaded = await db.Favorites.FindAsync(favorite!.Id);
        Assert.Equal(27000, reloaded!.LastNotifiedPrice);
    }

    [Fact]
    public async Task NotifyFavoritesOfPriceDropsAsync_DoesNotMarkNotified_WhenEmailSendThrows()
    {
        var sut = CreateSut(out var provider);
        using var scope = provider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var favoriteStore = scope.ServiceProvider.GetRequiredService<IFavoriteStore>();
        var userStore = scope.ServiceProvider.GetRequiredService<IUserAccountStore>();

        var user = new User
        {
            Id = "user-1",
            Email = "alici@example.com",
            NormalizedEmail = "alici@example.com",
            PasswordHash = "irrelevant",
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var (_, favorite) = await favoriteStore.AddAsync(new Favorite
        {
            UserId = user.Id,
            TrackedProductId = 1,
            StoreName = "Teknosa",
            ProductName = "iPhone 15",
            Url = "https://example.com",
            PriceAtFavoriteTime = 30000,
            CreatedAt = DateTime.UtcNow
        });

        await sut.NotifyFavoritesOfPriceDropsAsync(
            favoriteStore, userStore, new ThrowingEmailSender(), trackedProductId: 1, [Result("Teknosa", 27000)], CancellationToken.None);

        var reloaded = await db.Favorites.FindAsync(favorite!.Id);
        Assert.Null(reloaded!.LastNotifiedPrice);
    }

    [Fact]
    public async Task NotifyFavoritesOfPriceDropsAsync_DoesNothing_WhenNoFavoritesForTrackedProduct()
    {
        var sut = CreateSut(out var provider);
        using var scope = provider.CreateScope();

        var favoriteStore = scope.ServiceProvider.GetRequiredService<IFavoriteStore>();
        var userStore = scope.ServiceProvider.GetRequiredService<IUserAccountStore>();
        var emailSender = (FakeEmailSender)scope.ServiceProvider.GetRequiredService<IEmailSender>();

        await sut.NotifyFavoritesOfPriceDropsAsync(
            favoriteStore, userStore, emailSender, trackedProductId: 999, [Result("Teknosa", 27000)], CancellationToken.None);

        Assert.Empty(emailSender.SentEmails);
    }
}
