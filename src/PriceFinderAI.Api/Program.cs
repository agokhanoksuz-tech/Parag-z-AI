using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Services;
using PriceFinderAI.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/search", async (string product, IConfiguration configuration) =>
{
    var apiKey = configuration["SearchApi:ApiKey"];
    var baseUrl = configuration["SearchApi:BaseUrl"];

    var searchProduct = product
        .Replace("128gb", "", StringComparison.OrdinalIgnoreCase)
        .Replace("256gb", "", StringComparison.OrdinalIgnoreCase)
        .Replace("512gb", "", StringComparison.OrdinalIgnoreCase)
        .Replace("1tb", "", StringComparison.OrdinalIgnoreCase)
        .Trim();

    IReadOnlyList<IPriceProvider> providers =
    [
        new WebSearchPriceProvider(apiKey, baseUrl),
        new TeknosaProvider()
    ];

    var aggregator = new PriceAggregatorService(providers);
    var matcher = new ProductMatchingService();

    var rawResults = await aggregator.SearchAllAsync(searchProduct);

    var filteredResults = matcher
        .FilterRelevantResults(product, rawResults)
        .Where(x => x.TotalPrice > 0)
        .ToList();

    var finalResults = filteredResults.Count > 0
        ? filteredResults
        : rawResults.Where(x => x.TotalPrice > 0).ToList();
var response = finalResults.Select(x => new
{
    store = x.StoreName,
    product = x.ProductName,
    price = x.TotalPrice,
    url = x.ProductUrl
}).ToList();

var cheapest = response.OrderBy(x => x.price).FirstOrDefault();

return Results.Ok(new
{
    searchedProduct = product,
    resultCount = response.Count,
    cheapest,
    results = response
});
   

});

app.Run();