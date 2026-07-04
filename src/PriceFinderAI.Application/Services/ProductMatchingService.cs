using System.Text.RegularExpressions;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Services;

public sealed class ProductMatchingService
{
    private static readonly string[] AccessoryWords =
    [
        "kılıf", "kilif", "case",
        "cam", "ekran koruyucu", "koruyucu",
        "şarj", "sarj", "adaptör", "adapter",
        "kablo", "kulaklık", "stand"
    ];

    public IReadOnlyList<PriceResult> FilterRelevantResults(string query, IReadOnlyList<PriceResult> results)
    {
        var queryLower = query.ToLowerInvariant();
        var requestedStorage = ExtractStorage(queryLower);
        var requestedModelNumbers = ExtractModelNumbers(queryLower);

        return results
            .Where(result =>
            {
                var title = result.ProductName.ToLowerInvariant();

                if (AccessoryWords.Any(word => title.Contains(word)))
                    return false;

                if (queryLower.Contains("pro max") && !title.Contains("pro max"))
                    return false;

                if (queryLower.Contains("pro") && !queryLower.Contains("pro max"))
                {
                    if (!title.Contains("pro"))
                        return false;

                    if (title.Contains("pro max"))
                        return false;
                }

                foreach (var number in requestedModelNumbers)
                {
                    if (!title.Contains(number))
                        return false;
                }

                return true;
            })
            .OrderByDescending(x => StorageScore(x.ProductName, requestedStorage))
            .ThenBy(x => x.TotalPrice)
            .ToList();
    }

    private static int? ExtractStorage(string text)
    {
        var normalized = text.ToLowerInvariant().Replace(" ", "");

        if (normalized.Contains("128gb")) return 128;
        if (normalized.Contains("256gb")) return 256;
        if (normalized.Contains("512gb")) return 512;
        if (normalized.Contains("1tb")) return 1024;

        return null;
    }

    private static IReadOnlyList<string> ExtractModelNumbers(string text)
    {
        return Regex.Matches(text, @"\b\d{1,2}\b")
            .Select(match => match.Value)
            .ToList();
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