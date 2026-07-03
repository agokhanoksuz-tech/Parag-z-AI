using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Infrastructure.Providers;

public sealed class WebSearchPriceProvider : IPriceProvider
{
    private readonly HttpClient _httpClient = new();
    private readonly string? _apiKey;
    private readonly string? _baseUrl;

    public WebSearchPriceProvider(string? apiKey, string? baseUrl)
    {
        _apiKey = apiKey;
        _baseUrl = baseUrl;
    }

    public string Name => "Web Search Provider";

    public async Task<IReadOnlyList<PriceResult>> SearchAsync(
        string productName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return
            [
                new(
                    "Web Search Hazır",
                    productName,
                    0,
                    0,
                    0,
                    "API key henüz eklenmedi")
            ];
        }

        var response = await _httpClient.GetAsync(
            _baseUrl,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return
        [
            new(
                "Web Search API Bağlantısı Başarılı",
                productName,
                1,
                0,
                5,
                _baseUrl ?? "")
        ];
    }
}