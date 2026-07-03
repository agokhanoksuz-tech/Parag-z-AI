namespace PriceFinderAI.Application.Search;

public interface IWebSearchService
{
    Task<IReadOnlyList<WebSearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default);
}

public sealed record WebSearchResult(
    string Title,
    string Url,
    string Snippet
);