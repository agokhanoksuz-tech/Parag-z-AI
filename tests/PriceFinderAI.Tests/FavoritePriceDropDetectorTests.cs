using PriceFinderAI.Application.Services;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Tests;

public class FavoritePriceDropDetectorTests
{
    private static Favorite NewFavorite(
        string store,
        decimal priceAtFavoriteTime,
        decimal? lastNotifiedPrice = null,
        decimal? targetPrice = null) =>
        new()
        {
            UserId = "user-1",
            TrackedProductId = 1,
            StoreName = store,
            ProductName = "iPhone 15",
            Url = "https://example.com",
            PriceAtFavoriteTime = priceAtFavoriteTime,
            LastNotifiedPrice = lastNotifiedPrice,
            TargetPrice = targetPrice,
            CreatedAt = DateTime.UtcNow
        };

    private static PriceResult Result(string store, decimal price) =>
        new(store, "iPhone 15", price, 0, 0, "https://example.com");

    [Fact]
    public void DetectDrops_FindsDrop_WhenCurrentPriceBelowFavoriteBaseline()
    {
        var favorites = new[] { NewFavorite("Teknosa", priceAtFavoriteTime: 30000) };
        var currentResults = new[] { Result("Teknosa", 27000) };

        var drops = FavoritePriceDropDetector.DetectDrops(favorites, currentResults);

        var drop = Assert.Single(drops);
        Assert.Equal(27000, drop.NewPrice);
    }

    [Fact]
    public void DetectDrops_IgnoresUnchangedOrHigherPrice()
    {
        var favorites = new[] { NewFavorite("Teknosa", priceAtFavoriteTime: 30000) };
        var currentResults = new[] { Result("Teknosa", 30000), Result("Teknosa", 35000) };

        var drops = FavoritePriceDropDetector.DetectDrops(favorites, currentResults);

        Assert.Empty(drops);
    }

    [Fact]
    public void DetectDrops_IgnoresStoresNotInCurrentResults()
    {
        var favorites = new[] { NewFavorite("Teknosa", priceAtFavoriteTime: 30000) };
        var currentResults = new[] { Result("Hepsiburada", 20000) };

        var drops = FavoritePriceDropDetector.DetectDrops(favorites, currentResults);

        Assert.Empty(drops);
    }

    [Fact]
    public void DetectDrops_UsesLastNotifiedPrice_NotOriginalBaseline_WhenAlreadyNotifiedOnce()
    {
        // Fiyat daha önce 27000'e düşmüş ve bildirilmişti (LastNotifiedPrice=27000).
        // Şimdi tekrar 27000'e "düşmüş" gibi görünse de aslında değişmedi — tekrar bildirim yapılmamalı.
        var favorites = new[] { NewFavorite("Teknosa", priceAtFavoriteTime: 30000, lastNotifiedPrice: 27000) };
        var currentResults = new[] { Result("Teknosa", 27000) };

        var drops = FavoritePriceDropDetector.DetectDrops(favorites, currentResults);

        Assert.Empty(drops);
    }

    [Fact]
    public void DetectDrops_FindsFurtherDrop_BelowLastNotifiedPrice()
    {
        var favorites = new[] { NewFavorite("Teknosa", priceAtFavoriteTime: 30000, lastNotifiedPrice: 27000) };
        var currentResults = new[] { Result("Teknosa", 24000) };

        var drops = FavoritePriceDropDetector.DetectDrops(favorites, currentResults);

        var drop = Assert.Single(drops);
        Assert.Equal(24000, drop.NewPrice);
    }

    [Fact]
    public void DetectDrops_IsCaseInsensitive_ForStoreNameMatching()
    {
        var favorites = new[] { NewFavorite("teknosa", priceAtFavoriteTime: 30000) };
        var currentResults = new[] { Result("TEKNOSA", 25000) };

        var drops = FavoritePriceDropDetector.DetectDrops(favorites, currentResults);

        Assert.Single(drops);
    }

    [Fact]
    public void DetectDrops_IgnoresImprovement_WhenTargetPriceNotYetReached()
    {
        // Fiyat düştü (30000 -> 28000) ama kullanıcının belirlediği hedef fiyata
        // (25000) henüz ulaşılmadı — bildirim gönderilmemeli.
        var favorites = new[] { NewFavorite("Teknosa", priceAtFavoriteTime: 30000, targetPrice: 25000) };
        var currentResults = new[] { Result("Teknosa", 28000) };

        var drops = FavoritePriceDropDetector.DetectDrops(favorites, currentResults);

        Assert.Empty(drops);
    }

    [Fact]
    public void DetectDrops_FindsDrop_WhenPriceReachesTarget()
    {
        var favorites = new[] { NewFavorite("Teknosa", priceAtFavoriteTime: 30000, targetPrice: 25000) };
        var currentResults = new[] { Result("Teknosa", 24500) };

        var drops = FavoritePriceDropDetector.DetectDrops(favorites, currentResults);

        var drop = Assert.Single(drops);
        Assert.Equal(24500, drop.NewPrice);
    }

    [Fact]
    public void DetectDrops_FindsDrop_WhenPriceExactlyEqualsTarget()
    {
        var favorites = new[] { NewFavorite("Teknosa", priceAtFavoriteTime: 30000, targetPrice: 25000) };
        var currentResults = new[] { Result("Teknosa", 25000) };

        var drops = FavoritePriceDropDetector.DetectDrops(favorites, currentResults);

        Assert.Single(drops);
    }
}
