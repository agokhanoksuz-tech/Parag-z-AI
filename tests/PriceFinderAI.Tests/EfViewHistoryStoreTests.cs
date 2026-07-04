using Microsoft.EntityFrameworkCore;
using PriceFinderAI.Core.Models;
using PriceFinderAI.Infrastructure.Data;

namespace PriceFinderAI.Tests;

public class EfViewHistoryStoreTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<int> TrackAndSnapshotAsync(AppDbContext db, string query, string store, decimal price)
    {
        var tracked = new TrackedProduct { Query = query, CreatedAt = DateTime.UtcNow, LastCheckedAt = DateTime.UtcNow };
        db.TrackedProducts.Add(tracked);
        await db.SaveChangesAsync();

        db.PriceSnapshots.Add(new PriceSnapshot
        {
            TrackedProductId = tracked.Id,
            StoreName = store,
            ProductName = "iPhone 15",
            Price = price,
            Url = "https://example.com",
            CheckedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return tracked.Id;
    }

    [Fact]
    public async Task RecordViewAsync_CreatesNewEntry_ForFirstView()
    {
        await using var db = CreateContext();
        var store = new EfViewHistoryStore(db);
        var trackedProductId = await TrackAndSnapshotAsync(db, "iphone 15", "Teknosa", 30000);

        await store.RecordViewAsync("user-1", trackedProductId);

        Assert.Equal(1, await db.ViewedProducts.CountAsync());
    }

    [Fact]
    public async Task RecordViewAsync_UpdatesTimestamp_ForRepeatedView_DoesNotDuplicate()
    {
        await using var db = CreateContext();
        var store = new EfViewHistoryStore(db);
        var trackedProductId = await TrackAndSnapshotAsync(db, "iphone 15", "Teknosa", 30000);

        await store.RecordViewAsync("user-1", trackedProductId);
        await store.RecordViewAsync("user-1", trackedProductId);

        Assert.Equal(1, await db.ViewedProducts.CountAsync());
    }

    [Fact]
    public async Task GetRecentlyViewedAsync_ReturnsMostRecentFirst()
    {
        await using var db = CreateContext();
        var store = new EfViewHistoryStore(db);

        var iphoneId = await TrackAndSnapshotAsync(db, "iphone 15", "Teknosa", 30000);
        var macbookId = await TrackAndSnapshotAsync(db, "macbook air m2", "Teknosa", 25000);

        await store.RecordViewAsync("user-1", iphoneId);
        await store.RecordViewAsync("user-1", macbookId);

        var recent = await store.GetRecentlyViewedAsync("user-1", count: 8);

        Assert.Equal(2, recent.Count);
        Assert.Equal("macbook air m2", recent[0].Query);
        Assert.Equal("iphone 15", recent[1].Query);
    }

    [Fact]
    public async Task GetRecentlyViewedAsync_RespectsCountLimit()
    {
        await using var db = CreateContext();
        var store = new EfViewHistoryStore(db);

        var iphoneId = await TrackAndSnapshotAsync(db, "iphone 15", "Teknosa", 30000);
        var macbookId = await TrackAndSnapshotAsync(db, "macbook air m2", "Teknosa", 25000);

        await store.RecordViewAsync("user-1", iphoneId);
        await store.RecordViewAsync("user-1", macbookId);

        var recent = await store.GetRecentlyViewedAsync("user-1", count: 1);

        Assert.Single(recent);
    }

    [Fact]
    public async Task GetRecentlyViewedAsync_ReturnsOnlyThatUsersHistory()
    {
        await using var db = CreateContext();
        var store = new EfViewHistoryStore(db);

        var trackedProductId = await TrackAndSnapshotAsync(db, "iphone 15", "Teknosa", 30000);

        await store.RecordViewAsync("user-1", trackedProductId);

        var recentForOtherUser = await store.GetRecentlyViewedAsync("user-2", count: 8);

        Assert.Empty(recentForOtherUser);
    }
}
