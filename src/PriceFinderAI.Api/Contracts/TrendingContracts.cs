namespace PriceFinderAI.Api.Contracts;

public sealed record TrendingItemDto(
    string Query,
    string ProductName,
    string StoreName,
    decimal Price,
    string? ImageUrl,
    string Url,
    double? Rating,
    int? ReviewCount);
