using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Interfaces;

public interface IPriceProvider
{
    string Name { get; }

    Task<IReadOnlyList<PriceResult>> SearchAsync(
        string productName,
        CancellationToken cancellationToken = default
    );
}