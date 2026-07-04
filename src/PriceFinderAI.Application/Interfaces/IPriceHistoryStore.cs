using PriceFinderAI.Application.Models;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Interfaces;

public interface IPriceHistoryStore
{
    /// <summary>Returns the tracked product id, or null if the tracking cap (MaxTrackedProducts) was reached.</summary>
    Task<int?> EnsureTrackedAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// <paramref name="results"/> must already be filtered to TotalPrice &gt; 0 —
    /// zero-price placeholder results (e.g. a blocked scrape) would otherwise
    /// poison the 30-day lowest-price calculation. Returns the tracked product id,
    /// or null if the query isn't tracked.
    /// </summary>
    Task<int?> RecordSnapshotAsync(string query, IReadOnlyList<PriceResult> results, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, decimal>> GetLowestPricesLast30DaysAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>Most recent recorded price per store for a tracked product — used to resolve a favorite's baseline price.</summary>
    Task<IReadOnlyDictionary<string, decimal>> GetLatestPricesAsync(int trackedProductId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetQueriesDueForRefreshAsync(TimeSpan minAge, int maxBatchSize, CancellationToken cancellationToken = default);

    /// <summary>Most recently checked tracked products, each represented by its cheapest known snapshot — feeds the homepage.</summary>
    Task<IReadOnlyList<TrendingProduct>> GetTrendingAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>Daily lowest price across all stores, for the last <paramref name="days"/> days — feeds the price history chart.</summary>
    Task<IReadOnlyList<PricePoint>> GetPriceHistoryAsync(string query, int days, CancellationToken cancellationToken = default);
}
