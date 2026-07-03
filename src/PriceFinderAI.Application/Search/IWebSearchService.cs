namespace PriceFinderAI.Application.Search;

public interface IWebSearchService
{
    Task<string> SearchAsync(string query, CancellationToken cancellationToken = default);
}
