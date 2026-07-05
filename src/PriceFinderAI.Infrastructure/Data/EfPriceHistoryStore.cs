using Microsoft.EntityFrameworkCore;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Models;
using PriceFinderAI.Application.Services;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Infrastructure.Data;

public sealed class EfPriceHistoryStore(AppDbContext db, int maxTrackedProducts) : IPriceHistoryStore
{
    public async Task<int?> EnsureTrackedAsync(string query, CancellationToken cancellationToken = default)
    {
        var key = ProductMatchingService.Normalize(query);

        var existing = await db.TrackedProducts.FirstOrDefaultAsync(t => t.Query == key, cancellationToken);
        if (existing is not null)
            return existing.Id;

        var trackedCount = await db.TrackedProducts.CountAsync(cancellationToken);
        if (trackedCount >= maxTrackedProducts)
            return null;

        var tracked = new TrackedProduct
        {
            Query = key,
            CreatedAt = DateTime.UtcNow
        };
        db.TrackedProducts.Add(tracked);
        await db.SaveChangesAsync(cancellationToken);

        return tracked.Id;
    }

    public async Task<int?> RecordSnapshotAsync(string query, IReadOnlyList<PriceResult> results, CancellationToken cancellationToken = default)
    {
        var key = ProductMatchingService.Normalize(query);

        var tracked = await db.TrackedProducts.FirstOrDefaultAsync(t => t.Query == key, cancellationToken);
        if (tracked is null)
            return null;

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
                ImageUrl = result.ImageUrl,
                Rating = result.Rating,
                ReviewCount = result.ReviewCount,
                CheckedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        return tracked.Id;
    }

    public async Task<IReadOnlyDictionary<string, decimal>> GetLatestPricesAsync(int trackedProductId, CancellationToken cancellationToken = default)
    {
        // Client-side grouping on purpose: GroupBy(...).Select(g => g.OrderByDescending(...).First())
        // is not reliably translatable across relational providers, and snapshot volume per
        // tracked product is small (bounded by the background refresh cadence).
        var snapshots = await db.PriceSnapshots
            .Where(s => s.TrackedProductId == trackedProductId)
            .ToListAsync(cancellationToken);

        return snapshots
            .GroupBy(s => s.StoreName)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.CheckedAt).First().Price);
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

    public async Task<IReadOnlyList<TrendingProduct>> GetTrendingAsync(int count, CancellationToken cancellationToken = default)
    {
        // "En son kontrol edilenler" sırasıyla deterministik seçim yapılınca,
        // takip edilen ürün havuzu küçük olduğu sürece (henüz az kullanıcı
        // arattıysa) sayfa her yenilendiğinde birebir aynı ürünler
        // görünüyordu. Zaten taranmış, gerçek veriye sahip ürünler arasından
        // rastgele bir alt küme seçilir — hem sayfa yenilemede hem gün gün
        // gerçek çeşitlilik sağlar, uydurma bir veri eklenmez.
        var eligible = await db.TrackedProducts
            .Where(t => t.LastCheckedAt != null)
            .ToListAsync(cancellationToken);

        var selected = eligible
            .OrderBy(_ => Random.Shared.Next())
            .Take(count)
            .ToList();

        var trending = new List<TrendingProduct>();

        foreach (var tracked in selected)
        {
            // Client-side sıralama (bkz. GetLatestPricesAsync'teki not) — SQLite'ın
            // decimal-as-text sütunları için EF_DECIMAL collation'ı bazı değer
            // kombinasyonlarında yanlış sıra üretiyor, en ucuzu C# tarafında seçmek
            // güvenilir tek yol.
            var snapshots = await db.PriceSnapshots
                .Where(s => s.TrackedProductId == tracked.Id)
                .ToListAsync(cancellationToken);

            var cheapest = snapshots.OrderBy(s => s.Price).FirstOrDefault();

            if (cheapest is not null)
            {
                trending.Add(new TrendingProduct(
                    tracked.Query,
                    cheapest.ProductName,
                    cheapest.StoreName,
                    cheapest.Price,
                    cheapest.ImageUrl,
                    cheapest.Url,
                    cheapest.Rating,
                    cheapest.ReviewCount));
            }
        }

        return trending;
    }

    public async Task<IReadOnlyList<TrendingProduct>> SearchTrackedAsync(string queryPrefix, int count, CancellationToken cancellationToken = default)
    {
        var normalizedPrefix = ProductMatchingService.Normalize(queryPrefix);

        if (string.IsNullOrWhiteSpace(normalizedPrefix))
            return [];

        var trackedProducts = await db.TrackedProducts
            .Where(t => t.Query.Contains(normalizedPrefix))
            .OrderByDescending(t => t.LastCheckedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

        var matches = new List<TrendingProduct>();

        foreach (var tracked in trackedProducts)
        {
            var snapshots = await db.PriceSnapshots
                .Where(s => s.TrackedProductId == tracked.Id)
                .ToListAsync(cancellationToken);

            var cheapest = snapshots.OrderBy(s => s.Price).FirstOrDefault();

            if (cheapest is not null)
            {
                matches.Add(new TrendingProduct(
                    tracked.Query,
                    cheapest.ProductName,
                    cheapest.StoreName,
                    cheapest.Price,
                    cheapest.ImageUrl,
                    cheapest.Url,
                    cheapest.Rating,
                    cheapest.ReviewCount));
            }
        }

        return matches;
    }

    public async Task<IReadOnlyList<PricePoint>> GetPriceHistoryAsync(string query, int days, CancellationToken cancellationToken = default)
    {
        var key = ProductMatchingService.Normalize(query);

        var tracked = await db.TrackedProducts.FirstOrDefaultAsync(t => t.Query == key, cancellationToken);
        if (tracked is null)
            return [];

        var since = DateTime.UtcNow.AddDays(-days);

        var snapshots = await db.PriceSnapshots
            .Where(s => s.TrackedProductId == tracked.Id && s.CheckedAt >= since)
            .ToListAsync(cancellationToken);

        return snapshots
            .GroupBy(s => s.CheckedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new PricePoint(g.Key, g.Min(s => s.Price)))
            .ToList();
    }
}
