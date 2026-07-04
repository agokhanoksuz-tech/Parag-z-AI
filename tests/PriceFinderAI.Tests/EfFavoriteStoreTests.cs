using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Options;
using PriceFinderAI.Core.Models;
using PriceFinderAI.Infrastructure.Data;

namespace PriceFinderAI.Tests;

public class EfFavoriteStoreTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static EfFavoriteStore CreateStore(AppDbContext db, int maxFavoritesPerUser = 50) =>
        new(db, Options.Create(new FavoritesOptions { MaxFavoritesPerUser = maxFavoritesPerUser }));

    private static Favorite NewFavorite(string userId = "user-1", int trackedProductId = 1, string store = "Teknosa") =>
        new()
        {
            UserId = userId,
            TrackedProductId = trackedProductId,
            StoreName = store,
            ProductName = "iPhone 15",
            Url = "https://example.com",
            PriceAtFavoriteTime = 30000,
            CreatedAt = DateTime.UtcNow
        };

    [Fact]
    public async Task AddAsync_Succeeds_ForNewFavorite()
    {
        await using var db = CreateContext();
        var store = CreateStore(db);

        var (outcome, favorite) = await store.AddAsync(NewFavorite());

        Assert.Equal(AddFavoriteOutcome.Success, outcome);
        Assert.NotNull(favorite);
        Assert.Equal(1, await db.Favorites.CountAsync());
    }

    [Fact]
    public async Task AddAsync_RejectsDuplicate_SameUserTrackedProductAndStore()
    {
        await using var db = CreateContext();
        var store = CreateStore(db);

        await store.AddAsync(NewFavorite());
        var (outcome, favorite) = await store.AddAsync(NewFavorite());

        Assert.Equal(AddFavoriteOutcome.AlreadyFavorited, outcome);
        Assert.Null(favorite);
    }

    [Fact]
    public async Task AddAsync_RejectsNewFavorite_WhenAtLimit()
    {
        await using var db = CreateContext();
        var store = CreateStore(db, maxFavoritesPerUser: 1);

        await store.AddAsync(NewFavorite(store: "Teknosa"));
        var (outcome, favorite) = await store.AddAsync(NewFavorite(store: "Hepsiburada"));

        Assert.Equal(AddFavoriteOutcome.LimitReached, outcome);
        Assert.Null(favorite);
    }

    [Fact]
    public async Task RemoveAsync_ReturnsFalse_WhenFavoriteBelongsToDifferentUser()
    {
        await using var db = CreateContext();
        var store = CreateStore(db);

        var (_, favorite) = await store.AddAsync(NewFavorite(userId: "owner"));

        var removed = await store.RemoveAsync(favorite!.Id, "intruder");

        Assert.False(removed);
        Assert.Equal(1, await db.Favorites.CountAsync());
    }

    [Fact]
    public async Task RemoveAsync_RemovesFavorite_WhenOwnedByUser()
    {
        await using var db = CreateContext();
        var store = CreateStore(db);

        var (_, favorite) = await store.AddAsync(NewFavorite(userId: "owner"));

        var removed = await store.RemoveAsync(favorite!.Id, "owner");

        Assert.True(removed);
        Assert.Equal(0, await db.Favorites.CountAsync());
    }

    [Fact]
    public async Task MarkNotifiedAsync_UpdatesLastNotifiedPrice()
    {
        await using var db = CreateContext();
        var store = CreateStore(db);

        var (_, favorite) = await store.AddAsync(NewFavorite());
        await store.MarkNotifiedAsync(favorite!.Id, 27000);

        var reloaded = await db.Favorites.SingleAsync();
        Assert.Equal(27000, reloaded.LastNotifiedPrice);
    }

    [Fact]
    public async Task GetForUserAsync_ReturnsOnlyThatUsersFavorites()
    {
        await using var db = CreateContext();
        var store = CreateStore(db);

        await store.AddAsync(NewFavorite(userId: "user-1", trackedProductId: 1));
        await store.AddAsync(NewFavorite(userId: "user-2", trackedProductId: 1));

        var favorites = await store.GetForUserAsync("user-1");

        Assert.Single(favorites);
    }

    [Fact]
    public async Task SetTargetPriceAsync_UpdatesTargetPrice_WhenOwnedByUser()
    {
        await using var db = CreateContext();
        var store = CreateStore(db);

        var (_, favorite) = await store.AddAsync(NewFavorite(userId: "owner"));

        var updated = await store.SetTargetPriceAsync(favorite!.Id, "owner", 25000);

        Assert.True(updated);
        var reloaded = await db.Favorites.SingleAsync();
        Assert.Equal(25000, reloaded.TargetPrice);
    }

    [Fact]
    public async Task SetTargetPriceAsync_ReturnsFalse_WhenNotOwnedByUser()
    {
        await using var db = CreateContext();
        var store = CreateStore(db);

        var (_, favorite) = await store.AddAsync(NewFavorite(userId: "owner"));

        var updated = await store.SetTargetPriceAsync(favorite!.Id, "intruder", 25000);

        Assert.False(updated);
        var reloaded = await db.Favorites.SingleAsync();
        Assert.Null(reloaded.TargetPrice);
    }

    [Fact]
    public async Task SetTargetPriceAsync_CanClearTargetPrice_WithNull()
    {
        await using var db = CreateContext();
        var store = CreateStore(db);

        var (_, favorite) = await store.AddAsync(NewFavorite(userId: "owner"));
        await store.SetTargetPriceAsync(favorite!.Id, "owner", 25000);

        await store.SetTargetPriceAsync(favorite.Id, "owner", null);

        var reloaded = await db.Favorites.SingleAsync();
        Assert.Null(reloaded.TargetPrice);
    }
}
