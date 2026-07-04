using Microsoft.EntityFrameworkCore;
using PriceFinderAI.Api.Contracts;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Services;
using PriceFinderAI.Infrastructure.BackgroundJobs;
using PriceFinderAI.Infrastructure.Data;
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

builder.Services.AddDbContext<PriceHistoryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PriceHistory")));

builder.Services.AddScoped<IPriceHistoryStore>(sp =>
{
    var db = sp.GetRequiredService<PriceHistoryDbContext>();
    var maxTrackedProducts = builder.Configuration.GetValue("PriceTracking:MaxTrackedProducts", 200);
    return new EfPriceHistoryStore(db, maxTrackedProducts);
});

builder.Services.AddHostedService<PriceTrackingBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PriceHistoryDbContext>();
    db.Database.Migrate();
}

app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/search", async (
    string? product,
    string? sort,
    IConfiguration configuration,
    ILoggerFactory loggerFactory,
    IPriceHistoryStore historyStore,
    CancellationToken cancellationToken) =>
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

    var pipeline = new PriceSearchPipeline();
    var outcome = await pipeline.RunAsync(product, providers, logger, cancellationToken);

    await historyStore.EnsureTrackedAsync(product, cancellationToken);
    await historyStore.RecordSnapshotAsync(product, outcome.Results, cancellationToken);
    var last30DaysLowest = await historyStore.GetLowestPricesLast30DaysAsync(product, cancellationToken);

    var dtos = outcome.Results
        .Select(x => new SearchResultDto(
            x.StoreName,
            x.ProductName,
            x.TotalPrice,
            x.ProductUrl,
            SellerTrustCatalog.GetScore(x.StoreName),
            last30DaysLowest.TryGetValue(x.StoreName, out var lowest) ? lowest : null,
            ProductConditionCatalog.IsRefurbished(x.ProductName)))
        .ToList();

    // Sıralama parametresinden bağımsız hesaplanır — "en ucuz" sort=desc'te
    // yanlışlıkla en pahalıya dönüşmesin.
    var cheapest = dtos.OrderBy(x => x.Price).FirstOrDefault();

    var sortedResults = string.Equals(sort, "desc", StringComparison.OrdinalIgnoreCase)
        ? dtos.OrderByDescending(x => x.Price).ToList()
        : dtos.OrderBy(x => x.Price).ToList();

    return Results.Ok(new SearchResponse(
        SearchedProduct: product,
        UsedSearchProduct: outcome.WidenedQuery,
        ResultCount: sortedResults.Count,
        Cheapest: cheapest,
        Results: sortedResults));
})
.WithName("SearchProducts")
.WithSummary("Bir ürünün fiyatlarını farklı mağazalarda arar")
.WithDescription("Verilen ürün adına göre yapılandırılmış sağlayıcılarda (web araması, Teknosa) fiyat arar, alakasız sonuçları eler ve en ucuzu öne çıkarır. Opsiyonel `sort` parametresi (asc/desc) sonuçların sırasını belirler.")
.Produces<SearchResponse>(StatusCodes.Status200OK)
.ProducesValidationProblem();

app.Run();
