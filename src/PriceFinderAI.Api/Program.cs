using System.Linq;
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

app.UseHttpsRedirection();

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
    var matcher = new ProductMatchingService();

    var results = await aggregator.SearchAllAsync(product);

    results = matcher
        .FilterRelevantResults(product, results)
        .Where(x => x.TotalPrice > 0)
        .ToList();

    return Results.Ok(results);
});

app.Run();