namespace PriceFinderAI.Core.Models;

public sealed class PriceSnapshot
{
    public int Id { get; set; }
    public int TrackedProductId { get; set; }
    public required string StoreName { get; set; }
    public required string ProductName { get; set; }
    public decimal Price { get; set; }
    public required string Url { get; set; }
    public string? ImageUrl { get; set; }
    public double? Rating { get; set; }
    public int? ReviewCount { get; set; }
    public DateTime CheckedAt { get; set; }
}
