using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Services;

public sealed class PriceAggregatorService
{
    private readonly IReadOnlyList<IPriceProvider> _providers;

    public PriceAggregatorService(IEnumerable<IPriceProvider> providers)
    {
        _providers = providers.ToList();
    }

    public async Task<IReadOnlyList<PriceResult>> SearchAllAsync(string productName)
    {
        var allResults = new List<PriceResult>();

        foreach (var provider in _providers)
        {
            var results = await provider.SearchAsync(productName);
            allResults.AddRange(results);
        }

        return allResults
            .OrderBy(x => x.TotalPrice)
            .ToList();
    }
}