using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Services;

public sealed class ProductMatchingService
{
    public IReadOnlyList<PriceResult> FilterRelevantResults(
        string query,
        IReadOnlyList<PriceResult> results)
    {
        var queryLower = query.ToLowerInvariant();

        var filtered = results.Where(result =>
        {
            var title = result.ProductName.ToLowerInvariant();

            if (queryLower.Contains("pro max") && !title.Contains("pro max"))
                return false;

            if (queryLower.Contains("pro") && !queryLower.Contains("pro max"))
            {
                if (!title.Contains("pro"))
                    return false;

                if (title.Contains("pro max"))
                    return false;
            }

            return true;
        });

        return filtered
            .OrderBy(x => x.TotalPrice)
            .ToList();
    }
}