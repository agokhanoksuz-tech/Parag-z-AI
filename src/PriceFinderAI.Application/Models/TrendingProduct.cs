namespace PriceFinderAI.Application.Models;

public sealed record TrendingProduct(
    string Query,
    string ProductName,
    string StoreName,
    decimal Price,
    string? ImageUrl,
    string Url,
    double? Rating = null,
    int? ReviewCount = null);
