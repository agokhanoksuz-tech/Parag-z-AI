using PriceFinderAI.Application.Services;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Tests;

public class ProductMatchingServiceTests
{
    private readonly ProductMatchingService _sut = new();

    private static PriceResult Result(string productName) =>
        new("Test Mağaza", productName, 100, 0, 5, "https://example.com");

    [Fact]
    public void Filter_KeepsResultThatContainsAllQueryWords()
    {
        var results = new[] { Result("Apple iPhone 15 128 GB Mavi Cep Telefonu") };

        var filtered = _sut.Filter("iphone 15", results);

        Assert.Single(filtered);
    }

    [Fact]
    public void Filter_RemovesResultMissingAQueryWord()
    {
        var results = new[] { Result("Apple iPhone 14 128 GB Mavi Cep Telefonu") };

        var filtered = _sut.Filter("iphone 15", results);

        Assert.Empty(filtered);
    }

    [Theory]
    [InlineData("Apple iPhone 15 Kılıf Silikon Kırmızı")]
    [InlineData("iPhone 15 Ekran Koruyucu Cam")]
    [InlineData("iPhone 15 Şarj Adaptörü 20W")]
    public void Filter_RemovesAccessoriesMatchedByBadWords(string productName)
    {
        var results = new[] { Result(productName) };

        var filtered = _sut.Filter("iphone 15", results);

        Assert.Empty(filtered);
    }

    [Fact]
    public void Filter_RemovesKapakAccessories_EvenWhenVariantMatches()
    {
        var results = new[] { Result("iPhone 15 Pro Max Magsafe Kapak - Gold") };

        var filtered = _sut.Filter("iphone 15 pro max", results);

        Assert.Empty(filtered);
    }

    [Fact]
    public void Filter_IsCaseAndTurkishCharacterInsensitive()
    {
        var results = new[] { Result("APPLE İPHONE 15 128 GB MAVİ") };

        var filtered = _sut.Filter("İPHONE 15", results);

        Assert.Single(filtered);
    }

    [Theory]
    [InlineData("Apple iPhone 15 128 GB Siyah")]
    [InlineData("Apple iPhone 15 128GB Siyah")]
    public void Filter_MatchesStorageRegardlessOfSpacingBetweenNumberAndUnit(string productName)
    {
        var results = new[] { Result(productName) };

        var filtered = _sut.Filter("iphone 15 128gb", results);

        Assert.Single(filtered);
    }

    [Theory]
    [InlineData("Apple iPhone 15 Pro 128 GB")]
    [InlineData("Apple iPhone 15 Pro Max 256 GB")]
    [InlineData("Apple iPhone 15 Plus 128 GB")]
    public void Filter_ExcludesOtherVariants_WhenBaseModelIsRequested(string productName)
    {
        var results = new[] { Result(productName) };

        var filtered = _sut.Filter("iphone 15", results);

        Assert.Empty(filtered);
    }

    [Fact]
    public void Filter_KeepsOnlyProMax_WhenProMaxIsRequested()
    {
        var results = new[]
        {
            Result("Apple iPhone 15 Pro Max 256 GB"),
            Result("Apple iPhone 15 Pro 128 GB"),
            Result("Apple iPhone 15 128 GB")
        };

        var filtered = _sut.Filter("iphone 15 pro max", results);

        var result = Assert.Single(filtered);
        Assert.Equal("Apple iPhone 15 Pro Max 256 GB", result.ProductName);
    }

    [Fact]
    public void Filter_KeepsOnlyPlainPro_ExcludingProMax_WhenProIsRequested()
    {
        var results = new[]
        {
            Result("Apple iPhone 15 Pro Max 256 GB"),
            Result("Apple iPhone 15 Pro 128 GB")
        };

        var filtered = _sut.Filter("iphone 15 pro", results);

        var result = Assert.Single(filtered);
        Assert.Equal("Apple iPhone 15 Pro 128 GB", result.ProductName);
    }
}
