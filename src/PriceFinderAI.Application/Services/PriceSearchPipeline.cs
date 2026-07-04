using Microsoft.Extensions.Logging;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Services;

public sealed record PriceSearchOutcome(string WidenedQuery, IReadOnlyList<PriceResult> Results);

public sealed class PriceSearchPipeline
{
    private static readonly string[] StorageTokens =
    [
        "128gb", "128 gb",
        "256gb", "256 gb",
        "512gb", "512 gb",
        "1tb", "1 tb"
    ];

    public async Task<PriceSearchOutcome> RunAsync(
        string query,
        IEnumerable<IPriceProvider> providers,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        var widenedQuery = WidenForSearch(query);
        var aggregator = new PriceAggregatorService(providers, logger);
        var rawResults = await aggregator.SearchAllAsync(widenedQuery);

        var matcher = new ProductMatchingService();
        var filteredResults = matcher
            .Filter(query, rawResults)
            .Where(x => x.TotalPrice > 0)
            .ToList();

        return new PriceSearchOutcome(widenedQuery, filteredResults);
    }

    public static string WidenForSearch(string query)
    {
        var widened = query;

        foreach (var token in StorageTokens)
        {
            widened = widened.Replace(token, "", StringComparison.OrdinalIgnoreCase);
        }

        widened = widened.Trim();

        return string.IsNullOrWhiteSpace(widened) ? query : widened;
    }
}
