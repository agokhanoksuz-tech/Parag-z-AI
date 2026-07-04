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
    public void Filter_IsCaseAndTurkishCharacterInsensitive()
    {
        var results = new[] { Result("APPLE İPHONE 15 128 GB MAVİ") };

        var filtered = _sut.Filter("İPHONE 15", results);

        Assert.Single(filtered);
    }
}
