using PriceFinderAI.Application.Services;

namespace PriceFinderAI.Tests;

public class ProductConditionCatalogTests
{
    [Theory]
    [InlineData("Yenilenmiş iPhone 15 128 GB Siyah Cep Telefonu (1 Yıl Garantili) - C Kalite")]
    [InlineData("Apple iPhone 15 128GB Refurbished")]
    [InlineData("İkinci El iPhone 15 128GB")]
    public void IsRefurbished_ReturnsTrue_ForKnownRefurbishedMarkers(string productName)
    {
        Assert.True(ProductConditionCatalog.IsRefurbished(productName));
    }

    [Fact]
    public void IsRefurbished_ReturnsFalse_ForNewProduct()
    {
        Assert.False(ProductConditionCatalog.IsRefurbished("Apple iPhone 15 128GB Mavi"));
    }
}
