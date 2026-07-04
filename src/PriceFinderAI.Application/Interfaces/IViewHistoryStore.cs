using PriceFinderAI.Application.Models;

namespace PriceFinderAI.Application.Interfaces;

public interface IViewHistoryStore
{
    /// <summary>Upserts the view timestamp for this (user, tracked product) pair.</summary>
    Task RecordViewAsync(string userId, int trackedProductId, CancellationToken cancellationToken = default);

    /// <summary>Most recently viewed distinct products for this user, each represented by its cheapest known snapshot.</summary>
    Task<IReadOnlyList<TrendingProduct>> GetRecentlyViewedAsync(string userId, int count, CancellationToken cancellationToken = default);
}
