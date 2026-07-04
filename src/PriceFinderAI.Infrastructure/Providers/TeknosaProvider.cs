using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Infrastructure.Providers;

public sealed class TeknosaProvider : IPriceProvider
{
    private readonly HttpClient _httpClient;

    public TeknosaProvider(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public string Name => "Teknosa";

    public async Task<IReadOnlyList<PriceResult>> SearchAsync(
        string productName,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://www.teknosa.com/arama/?s={WebUtility.UrlEncode(productName)}";

        string html;

        try
        {
            html = await _httpClient.GetStringAsync(url, cancellationToken);
        }
        catch
        {
            return
            [
                new PriceResult(
                    "Teknosa",
                    productName,
                    0,
                    0,
                    0,
                    $"{url} - erişim engellendi")
            ];
        }

        var priceMatch = Regex.Match(html, @"(\d{1,3}(\.\d{3})+|\d+),?\d*\s*TL");

        if (!priceMatch.Success)
        {
            return
            [
                new PriceResult(
                    "Teknosa",
                    productName,
                    0,
                    0,
                    0,
                    $"{url} - fiyat bulunamadı")
            ];
        }

        var rawPrice = priceMatch.Value
            .Replace("TL", "")
            .Replace(".", "")
            .Replace(",", ".")
            .Trim();

        decimal.TryParse(
            rawPrice,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var price);

        return
        [
            new PriceResult(
                "Teknosa",
                productName,
                price,
                0,
                4.5,
                url)
        ];
    }
}