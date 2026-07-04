using System.Net;
using PriceFinderAI.Infrastructure.Providers;

namespace PriceFinderAI.Tests;

public class TeknosaProviderTests
{
    [Fact]
    public async Task SearchAsync_ReturnsZeroPriceFallback_WhenHttpCallFails()
    {
        var handler = new FakeHttpMessageHandler(_ => throw new HttpRequestException("blocked"));
        var provider = new TeknosaProvider(new HttpClient(handler));

        var results = await provider.SearchAsync("iphone 15");

        var result = Assert.Single(results);
        Assert.Equal(0, result.TotalPrice);
        Assert.Contains("erişim engellendi", result.ProductUrl);
    }

    [Fact]
    public async Task SearchAsync_ParsesPriceFromHtml()
    {
        const string html = "<div>Fiyat: 44.400,00 TL</div>";

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html)
        });

        var provider = new TeknosaProvider(new HttpClient(handler));

        var results = await provider.SearchAsync("iphone 15");

        var result = Assert.Single(results);
        Assert.Equal(44400.00m, result.TotalPrice);
    }

    [Fact]
    public async Task SearchAsync_ReturnsZeroPriceFallback_WhenNoPriceFoundInHtml()
    {
        const string html = "<div>fiyat bilgisi yok</div>";

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html)
        });

        var provider = new TeknosaProvider(new HttpClient(handler));

        var results = await provider.SearchAsync("iphone 15");

        var result = Assert.Single(results);
        Assert.Equal(0, result.TotalPrice);
        Assert.Contains("fiyat bulunamadı", result.ProductUrl);
    }
}
