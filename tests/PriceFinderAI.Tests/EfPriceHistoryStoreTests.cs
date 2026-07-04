using Microsoft.EntityFrameworkCore;
using PriceFinderAI.Core.Models;
using PriceFinderAI.Infrastructure.Data;

namespace PriceFinderAI.Tests;

public class EfPriceHistoryStoreTests
{
    private static PriceHistoryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PriceHistoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PriceHistoryDbContext(options);
    }

    private static PriceResult Result(string store, decimal price) =>
        new(store, "iPhone 15", price, 0, 0, "https://example.com");

    [Fact]
    public async Task EnsureTrackedAsync_CreatesTrackedProduct_WhenNew()
    {
        await using var db = CreateContext();
        var store = new EfPriceHistoryStore(db, maxTrackedProducts: 200);

        await store.EnsureTrackedAsync("iphone 15");

        Assert.Equal(1, await db.TrackedProducts.CountAsync());
    }

    [Fact]
    public async Task EnsureTrackedAsync_DoesNotDuplicate_ForDifferentlyCasedSameQuery()
    {
        await using var db = CreateContext();
        var store = new EfPriceHistoryStore(db, maxTrackedProducts: 200);

        await store.EnsureTrackedAsync("iPhone 15");
        await store.EnsureTrackedAsync("IPHONE 15");

        Assert.Equal(1, await db.TrackedProducts.CountAsync());
    }

    [Fact]
    public async Task EnsureTrackedAsync_SkipsNewProduct_WhenAtCap()
    {
        await using var db = CreateContext();
        var store = new EfPriceHistoryStore(db, maxTrackedProducts: 1);

        await store.EnsureTrackedAsync("iphone 15");
        await store.EnsureTrackedAsync("samsung s24");

        Assert.Equal(1, await db.TrackedProducts.CountAsync());
    }

    [Fact]
    public async Task RecordSnapshotAsync_DoesNothing_WhenQueryNotTracked()
    {
        await using var db = CreateContext();
        var store = new EfPriceHistoryStore(db, maxTrackedProducts: 200);

        await store.RecordSnapshotAsync("iphone 15", [Result("Teknosa", 30000)]);

        Assert.Equal(0, await db.PriceSnapshots.CountAsync());
    }

    [Fact]
    public async Task GetLowestPricesLast30DaysAsync_ReturnsMinimumPerStore_WithinWindow()
    {
        await using var db = CreateContext();
        var store = new EfPriceHistoryStore(db, maxTrackedProducts: 200);

        await store.EnsureTrackedAsync("iphone 15");
        await store.RecordSnapshotAsync("iphone 15", [Result("Teknosa", 30000)]);
        await store.RecordSnapshotAsync("iphone 15", [Result("Teknosa", 25000)]);
        await store.RecordSnapshotAsync("iphone 15", [Result("Hepsiburada", 31000)]);

        var lowest = await store.GetLowestPricesLast30DaysAsync("iphone 15");

        Assert.Equal(25000, lowest["Teknosa"]);
        Assert.Equal(31000, lowest["Hepsiburada"]);
    }

    [Fact]
    public async Task GetLowestPricesLast30DaysAsync_IgnoresSnapshotsOlderThan30Days()
    {
        await using var db = CreateContext();
        var store = new EfPriceHistoryStore(db, maxTrackedProducts: 200);

        await store.EnsureTrackedAsync("iphone 15");
        var tracked = await db.TrackedProducts.SingleAsync();

        db.PriceSnapshots.Add(new PriceSnapshot
        {
            TrackedProductId = tracked.Id,
            StoreName = "Teknosa",
            ProductName = "iPhone 15",
            Price = 10000,
            Url = "https://example.com",
            CheckedAt = DateTime.UtcNow.AddDays(-31)
        });
        await db.SaveChangesAsync();

        var lowest = await store.GetLowestPricesLast30DaysAsync("iphone 15");

        Assert.False(lowest.ContainsKey("Teknosa"));
    }

    [Fact]
    public async Task GetQueriesDueForRefreshAsync_ReturnsNeverCheckedProductsFirst_UpToBatchSize()
    {
        await using var db = CreateContext();
        var store = new EfPriceHistoryStore(db, maxTrackedProducts: 200);

        await store.EnsureTrackedAsync("iphone 15");
        await store.EnsureTrackedAsync("samsung s24");
        await store.EnsureTrackedAsync("ipad air");

        var due = await store.GetQueriesDueForRefreshAsync(TimeSpan.FromHours(24), maxBatchSize: 2);

        Assert.Equal(2, due.Count);
    }

    [Fact]
    public async Task GetQueriesDueForRefreshAsync_ExcludesRecentlyCheckedProducts()
    {
        await using var db = CreateContext();
        var store = new EfPriceHistoryStore(db, maxTrackedProducts: 200);

        await store.EnsureTrackedAsync("iphone 15");
        await store.RecordSnapshotAsync("iphone 15", [Result("Teknosa", 30000)]);

        var due = await store.GetQueriesDueForRefreshAsync(TimeSpan.FromHours(24), maxBatchSize: 10);

        Assert.Empty(due);
    }
}
