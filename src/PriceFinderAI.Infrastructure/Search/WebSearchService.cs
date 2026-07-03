using System.Net.Http.Headers;
using PriceFinderAI.Application.Search;

namespace PriceFinderAI.Infrastructure.Search;

public sealed class WebSearchService : IWebSearchService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public WebSearchService(
        HttpClient httpClient,
        string apiKey,
        string baseUrl)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _baseUrl = baseUrl;
    }

    public async Task<string> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var request =
            $"{_baseUrl}?q={Uri.EscapeDataString(query)}&api_key={_apiKey}";

        return await _httpClient.GetStringAsync(request, cancellationToken);
    }
}