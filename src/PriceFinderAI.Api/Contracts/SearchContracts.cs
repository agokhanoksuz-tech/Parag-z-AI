namespace PriceFinderAI.Api.Contracts;

public sealed record SearchResultDto(
    string Store,
    string Product,
    decimal Price,
    string Url,
    double TrustScore,
    decimal? Last30DaysLowestPrice,
    bool IsRefurbished,
    string? ImageUrl,
    string? StoreIconUrl,
    string? ImmersiveProductToken,
    double? Rating,
    int? ReviewCount);

public sealed record SearchResponse(
    string SearchedProduct,
    string UsedSearchProduct,
    int ResultCount,
    SearchResultDto? Cheapest,
    IReadOnlyList<SearchResultDto> Results,
    DateTime GeneratedAt);
