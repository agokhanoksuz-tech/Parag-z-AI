using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Services;
using PriceFinderAI.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/search", async (string product, IConfiguration configuration) =>
{
    var apiKey = configuration["SearchApi:ApiKey"];
    var baseUrl = configuration["SearchApi:BaseUrl"];

    IReadOnlyList<IPriceProvider> providers =
    [
        new WebSearchPriceProvider(apiKey, baseUrl),
        new TeknosaProvider()
    ];

    var aggregator = new PriceAggregatorService(providers);

    var searchProduct = product
        .Replace("128gb", "", StringComparison.OrdinalIgnoreCase)
        .Replace("128 gb", "", StringComparison.OrdinalIgnoreCase)
        .Replace("256gb", "", StringComparison.OrdinalIgnoreCase)
        .Replace("256 gb", "", StringComparison.OrdinalIgnoreCase)
        .Replace("512gb", "", StringComparison.OrdinalIgnoreCase)
        .Replace("512 gb", "", StringComparison.OrdinalIgnoreCase)
        .Replace("1tb", "", StringComparison.OrdinalIgnoreCase)
        .Replace("1 tb", "", StringComparison.OrdinalIgnoreCase)
        .Trim();

    var rawResults = await aggregator.SearchAllAsync(searchProduct);
var matcher = new ProductMatchingService();

var finalResults = matcher
    .FilterRelevantResults(product, rawResults)
    .Where(x => x.TotalPrice > 0)
    .ToList();
   
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
        usedSearchProduct = searchProduct,
        resultCount = response.Count,
        cheapest,
        results = response
    });
});

app.Run();