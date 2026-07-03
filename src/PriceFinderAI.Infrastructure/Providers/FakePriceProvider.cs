using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Infrastructure.Providers;

public sealed class FakePriceProvider : IPriceProvider
{
    public string Name => "Fake Provider";

    public Task<IReadOnlyList<PriceResult>> SearchAsync(string productName, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PriceResult> results =
        [
            new PriceResult(
                "Teknosa",
                productName,
                44400,
                0,
                4.9,
                "https://www.teknosa.com")
        ];

        return Task.FromResult(results);
    }
}