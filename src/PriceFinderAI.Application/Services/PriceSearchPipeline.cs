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
        IReadOnlyList<PriceResult> filteredResults = matcher
            .Filter(query, rawResults)
            .Where(x => x.TotalPrice > 0)
            .ToList();

        filteredResults = ExcludePriceOutliers(filteredResults);

        return new PriceSearchOutcome(widenedQuery, filteredResults);
    }

    /// <summary>
    /// Aksesuar/kılıf ilanları başlıkta hiçbir anahtar kelimeyi tutturmadan
    /// kelime bazlı filtreyi geçebiliyor (örn. "Casedoit - Good Ahead"),
    /// ama fiyatları gerçek bir telefondan onlarca kat düşük oluyor. En az
    /// 3 sonuç varsa medyanın çok altındaki (implausibly ucuz) sonuçları ele.
    /// </summary>
    private static IReadOnlyList<PriceResult> ExcludePriceOutliers(IReadOnlyList<PriceResult> results)
    {
        const decimal minimumFractionOfMedian = 0.25m;

        if (results.Count < 3)
            return results;

        var sortedPrices = results.Select(x => x.TotalPrice).OrderBy(p => p).ToList();
        var mid = sortedPrices.Count / 2;
        var median = sortedPrices.Count % 2 == 0
            ? (sortedPrices[mid - 1] + sortedPrices[mid]) / 2
            : sortedPrices[mid];

        var threshold = median * minimumFractionOfMedian;

        return results.Where(x => x.TotalPrice >= threshold).ToList();
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
