using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Services;

public static class FavoritePriceDropDetector
{
    /// <summary>
    /// Compares each favorite's baseline price (last notified price, or the price at
    /// favorite time if never notified) against the freshly scraped results for the
    /// same tracked product, matched by store name. If a <see cref="Favorite.TargetPrice"/>
    /// is set, a drop only counts once the price has actually reached that target —
    /// otherwise every improvement over the baseline counts.
    /// </summary>
    public static IReadOnlyList<(Favorite Favorite, decimal NewPrice)> DetectDrops(
        IReadOnlyList<Favorite> favorites,
        IReadOnlyList<PriceResult> currentResults)
    {
        var drops = new List<(Favorite, decimal)>();

        foreach (var favorite in favorites)
        {
            var match = currentResults.FirstOrDefault(r =>
                string.Equals(r.StoreName, favorite.StoreName, StringComparison.OrdinalIgnoreCase));

            if (match is null)
                continue;

            var baseline = favorite.LastNotifiedPrice ?? favorite.PriceAtFavoriteTime;
            if (match.TotalPrice >= baseline)
                continue;

            if (favorite.TargetPrice is decimal target && match.TotalPrice > target)
                continue;

            drops.Add((favorite, match.TotalPrice));
        }

        return drops;
    }
}
