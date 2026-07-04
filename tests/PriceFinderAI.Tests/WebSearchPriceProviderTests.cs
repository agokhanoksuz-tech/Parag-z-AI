using System.Net;
using PriceFinderAI.Infrastructure.Providers;

namespace PriceFinderAI.Tests;

public class WebSearchPriceProviderTests
{
    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenApiKeyMissing()
    {
        var provider = new WebSearchPriceProvider(apiKey: null, baseUrl: "https://serpapi.com/search.json");

        var results = await provider.SearchAsync("iphone 15");

        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_ParsesShoppingResultsIntoPriceResults()
    {
        const string json = """
        {
          "shopping_results": [
            {
              "title": "Apple iPhone 15 128 GB Mavi",
              "source": "Test Mağaza",
              "price": "24.423,47 TL",
              "product_link": "https://example.com/1"
            }
          ]
        }
        """;

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var provider = new WebSearchPriceProvider("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var results = await provider.SearchAsync("iphone 15");

        var result = Assert.Single(results);
        Assert.Equal("Test Mağaza", result.StoreName);
        Assert.Equal("Apple iPhone 15 128 GB Mavi", result.ProductName);
        Assert.Equal(24423.47m, result.TotalPrice);
        Assert.Equal("https://example.com/1", result.ProductUrl);
    }

    [Fact]
    public async Task SearchAsync_ExtractsThumbnailAndStoreIcon_WhenPresent()
    {
        const string json = """
        {
          "shopping_results": [
            {
              "title": "Apple iPhone 15 128 GB Mavi",
              "source": "Test Mağaza",
              "price": "24.423,47 TL",
              "product_link": "https://example.com/1",
              "thumbnail": "https://example.com/thumb.png",
              "source_icon": "https://example.com/icon.png"
            }
          ]
        }
        """;

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var provider = new WebSearchPriceProvider("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var results = await provider.SearchAsync("iphone 15");

        var result = Assert.Single(results);
        Assert.Equal("https://example.com/thumb.png", result.ImageUrl);
        Assert.Equal("https://example.com/icon.png", result.StoreIconUrl);
    }

    [Fact]
    public async Task SearchAsync_ExtractsImmersiveProductToken_WhenPresent()
    {
        const string json = """
        {
          "shopping_results": [
            {
              "title": "Apple iPhone 15 128 GB Mavi",
              "source": "Test Mağaza",
              "price": "24.423,47 TL",
              "product_link": "https://example.com/1",
              "immersive_product_page_token": "abc123token"
            }
          ]
        }
        """;

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var provider = new WebSearchPriceProvider("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var results = await provider.SearchAsync("iphone 15");

        var result = Assert.Single(results);
        Assert.Equal("abc123token", result.ImmersiveProductToken);
    }

    [Fact]
    public async Task SearchAsync_LeavesImageFieldsNull_WhenAbsent()
    {
        const string json = """
        {
          "shopping_results": [
            {
              "title": "Apple iPhone 15 128 GB Mavi",
              "source": "Test Mağaza",
              "price": "24.423,47 TL",
              "product_link": "https://example.com/1"
            }
          ]
        }
        """;

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var provider = new WebSearchPriceProvider("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var results = await provider.SearchAsync("iphone 15");

        var result = Assert.Single(results);
        Assert.Null(result.ImageUrl);
        Assert.Null(result.StoreIconUrl);
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenResponseHasNoShoppingResults()
    {
        const string json = """{"error":"Google hasn't returned any results for this query."}""";

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var provider = new WebSearchPriceProvider("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var results = await provider.SearchAsync("iphone 15");

        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_RequestsTurkishGoogleDomain()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        });

        var provider = new WebSearchPriceProvider("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        await provider.SearchAsync("iphone 15");

        Assert.NotNull(handler.LastRequest);
        Assert.Contains("google_domain=google.com.tr", handler.LastRequest!.RequestUri!.Query);
    }
}
