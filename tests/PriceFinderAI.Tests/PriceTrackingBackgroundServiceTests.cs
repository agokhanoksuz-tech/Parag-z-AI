using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Infrastructure.BackgroundJobs;
using PriceFinderAI.Infrastructure.Data;

namespace PriceFinderAI.Tests;

public class PriceTrackingBackgroundServiceTests
{
    // RunOnceAsync's per-query loop constructs the real WebSearchPriceProvider/TeknosaProvider
    // internally (no injection seam, by design — see PriceSearchPipeline usage in Program.cs),
    // so it can't be exercised here without hitting the real network. This test only verifies
    // the "nothing due" path: DI wiring + scope creation work, and an empty tracked list means
    // no provider is ever touched.
    [Fact]
    public async Task RunOnceAsync_CompletesWithoutError_WhenNoProductsAreTracked()
    {
        var services = new ServiceCollection();

        services.AddDbContext<PriceHistoryDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddScoped<IPriceHistoryStore>(sp =>
            new EfPriceHistoryStore(sp.GetRequiredService<PriceHistoryDbContext>(), maxTrackedProducts: 200));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PriceTracking:RefreshInterval"] = "1.00:00:00",
                ["PriceTracking:MaxBatchPerRun"] = "3"
            })
            .Build();

        await using var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var sut = new PriceTrackingBackgroundService(
            scopeFactory,
            configuration,
            NullLogger<PriceTrackingBackgroundService>.Instance);

        await sut.RunOnceAsync(CancellationToken.None);
    }
}
