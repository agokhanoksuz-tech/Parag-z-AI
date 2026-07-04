namespace PriceFinderAI.Api.Contracts;

public sealed record SearchResultDto(string Store, string Product, decimal Price, string Url);

public sealed record SearchResponse(
    string SearchedProduct,
    string UsedSearchProduct,
    int ResultCount,
    SearchResultDto? Cheapest,
    IReadOnlyList<SearchResultDto> Results);
