using System.Globalization;
using System.Text.Json;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Infrastructure.Providers;

public sealed class WebSearchPriceProvider : IPriceProvider
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string? _baseUrl;

    public WebSearchPriceProvider(string? apiKey, string? baseUrl, HttpClient? httpClient = null)
    {
        _apiKey = apiKey;
        _baseUrl = baseUrl;
        _httpClient = httpClient ?? new HttpClient();
    }

    public string Name => "Web Search Provider";

    public async Task<IReadOnlyList<PriceResult>> SearchAsync(
        string productName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_baseUrl))
        {
            return [];
        }

        var requestUrl =
            $"{_baseUrl}?engine=google_shopping&q={Uri.EscapeDataString(productName)}&google_domain=google.com.tr&gl=tr&hl=tr&api_key={_apiKey}";

        var json = await _httpClient.GetStringAsync(requestUrl, cancellationToken);

        using var document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty("shopping_results", out var results))
        {
            return [];
        }

        var priceResults = new List<PriceResult>();

        foreach (var item in results.EnumerateArray().Take(30))
        {
            var title = GetFirstAvailableString(item, "title");

            if (string.IsNullOrWhiteSpace(title))
                title = productName;

            var source = GetFirstAvailableString(item, "source", "merchant", "seller");
            if (string.IsNullOrWhiteSpace(source))
                source = "Bilinmeyen Mağaza";

            var link = GetFirstAvailableString(item, "product_link", "link", "serpapi_product_api");

            var priceText = GetFirstAvailableString(item, "price", "extracted_price");

            var price = ParsePrice(priceText);

            var imageUrl = GetFirstAvailableString(item, "thumbnail");
            var storeIconUrl = GetFirstAvailableString(item, "source_icon");
            var immersiveToken = GetFirstAvailableString(item, "immersive_product_page_token");
            var (rating, reviewCount) = ParseRating(item);

            priceResults.Add(new PriceResult(
                source,
                title,
                price,
                0,
                0,
                link,
                string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl,
                string.IsNullOrWhiteSpace(storeIconUrl) ? null : storeIconUrl,
                string.IsNullOrWhiteSpace(immersiveToken) ? null : immersiveToken,
                rating,
                reviewCount));
        }

        return priceResults
            .OrderBy(x => x.TotalPrice)
            .ToList();
    }

    private static string GetFirstAvailableString(JsonElement item, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (!item.TryGetProperty(propertyName, out var property))
                continue;

            if (property.ValueKind == JsonValueKind.String)
            {
                var value = property.GetString();

                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            if (property.ValueKind == JsonValueKind.Number)
            {
                return property.ToString();
            }
        }

        return "";
    }

    /// <summary>
    /// SerpApi'nin rating/reviews alanları bazen anlamsız değerler taşıyor (örn.
    /// 21 milyar yorum) — bu yüzden mantıklı bir aralığa uymayan değerler
    /// sessizce null'a düşürülür; yorum sayısı yoksa/saçmaysa puan da gösterilmez.
    /// </summary>
    private static (double? Rating, int? ReviewCount) ParseRating(JsonElement item)
    {
        var ratingText = GetFirstAvailableString(item, "rating");
        var reviewsText = GetFirstAvailableString(item, "reviews");

        double? rating = double.TryParse(ratingText, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedRating)
            && parsedRating is >= 0 and <= 5
            ? parsedRating
            : null;

        int? reviewCount = long.TryParse(reviewsText, out var parsedReviews) && parsedReviews is >= 0 and <= 500_000
            ? (int)parsedReviews
            : null;

        if (reviewCount is null)
            rating = null;

        return (rating, reviewCount);
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