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
        // Sağlayıcılar paralel çalıştırılır — sırayla çalıştırıldığında (eskiden
        // olduğu gibi) yavaş/yanıt vermeyen tek bir sağlayıcı (örn. Teknosa'nın
        // bot korumasından dönen istekler) toplam arama süresini gereksiz yere
        // uzatıyordu.
        var resultsPerProvider = await Task.WhenAll(_providers.Select(provider => SearchSingleProviderAsync(provider, productName)));

        return resultsPerProvider
            .SelectMany(results => results)
            .OrderBy(x => x.TotalPrice)
            .ToList();
    }

    private async Task<IReadOnlyList<PriceResult>> SearchSingleProviderAsync(IPriceProvider provider, string productName)
    {
        try
        {
            return await provider.SearchAsync(productName);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "{Provider} sağlayıcısından sonuç alınamadı", provider.Name);
            return [];
        }
    }
}