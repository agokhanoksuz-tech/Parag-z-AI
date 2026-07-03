using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Infrastructure.Providers;

public sealed class FakePriceProvider : IPriceProvider
{
    public string Name => "Fake Provider";

    public Task<IReadOnlyList<PriceResult>> SearchAsync(
        string productName,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PriceResult> results =
        [
            new("Teknosa", productName, 44400, 0, 4.9, "https://www.teknosa.com"),
            new("Hepsiburada", productName, 45000, 0, 4.8, "https://www.hepsiburada.com"),
            new("Trendyol", productName, 44500, 40, 4.5, "https://www.trendyol.com"),
            new("N11", productName, 43900, 70, 4.1, "https://www.n11.com")
        ];

        return Task.FromResult(results);
    }
}