using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Interfaces;

public enum AddFavoriteOutcome
{
    Success,
    AlreadyFavorited,
    LimitReached
}

public interface IFavoriteStore
{
    Task<(AddFavoriteOutcome Outcome, Favorite? Favorite)> AddAsync(Favorite favorite, CancellationToken cancellationToken = default);

    /// <summary>Returns false if no favorite with this id belongs to the given user.</summary>
    Task<bool> RemoveAsync(int favoriteId, string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Favorite>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Favorite>> GetForTrackedProductAsync(int trackedProductId, CancellationToken cancellationToken = default);

    Task MarkNotifiedAsync(int favoriteId, decimal newPrice, CancellationToken cancellationToken = default);

    /// <summary>Returns false if no favorite with this id belongs to the given user.</summary>
    Task<bool> SetTargetPriceAsync(int favoriteId, string userId, decimal? targetPrice, CancellationToken cancellationToken = default);
}
