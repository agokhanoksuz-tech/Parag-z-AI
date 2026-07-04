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

    public IReadOnlyList<PriceResult> FilterRelevantResults(
        string query,
        IReadOnlyList<PriceResult> results)
    {
        var queryLower = query.ToLowerInvariant();
        var requestedStorage = ExtractStorage(queryLower);
        var requestedNumbers = ExtractNumbers(queryLower);

        var filtered = results.Where(result =>
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

            foreach (var number in requestedNumbers)
            {
                if (!title.Contains(number))
                    return false;
            }

            if (requestedStorage is not null)
            {
                var titleStorage = ExtractStorage(title);

                if (titleStorage is not null && titleStorage != requestedStorage)
                    return false;
            }

            return true;
        });

        return filtered
            .OrderBy(x => x.TotalPrice)
            .ToList();
    }

    private static int? ExtractStorage(string text)
    {
        if (text.Contains("128 gb") || text.Contains("128gb"))
            return 128;

        if (text.Contains("256 gb") || text.Contains("256gb"))
            return 256;

        if (text.Contains("512 gb") || text.Contains("512gb"))
            return 512;

        if (text.Contains("1 tb") || text.Contains("1tb"))
            return 1024;

        return null;
    }

    private static IReadOnlyList<string> ExtractNumbers(string text)
    {
        return Regex.Matches(text, @"\b\d{1,2}\b")
            .Select(match => match.Value)
            .ToList();
    }
}