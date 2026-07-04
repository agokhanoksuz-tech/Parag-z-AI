using System.Net;
using PriceFinderAI.Infrastructure.Providers;

namespace PriceFinderAI.Tests;

public class SerpApiProductLinkResolverTests
{
    [Fact]
    public async Task ResolveDirectLinkAsync_ReturnsNull_WhenApiKeyMissing()
    {
        var resolver = new SerpApiProductLinkResolver(apiKey: null, baseUrl: "https://serpapi.com/search.json");

        var link = await resolver.ResolveDirectLinkAsync("some-token", "Teknosa");

        Assert.Null(link);
    }

    [Fact]
    public async Task ResolveDirectLinkAsync_ReturnsNull_WhenSerpApiReturnsErrorStatus()
    {
        // Geçersiz/süresi dolmuş bir token SerpApi'den 400 döndürür — bu, arayan
        // tarafın 500 yerine düzgün bir null/404 alması gereken gerçek bir senaryo.
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("""{"error":"Invalid page token."}""")
        });

        var resolver = new SerpApiProductLinkResolver("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var link = await resolver.ResolveDirectLinkAsync("invalid-token", "Teknosa");

        Assert.Null(link);
    }

    [Fact]
    public async Task ResolveDirectLinkAsync_ReturnsMatchingStoreLink()
    {
        const string json = """
        {
          "product_results": {
            "stores": [
              { "name": "Trendyol.com", "link": "https://www.trendyol.com/example-product" },
              { "name": "Teknosa", "link": "https://www.teknosa.com/example-product" }
            ]
          }
        }
        """;

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var resolver = new SerpApiProductLinkResolver("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var link = await resolver.ResolveDirectLinkAsync("some-token", "Teknosa");

        Assert.Equal("https://www.teknosa.com/example-product", link);
    }

    [Fact]
    public async Task ResolveDirectLinkAsync_MatchesStoreName_CaseInsensitivelyAndPartially()
    {
        const string json = """
        {
          "product_results": {
            "stores": [
              { "name": "MediaMarkt Pazaryeri", "link": "https://www.mediamarkt.com.tr/example-product" }
            ]
          }
        }
        """;

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var resolver = new SerpApiProductLinkResolver("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var link = await resolver.ResolveDirectLinkAsync("some-token", "MediaMarkt");

        Assert.Equal("https://www.mediamarkt.com.tr/example-product", link);
    }

    [Fact]
    public async Task ResolveDirectLinkAsync_ReturnsNull_WhenNoStoreMatches()
    {
        const string json = """
        {
          "product_results": {
            "stores": [
              { "name": "Hepsiburada", "link": "https://www.hepsiburada.com/example-product" }
            ]
          }
        }
        """;

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var resolver = new SerpApiProductLinkResolver("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var link = await resolver.ResolveDirectLinkAsync("some-token", "Teknosa");

        Assert.Null(link);
    }

    [Fact]
    public async Task ResolveDirectLinkAsync_ReturnsNull_WhenProductResultsMissing()
    {
        const string json = """{"error":"invalid token"}""";

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var resolver = new SerpApiProductLinkResolver("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var link = await resolver.ResolveDirectLinkAsync("some-token", "Teknosa");

        Assert.Null(link);
    }
}
