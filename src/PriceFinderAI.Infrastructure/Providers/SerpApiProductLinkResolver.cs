using System.Text.Json;
using PriceFinderAI.Application.Interfaces;

namespace PriceFinderAI.Infrastructure.Providers;

public sealed class SerpApiProductLinkResolver : IProductLinkResolver
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string? _baseUrl;

    public SerpApiProductLinkResolver(string? apiKey, string? baseUrl, HttpClient? httpClient = null)
    {
        _apiKey = apiKey;
        _baseUrl = baseUrl;
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<string?> ResolveDirectLinkAsync(string token, string storeName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_baseUrl) || string.IsNullOrWhiteSpace(token))
            return null;

        var requestUrl =
            $"{_baseUrl}?engine=google_immersive_product&page_token={Uri.EscapeDataString(token)}&api_key={_apiKey}";

        string json;

        try
        {
            json = await _httpClient.GetStringAsync(requestUrl, cancellationToken);
        }
        catch (HttpRequestException)
        {
            // Geçersiz/süresi dolmuş token, SerpApi tarafında hata (400/5xx) — arayan
            // taraf zaten orijinal Google Shopping linkine düşecek şekilde tasarlandı.
            return null;
        }

        using var document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty("product_results", out var productResults))
            return null;

        if (!productResults.TryGetProperty("stores", out var stores) || stores.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var store in stores.EnumerateArray())
        {
            if (!store.TryGetProperty("name", out var nameProperty))
                continue;

            var name = nameProperty.GetString();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var matches = name.Contains(storeName, StringComparison.OrdinalIgnoreCase)
                || storeName.Contains(name, StringComparison.OrdinalIgnoreCase);

            if (!matches)
                continue;

            if (store.TryGetProperty("link", out var linkProperty) && linkProperty.ValueKind == JsonValueKind.String)
            {
                var link = linkProperty.GetString();
                if (!string.IsNullOrWhiteSpace(link))
                    return link;
            }
        }

        return null;
    }
}
