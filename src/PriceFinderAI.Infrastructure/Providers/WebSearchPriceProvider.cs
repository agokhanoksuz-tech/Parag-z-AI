using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Infrastructure.Providers;

public sealed class WebSearchPriceProvider : IPriceProvider
{
    private readonly HttpClient _httpClient = new();

    public string Name => "Search API Provider";

    public async Task<IReadOnlyList<PriceResult>> SearchAsync(
        string productName,
        CancellationToken cancellationToken = default)
    {
        // İlk test: internete gerçekten çıkabiliyor muyuz?
        var response = await _httpClient.GetAsync(
            "https://example.com",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        IReadOnlyList<PriceResult> results =
        [
            new(
                "HTTP Test Başarılı",
                productName,
                1,
                0,
                5.0,
                "https://example.com")
        ];

        return results;
    }
}