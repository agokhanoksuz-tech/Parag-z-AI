namespace PriceFinderAI.Api.Contracts;

public sealed record CreateFavoriteRequest(string Query, string StoreName, string ProductName, string Url, decimal? TargetPrice = null);

public sealed record SetTargetPriceRequest(decimal? TargetPrice);

public sealed record FavoriteDto(
    int Id,
    string StoreName,
    string ProductName,
    string Url,
    decimal PriceAtFavoriteTime,
    decimal? CurrentPrice,
    decimal? TargetPrice,
    DateTime CreatedAt);
