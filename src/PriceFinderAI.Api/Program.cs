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

    IReadOnlyList<IPriceProvider> providers =
    [
        new WebSearchPriceProvider(apiKey, baseUrl)
    ];

    var aggregator = new PriceAggregatorService(providers);
    var results = await aggregator.SearchAllAsync(product);

    return Results.Ok(new
    {
        apiKeyStatus = string.IsNullOrWhiteSpace(apiKey) ? "YOK" : "VAR",
        baseUrl,
        count = results.Count,
        results
    });
});

app.Run();