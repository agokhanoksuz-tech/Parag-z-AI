namespace PriceFinderAI.Application.Services;

public static class ProductConditionCatalog
{
    private static readonly string[] RefurbishedMarkers =
    [
        "yenilenmis", "refurbished", "ikinci el"
    ];

    public static bool IsRefurbished(string productName)
    {
        var normalized = ProductMatchingService.Normalize(productName);

        return RefurbishedMarkers.Any(marker => normalized.Contains(ProductMatchingService.Normalize(marker)));
    }
}
