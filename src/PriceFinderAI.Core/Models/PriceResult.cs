namespace PriceFinderAI.Core.Models;

public sealed record PriceResult(
    string StoreName,
    string ProductName,
    decimal Price,
    decimal ShippingPrice,
    double TrustScore,
    string ProductUrl
)
{
    public decimal TotalPrice => Price + ShippingPrice;
}