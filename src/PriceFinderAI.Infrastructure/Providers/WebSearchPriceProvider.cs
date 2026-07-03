using System.Text.Json;
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
        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_baseUrl))
        {
            return
            [
                new PriceResult(
                    "Web Search",
                    productName,
                    0,
                    0,
                    0,
                    "API key henüz eklenmedi")
            ];
        }

        var requestUrl =
            $"{_baseUrl}?engine=google_shopping&q={Uri.EscapeDataString(productName)}&gl=tr&hl=tr&api_key={_apiKey}";

        var json = await _httpClient.GetStringAsync(requestUrl, cancellationToken);

        using var document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty("shopping_results", out var results))
        {
            return
            [
                new PriceResult(
                    "Web Search",
                    productName,
                    0,
                    0,
                    0,
                    "Sonuç bulunamadı")
            ];
        }

        var priceResults = new List<PriceResult>();

        foreach (var item in results.EnumerateArray().Take(10))
        {
            var title = item.TryGetProperty("title", out var titleProp)
                ? titleProp.GetString() ?? productName
                : productName;

            var source = item.TryGetProperty("source", out var sourceProp)
                ? sourceProp.GetString() ?? "Bilinmeyen Mağaza"
                : "Bilinmeyen Mağaza";

            var link = item.TryGetProperty("link", out var linkProp)
                ? linkProp.GetString() ?? ""
                : "";

            var priceText = item.TryGetProperty("price", out var priceProp)
                ? priceProp.GetString() ?? ""
                : "";

            var price = ParsePrice(priceText);

            priceResults.Add(new PriceResult(
                source,
                title,
                price,
                0,
                0,
                link));
        }

        return priceResults
            .OrderBy(x => x.TotalPrice)
            .ToList();
    }

    private static decimal ParsePrice(string text)
    {
        var clean = text
            .Replace("TL", "")
            .Replace("₺", "")
            .Replace(".", "")
            .Replace(",", ".")
            .Trim();

        return decimal.TryParse(
            clean,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var price)
            ? price
            : 0;
    }
}