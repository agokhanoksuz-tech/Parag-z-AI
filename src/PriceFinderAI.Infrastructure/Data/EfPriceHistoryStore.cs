using Microsoft.EntityFrameworkCore;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Services;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Infrastructure.Data;

public sealed class EfPriceHistoryStore(PriceHistoryDbContext db, int maxTrackedProducts) : IPriceHistoryStore
{
    public async Task EnsureTrackedAsync(string query, CancellationToken cancellationToken = default)
    {
        var key = ProductMatchingService.Normalize(query);

        var exists = await db.TrackedProducts.AnyAsync(t => t.Query == key, cancellationToken);
        if (exists)
            return;

        var trackedCount = await db.TrackedProducts.CountAsync(cancellationToken);
        if (trackedCount >= maxTrackedProducts)
            return;

        db.TrackedProducts.Add(new TrackedProduct
        {
            Query = key,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordSnapshotAsync(string query, IReadOnlyList<PriceResult> results, CancellationToken cancellationToken = default)
    {
        var key = ProductMatchingService.Normalize(query);

        var tracked = await db.TrackedProducts.FirstOrDefaultAsync(t => t.Query == key, cancellationToken);
        if (tracked is null)
            return;

        var now = DateTime.UtcNow;
        tracked.LastCheckedAt = now;

        foreach (var result in results)
        {
            db.PriceSnapshots.Add(new PriceSnapshot
            {
                TrackedProductId = tracked.Id,
                StoreName = result.StoreName,
                ProductName = result.ProductName,
                Price = result.TotalPrice,
                Url = result.ProductUrl,
                CheckedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, decimal>> GetLowestPricesLast30DaysAsync(string query, CancellationToken cancellationToken = default)
    {
        var key = ProductMatchingService.Normalize(query);

        var tracked = await db.TrackedProducts.FirstOrDefaultAsync(t => t.Query == key, cancellationToken);
        if (tracked is null)
            return new Dictionary<string, decimal>();

        var since = DateTime.UtcNow.AddDays(-30);

        return await db.PriceSnapshots
            .Where(s => s.TrackedProductId == tracked.Id && s.CheckedAt >= since)
            .GroupBy(s => s.StoreName)
            .Select(g => new { Store = g.Key, Lowest = g.Min(x => x.Price) })
            .ToDictionaryAsync(x => x.Store, x => x.Lowest, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetQueriesDueForRefreshAsync(TimeSpan minAge, int maxBatchSize, CancellationToken cancellationToken = default)
    {
        var threshold = DateTime.UtcNow - minAge;

        return await db.TrackedProducts
            .Where(t => t.LastCheckedAt == null || t.LastCheckedAt < threshold)
            .OrderBy(t => t.LastCheckedAt)
            .Take(maxBatchSize)
            .Select(t => t.Query)
            .ToListAsync(cancellationToken);
    }
}
