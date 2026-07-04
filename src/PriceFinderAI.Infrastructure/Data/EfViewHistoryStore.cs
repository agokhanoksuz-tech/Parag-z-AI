using Microsoft.EntityFrameworkCore;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Models;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Infrastructure.Data;

public sealed class EfViewHistoryStore(AppDbContext db) : IViewHistoryStore
{
    public async Task RecordViewAsync(string userId, int trackedProductId, CancellationToken cancellationToken = default)
    {
        var existing = await db.ViewedProducts.FirstOrDefaultAsync(
            v => v.UserId == userId && v.TrackedProductId == trackedProductId,
            cancellationToken);

        if (existing is not null)
        {
            existing.ViewedAt = DateTime.UtcNow;
        }
        else
        {
            db.ViewedProducts.Add(new ViewedProduct
            {
                UserId = userId,
                TrackedProductId = trackedProductId,
                ViewedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TrendingProduct>> GetRecentlyViewedAsync(string userId, int count, CancellationToken cancellationToken = default)
    {
        var viewed = await db.ViewedProducts
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.ViewedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

        var results = new List<TrendingProduct>();

        foreach (var view in viewed)
        {
            var tracked = await db.TrackedProducts.FirstOrDefaultAsync(t => t.Id == view.TrackedProductId, cancellationToken);
            if (tracked is null)
                continue;

            var cheapest = await db.PriceSnapshots
                .Where(s => s.TrackedProductId == view.TrackedProductId)
                .OrderBy(s => s.Price)
                .FirstOrDefaultAsync(cancellationToken);

            if (cheapest is not null)
            {
                results.Add(new TrendingProduct(
                    tracked.Query,
                    cheapest.ProductName,
                    cheapest.StoreName,
                    cheapest.Price,
                    cheapest.ImageUrl,
                    cheapest.Url));
            }
        }

        return results;
    }
}
