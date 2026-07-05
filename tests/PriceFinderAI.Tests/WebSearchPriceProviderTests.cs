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
    public async Task SearchAsync_ExtractsRatingAndReviewCount_WhenPlausible()
    {
        const string json = """
        {
          "shopping_results": [
            {
              "title": "Apple iPhone 15 128 GB Mavi",
              "source": "Test Mağaza",
              "price": "24.423,47 TL",
              "product_link": "https://example.com/1",
              "rating": 4.6,
              "reviews": 1250
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
        Assert.Equal(4.6, result.Rating);
        Assert.Equal(1250, result.ReviewCount);
    }

    [Fact]
    public async Task SearchAsync_DropsRatingAndReviewCount_WhenReviewCountIsImplausiblyHigh()
    {
        // Gerçek bir veri hatası: SerpApi bazen anlamsız yorum sayıları döndürüyor
        // (örn. 21 milyar) — bu tür değerler sessizce null'a düşürülmeli.
        const string json = """
        {
          "shopping_results": [
            {
              "title": "Apple iPhone 15 128 GB Mavi",
              "source": "Test Mağaza",
              "price": "24.423,47 TL",
              "product_link": "https://example.com/1",
              "rating": 4.6,
              "reviews": 21000000000
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
        Assert.Null(result.Rating);
        Assert.Null(result.ReviewCount);
    }

    [Fact]
    public async Task SearchAsync_LeavesRatingNull_WhenReviewCountAbsent()
    {
        const string json = """
        {
          "shopping_results": [
            {
              "title": "Apple iPhone 15 128 GB Mavi",
              "source": "Test Mağaza",
              "price": "24.423,47 TL",
              "product_link": "https://example.com/1",
              "rating": 4.6
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
        Assert.Null(result.Rating);
        Assert.Null(result.ReviewCount);
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
    public async Task SearchAsync_RetriesWithNoCache_WhenFirstAttemptReturnsNoShoppingResults()
    {
        // Gerçek bir davranış: Google Shopping aynı sorgu için bazen tutarsız
        // şekilde 0 sonuç dönüyor (SerpApi'nin önbelleğe aldığı boş bir yanıt
        // olabilir) — ilk deneme boşsa no_cache ile bir kez daha denenmeli.
        const string emptyJson = """{"error":"Google hasn't returned any results for this query."}""";
        const string secondAttemptJson = """
        {
          "shopping_results": [
            {
              "title": "Poco X8 5G 128 GB",
              "source": "Test Mağaza",
              "price": "12.999,00 TL",
              "product_link": "https://example.com/1"
            }
          ]
        }
        """;

        var callCount = 0;
        var handler = new FakeHttpMessageHandler(request =>
        {
            callCount++;
            var isRetry = request.RequestUri!.Query.Contains("no_cache=true");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(isRetry ? secondAttemptJson : emptyJson)
            };
        });

        var provider = new WebSearchPriceProvider("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var results = await provider.SearchAsync("poco x8");

        Assert.Equal(2, callCount);
        var result = Assert.Single(results);
        Assert.Equal("Poco X8 5G 128 GB", result.ProductName);
    }

    [Fact]
    public async Task SearchAsync_RetriesTwice_WhenFirstNoCacheAttemptIsAlsoEmpty()
    {
        // Canlıda gözlemlendi: art arda İKİ ayrı no_cache denemesinin ikisi de
        // 0 sonuç döndü, üçüncü deneme 40 sonuç verdi — tek bir yeniden deneme
        // her zaman yetmiyor.
        const string emptyJson = """{"error":"Google hasn't returned any results for this query."}""";
        const string successJson = """
        {
          "shopping_results": [
            {
              "title": "Poco X8 5G 128 GB",
              "source": "Test Mağaza",
              "price": "12.999,00 TL",
              "product_link": "https://example.com/1"
            }
          ]
        }
        """;

        var callCount = 0;
        var handler = new FakeHttpMessageHandler(_ =>
        {
            callCount++;
            var content = callCount < 3 ? emptyJson : successJson;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) };
        });

        var provider = new WebSearchPriceProvider("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var results = await provider.SearchAsync("poco x8");

        Assert.Equal(3, callCount);
        Assert.Single(results);
    }

    [Fact]
    public async Task SearchAsync_GivesUpAfterExhaustingRetries_WhenAllAttemptsReturnEmpty()
    {
        const string emptyJson = """{"error":"Google hasn't returned any results for this query."}""";

        var callCount = 0;
        var handler = new FakeHttpMessageHandler(_ =>
        {
            callCount++;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(emptyJson) };
        });

        var provider = new WebSearchPriceProvider("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var results = await provider.SearchAsync("poco x8");

        Assert.Equal(3, callCount);
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_RetriesWithNoCache_WhenFirstAttemptThrows()
    {
        // Gerçek bir canlı davranış: ilk SerpApi çağrısı bazen zaman aşımına
        // uğruyor (bkz. HttpClient.Timeout). Bu durum boş sonuçla aynı şekilde
        // ele alınıp no_cache ile yeniden denenmeli, üst katmana hemen hata
        // fırlatılmamalı.
        const string secondAttemptJson = """
        {
          "shopping_results": [
            {
              "title": "Poco X8 5G 128 GB",
              "source": "Test Mağaza",
              "price": "12.999,00 TL",
              "product_link": "https://example.com/1"
            }
          ]
        }
        """;

        var callCount = 0;
        var handler = new FakeHttpMessageHandler(request =>
        {
            callCount++;
            if (!request.RequestUri!.Query.Contains("no_cache=true"))
                throw new HttpRequestException("simulated timeout");

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(secondAttemptJson) };
        });

        var provider = new WebSearchPriceProvider("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        var results = await provider.SearchAsync("poco x8");

        Assert.Equal(2, callCount);
        var result = Assert.Single(results);
        Assert.Equal("Poco X8 5G 128 GB", result.ProductName);
    }

    [Fact]
    public async Task SearchAsync_DoesNotRetry_WhenFirstAttemptHasResults()
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

        var callCount = 0;
        var handler = new FakeHttpMessageHandler(_ =>
        {
            callCount++;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
        });

        var provider = new WebSearchPriceProvider("fake-key", "https://serpapi.com/search.json", new HttpClient(handler));

        await provider.SearchAsync("iphone 15");

        Assert.Equal(1, callCount);
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
