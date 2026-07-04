using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Services;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Tests;

public class PriceAggregatorServiceTests
{
    private sealed class StubProvider(string name, IReadOnlyList<PriceResult> results) : IPriceProvider
    {
        public string Name => name;

        public Task<IReadOnlyList<PriceResult>> SearchAsync(
            string productName,
            CancellationToken cancellationToken = default)
            => Task.FromResult(results);
    }

    private sealed class ThrowingProvider : IPriceProvider
    {
        public string Name => "Throwing";

        public Task<IReadOnlyList<PriceResult>> SearchAsync(
            string productName,
            CancellationToken cancellationToken = default)
            => throw new HttpRequestException("boom");
    }

    private static PriceResult Result(string store, decimal price) =>
        new(store, "iPhone 15", price, 0, 5, "https://example.com");

    [Fact]
    public async Task SearchAllAsync_MergesAndSortsResultsAcrossProvidersByTotalPrice()
    {
        var providerA = new StubProvider("A", [Result("A", 30000)]);
        var providerB = new StubProvider("B", [Result("B", 10000), Result("B2", 20000)]);

        var aggregator = new PriceAggregatorService([providerA, providerB]);

        var results = await aggregator.SearchAllAsync("iphone 15");

        Assert.Equal(3, results.Count);
        Assert.Equal(10000, results[0].TotalPrice);
        Assert.Equal(20000, results[1].TotalPrice);
        Assert.Equal(30000, results[2].TotalPrice);
    }

    [Fact]
    public async Task SearchAllAsync_SkipsProviderThatThrows_AndReturnsOthers()
    {
        var workingProvider = new StubProvider("Working", [Result("Working", 15000)]);
        var throwingProvider = new ThrowingProvider();

        var aggregator = new PriceAggregatorService([throwingProvider, workingProvider]);

        var results = await aggregator.SearchAllAsync("iphone 15");

        var result = Assert.Single(results);
        Assert.Equal("Working", result.StoreName);
    }
}
