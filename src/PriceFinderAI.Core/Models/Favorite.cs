namespace PriceFinderAI.Core.Models;

public sealed class Favorite
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public int TrackedProductId { get; set; }
    public required string StoreName { get; set; }
    public required string ProductName { get; set; }
    public required string Url { get; set; }
    public decimal PriceAtFavoriteTime { get; set; }
    public decimal? LastNotifiedPrice { get; set; }
    /// <summary>Belirtilirse, fiyat her düşüşte değil yalnızca bu değere inince/altına düşünce bildirim gönderilir.</summary>
    public decimal? TargetPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
