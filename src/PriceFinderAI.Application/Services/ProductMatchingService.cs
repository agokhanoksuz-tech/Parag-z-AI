using System.Text.RegularExpressions;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Services;

public sealed class ProductMatchingService
{
    private static readonly string[] BadWords =
    [
        "replika", "replica", "çakma", "cakma",
        "kılıf", "kilif", "case", "kapak",
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
        var queryVariant = ExtractVariant(normalizedQuery);

        return products
            .Where(p => IsValidProduct(p, queryWords, queryVariant))
            .ToList();
    }

    private static bool IsValidProduct(PriceResult product, string[] queryWords, PhoneVariant queryVariant)
    {
        var title = Normalize(product.ProductName);

        if (BadWords.Any(bad => title.Contains(Normalize(bad))))
            return false;

        foreach (var word in queryWords)
        {
            if (!title.Contains(word))
                return false;
        }

        // "iphone 15" arandığında "iphone 15 pro max" gibi farklı bir
        // varyantın sızmaması için: sorgu belirli bir varyant istiyorsa
        // (veya hiç istemiyorsa) başlık aynı varyanda ait olmalı.
        if (ExtractVariant(title) != queryVariant)
            return false;

        return true;
    }

    private enum PhoneVariant
    {
        Base,
        Plus,
        Pro,
        ProMax
    }

    private static PhoneVariant ExtractVariant(string normalizedText)
    {
        if (normalizedText.Contains("pro max"))
            return PhoneVariant.ProMax;

        if (normalizedText.Contains("pro"))
            return PhoneVariant.Pro;

        if (normalizedText.Contains("plus"))
            return PhoneVariant.Plus;

        return PhoneVariant.Base;
    }

    public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // ToLowerInvariant() büyük Türkçe "İ" harfini küçültmüyor (olduğu gibi
        // bırakıyor); aşağıdaki regex ASCII olmayan bu karakteri sonradan
        // sessizce siler ve kelimeyi ikiye böler (örn. "İkinci" -> "kinci").
        // Bu yüzden küçültmeden önce açıkça "i"ye çevriliyor.
        text = text.Replace("İ", "i");

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

        // "128 gb" ve "128gb" aynı ürünü ifade eder; boşluklu/bitişik yazım
        // farkı yüzünden alt-dize eşleşmesinin kırılmaması için birleştir.
        text = Regex.Replace(text, @"(\d+)\s+(gb|tb)\b", "$1$2");

        return text;
    }
}