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
        "kablo", "kulaklık"
    ];

    public IReadOnlyList<PriceResult> FilterRelevantResults(
        string query,
        IReadOnlyList<PriceResult> results)
    {
        var q = Normalize(query);
        var requestedModel = ExtractIphoneModel(q);
        var wantsProMax = q.Contains("pro max");
        var wantsPro = q.Contains("pro") && !wantsProMax;
        var requestedStorage = ExtractStorage(q);

        return results
            .Where(x =>
            {
                var title = Normalize(x.ProductName);

                if (BadWords.Any(title.Contains))
                    return false;

                if (requestedModel is not null && ExtractIphoneModel(title) != requestedModel)
                    return false;

                if (wantsProMax && !title.Contains("pro max"))
                    return false;

                if (wantsPro)
                {
                    if (!title.Contains("pro"))
                        return false;

                    if (title.Contains("pro max"))
                        return false;
                }

                return true;
            })
            .OrderByDescending(x => StorageScore(Normalize(x.ProductName), requestedStorage))
            .ThenBy(x => x.TotalPrice)
            .ToList();
    }

    private static string Normalize(string text)
    {
        return text
            .ToLowerInvariant()
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c");
    }

    private static int? ExtractIphoneModel(string text)
    {
        var match = Regex.Match(text, @"iphone\s*(\d{2})");

        if (!match.Success)
            return null;

        return int.TryParse(match.Groups[1].Value, out var model)
            ? model
            : null;
    }

    private static int? ExtractStorage(string text)
    {
        var normalized = text.Replace(" ", "");

        if (normalized.Contains("128gb")) return 128;
        if (normalized.Contains("256gb")) return 256;
        if (normalized.Contains("512gb")) return 512;
        if (normalized.Contains("1tb")) return 1024;

        return null;
    }

    private static int StorageScore(string title, int? requestedStorage)
    {
        if (requestedStorage is null)
            return 0;

        var titleStorage = ExtractStorage(title);

        if (titleStorage == requestedStorage)
            return 2;

        if (titleStorage is null)
            return 1;

        return 0;
    }
}