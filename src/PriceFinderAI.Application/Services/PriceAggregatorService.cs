using Microsoft.Extensions.Logging;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Services;

public sealed class PriceAggregatorService
{
    private readonly IReadOnlyList<IPriceProvider> _providers;
    private readonly ILogger? _logger;

    public PriceAggregatorService(IEnumerable<IPriceProvider> providers, ILogger? logger = null)
    {
        _providers = providers.ToList();
        _logger = logger;
    }

    public async Task<IReadOnlyList<PriceResult>> SearchAllAsync(string productName)
    {
        var allResults = new List<PriceResult>();

        foreach (var provider in _providers)
        {
            try
            {
                var results = await provider.SearchAsync(productName);
                allResults.AddRange(results);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "{Provider} sağlayıcısından sonuç alınamadı", provider.Name);
            }
        }

        return allResults
            .OrderBy(x => x.TotalPrice)
            .ToList();
    }
}