using PriceFinderAI.Application.Services;

namespace PriceFinderAI.Tests;

public class SellerTrustCatalogTests
{
    [Theory]
    [InlineData("Teknosa")]
    [InlineData("Hepsiburada")]
    [InlineData("Trendyol")]
    [InlineData("Vatan Bilgisayar")]
    public void GetScore_ReturnsHighScore_ForKnownSellers(string storeName)
    {
        var score = SellerTrustCatalog.GetScore(storeName);

        Assert.True(score >= 4.0, $"{storeName} için beklenen yüksek güven puanı alınamadı: {score}");
    }

    [Fact]
    public void GetScore_ReturnsLowDefaultScore_ForUnknownSeller()
    {
        var score = SellerTrustCatalog.GetScore("Wireless Source");

        Assert.True(score < 3.0);
    }

    [Fact]
    public void GetScore_MatchesKnownSellerAsSubstring()
    {
        var score = SellerTrustCatalog.GetScore("Teknosa Mağaza - Kadıköy Şubesi");

        Assert.Equal(SellerTrustCatalog.GetScore("Teknosa"), score);
    }

    [Fact]
    public void GetScore_IsCaseInsensitive()
    {
        Assert.Equal(SellerTrustCatalog.GetScore("teknosa"), SellerTrustCatalog.GetScore("TEKNOSA"));
    }
}
