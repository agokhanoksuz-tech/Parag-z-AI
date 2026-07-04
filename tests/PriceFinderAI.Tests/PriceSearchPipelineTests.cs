using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Services;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Tests;

public class PriceSearchPipelineTests
{
    private sealed class RecordingProvider(IReadOnlyList<PriceResult> results) : IPriceProvider
    {
        public string? LastQuery { get; private set; }
        public string Name => "Recording";

        public Task<IReadOnlyList<PriceResult>> SearchAsync(
            string productName,
            CancellationToken cancellationToken = default)
        {
            LastQuery = productName;
            return Task.FromResult(results);
        }
    }

    private static PriceResult Result(string productName, decimal price) =>
        new("Test Mağaza", productName, price, 0, 0, "https://example.com");

    [Fact]
    public async Task RunAsync_WidensQueryByStrippingStorageTokens_ForSearchButFiltersByOriginalQuery()
    {
        var provider = new RecordingProvider([Result("Apple iPhone 15 128GB Mavi", 30000)]);

        var pipeline = new PriceSearchPipeline();
        var outcome = await pipeline.RunAsync("iphone 15 128gb", [provider]);

        Assert.Equal("iphone 15", provider.LastQuery);
        Assert.Equal("iphone 15", outcome.WidenedQuery);
        Assert.Single(outcome.Results);
    }

    [Fact]
    public async Task RunAsync_DropsZeroPriceResults()
    {
        var provider = new RecordingProvider([Result("iPhone 15", 0)]);

        var pipeline = new PriceSearchPipeline();
        var outcome = await pipeline.RunAsync("iphone 15", [provider]);

        Assert.Empty(outcome.Results);
    }

    [Fact]
    public async Task RunAsync_FallsBackToOriginalQuery_WhenWideningWouldEmptyIt()
    {
        var provider = new RecordingProvider([Result("128 GB", 30000)]);

        var pipeline = new PriceSearchPipeline();
        var outcome = await pipeline.RunAsync("128gb", [provider]);

        Assert.Equal("128gb", provider.LastQuery);
        Assert.Equal("128gb", outcome.WidenedQuery);
    }
}
