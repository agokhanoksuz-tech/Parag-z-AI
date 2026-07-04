using System.Text.RegularExpressions;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Services;

public sealed class ProductMatchingService
{
    private static readonly string[] BadWords =
    [
        "replika", "replica", "çakma", "cakma",
        "kılıf", "kilif", "case",
        "cam", "koruyucu", "ekran koruyucu",
        "şarj", "sarj", "adaptör", "adapter",
        "kablo", "kulaklık", "airpods",
        "yd", "yurt dışı", "yurtdışı", "yurt disi", "yurtdisi",
        "imei", "kayıtsız", "kayitsiz"
    ];

    public IReadOnlyList<PriceResult> Filter(string query, IReadOnlyList<PriceResult> products)
    {
        var normalizedQuery = Normalize(query);
        var queryWords = normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return products
            .Where(p => IsValidProduct(p, queryWords))
            .ToList();
    }

    private static bool IsValidProduct(PriceResult product, string[] queryWords)
    {
        var title = Normalize(product.ProductName);

        if (BadWords.Any(bad => title.Contains(Normalize(bad))))
            return false;

        foreach (var word in queryWords)
        {
            if (!title.Contains(word))
                return false;
        }

        return true;
    }

    private static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        text = text.ToLowerInvariant();

        text = text
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c");

        text = Regex.Replace(text, @"[^a-z0-9\s]", " ");
        text = Regex.Replace(text, @"\s+", " ").Trim();

        return text;
    }
}