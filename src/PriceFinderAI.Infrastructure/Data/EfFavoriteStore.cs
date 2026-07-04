using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Options;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Infrastructure.Data;

public sealed class EfFavoriteStore(AppDbContext db, IOptions<FavoritesOptions> options) : IFavoriteStore
{
    public async Task<(AddFavoriteOutcome Outcome, Favorite? Favorite)> AddAsync(Favorite favorite, CancellationToken cancellationToken = default)
    {
        var alreadyExists = await db.Favorites.AnyAsync(
            f => f.UserId == favorite.UserId
                && f.TrackedProductId == favorite.TrackedProductId
                && f.StoreName == favorite.StoreName,
            cancellationToken);

        if (alreadyExists)
            return (AddFavoriteOutcome.AlreadyFavorited, null);

        var currentCount = await db.Favorites.CountAsync(f => f.UserId == favorite.UserId, cancellationToken);
        if (currentCount >= options.Value.MaxFavoritesPerUser)
            return (AddFavoriteOutcome.LimitReached, null);

        db.Favorites.Add(favorite);
        await db.SaveChangesAsync(cancellationToken);

        return (AddFavoriteOutcome.Success, favorite);
    }

    public async Task<bool> RemoveAsync(int favoriteId, string userId, CancellationToken cancellationToken = default)
    {
        var favorite = await db.Favorites.FirstOrDefaultAsync(
            f => f.Id == favoriteId && f.UserId == userId,
            cancellationToken);

        if (favorite is null)
            return false;

        db.Favorites.Remove(favorite);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<Favorite>> GetForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await db.Favorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Favorite>> GetForTrackedProductAsync(int trackedProductId, CancellationToken cancellationToken = default) =>
        await db.Favorites
            .Where(f => f.TrackedProductId == trackedProductId)
            .ToListAsync(cancellationToken);

    public async Task MarkNotifiedAsync(int favoriteId, decimal newPrice, CancellationToken cancellationToken = default)
    {
        var favorite = await db.Favorites.FirstOrDefaultAsync(f => f.Id == favoriteId, cancellationToken);
        if (favorite is null)
            return;

        favorite.LastNotifiedPrice = newPrice;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SetTargetPriceAsync(int favoriteId, string userId, decimal? targetPrice, CancellationToken cancellationToken = default)
    {
        var favorite = await db.Favorites.FirstOrDefaultAsync(
            f => f.Id == favoriteId && f.UserId == userId,
            cancellationToken);

        if (favorite is null)
            return false;

        favorite.TargetPrice = targetPrice;
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
