namespace PriceFinderAI.Core.Models;

public sealed record PriceResult(
    string StoreName,
    string ProductName,
    decimal Price,
    decimal ShippingPrice,
    double TrustScore,
    string ProductUrl,
    string? ImageUrl = null,
    string? StoreIconUrl = null,
    string? ImmersiveProductToken = null,
    double? Rating = null,
    int? ReviewCount = null
)
{
    public decimal TotalPrice => Price + ShippingPrice;
}