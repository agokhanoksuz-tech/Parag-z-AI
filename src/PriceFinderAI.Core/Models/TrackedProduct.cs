namespace PriceFinderAI.Core.Models;

public sealed class TrackedProduct
{
    public int Id { get; set; }
    public required string Query { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastCheckedAt { get; set; }
}
