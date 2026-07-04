using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Interfaces;

public interface IPriceHistoryStore
{
    Task EnsureTrackedAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// <paramref name="results"/> must already be filtered to TotalPrice &gt; 0 —
    /// zero-price placeholder results (e.g. a blocked scrape) would otherwise
    /// poison the 30-day lowest-price calculation.
    /// </summary>
    Task RecordSnapshotAsync(string query, IReadOnlyList<PriceResult> results, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, decimal>> GetLowestPricesLast30DaysAsync(string query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetQueriesDueForRefreshAsync(TimeSpan minAge, int maxBatchSize, CancellationToken cancellationToken = default);
}
