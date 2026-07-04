namespace PriceFinderAI.Application.Services;

public static class SellerTrustCatalog
{
    private const double UnverifiedSellerScore = 2.5;

    private static readonly (string Name, double Score)[] KnownSellers =
    [
        ("Teknosa", 4.8),
        ("Hepsiburada", 4.7),
        ("Trendyol", 4.6),
        ("Vatan Bilgisayar", 4.5),
        ("Vatan", 4.5),
        ("MediaMarkt", 4.4),
        ("N11", 4.3),
        ("Amazon", 4.6),
        ("Apple", 4.9)
    ];

    public static double GetScore(string storeName)
    {
        if (string.IsNullOrWhiteSpace(storeName))
            return UnverifiedSellerScore;

        foreach (var (name, score) in KnownSellers)
        {
            if (storeName.Contains(name, StringComparison.OrdinalIgnoreCase))
                return score;
        }

        return UnverifiedSellerScore;
    }
}
