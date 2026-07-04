using PriceFinderAI.Api.Contracts;
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

app.MapGet("/search", async (string? product, IConfiguration configuration, ILoggerFactory loggerFactory) =>
{
    if (string.IsNullOrWhiteSpace(product))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["product"] = ["Ürün adı boş olamaz."]
        });
    }

    var apiKey = configuration["SearchApi:ApiKey"];
    var baseUrl = configuration["SearchApi:BaseUrl"];
    var logger = loggerFactory.CreateLogger("Search");

    IReadOnlyList<IPriceProvider> providers =
    [
        new WebSearchPriceProvider(apiKey, baseUrl),
        new TeknosaProvider()
    ];

    var aggregator = new PriceAggregatorService(providers, logger);

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

    if (string.IsNullOrWhiteSpace(searchProduct))
    {
        searchProduct = product;
    }

    var rawResults = await aggregator.SearchAllAsync(searchProduct);
    var matcher = new ProductMatchingService();

    var finalResults = matcher
        .Filter(product, rawResults)
        .Where(x => x.TotalPrice > 0)
        .Select(x => new SearchResultDto(x.StoreName, x.ProductName, x.TotalPrice, x.ProductUrl))
        .ToList();

    var cheapest = finalResults.OrderBy(x => x.Price).FirstOrDefault();

    return Results.Ok(new SearchResponse(
        SearchedProduct: product,
        UsedSearchProduct: searchProduct,
        ResultCount: finalResults.Count,
        Cheapest: cheapest,
        Results: finalResults));
})
.WithName("SearchProducts")
.WithSummary("Bir ürünün fiyatlarını farklı mağazalarda arar")
.WithDescription("Verilen ürün adına göre yapılandırılmış sağlayıcılarda (web araması, Teknosa) fiyat arar, alakasız sonuçları eler ve en ucuzu öne çıkarır.")
.Produces<SearchResponse>(StatusCodes.Status200OK)
.ProducesValidationProblem();

app.Run();