namespace PriceFinderAI.Core.Models;

public sealed class ViewedProduct
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public int TrackedProductId { get; set; }
    public DateTime ViewedAt { get; set; }
}
