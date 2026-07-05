using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PriceFinderAI.Api.Contracts;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Options;
using PriceFinderAI.Application.Services;
using PriceFinderAI.Core.Models;
using PriceFinderAI.Infrastructure.BackgroundJobs;
using PriceFinderAI.Infrastructure.Data;
using PriceFinderAI.Infrastructure.Email;
using PriceFinderAI.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PriceHistory")));

builder.Services.AddScoped<IPriceHistoryStore>(sp =>
{
    var db = sp.GetRequiredService<AppDbContext>();
    var maxTrackedProducts = builder.Configuration.GetValue("PriceTracking:MaxTrackedProducts", 200);
    return new EfPriceHistoryStore(db, maxTrackedProducts);
});

builder.Services.AddHostedService<PriceTrackingBackgroundService>();

builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserAccountStore, EfUserAccountStore>();
builder.Services.AddScoped<AuthService>();

builder.Services.Configure<FavoritesOptions>(builder.Configuration.GetSection("Favorites"));
builder.Services.AddScoped<IFavoriteStore, EfFavoriteStore>();

builder.Services.AddScoped<IViewHistoryStore, EfViewHistoryStore>();

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddScoped<IProductLinkResolver>(sp =>
{
    var apiKey = builder.Configuration["SearchApi:ApiKey"];
    var baseUrl = builder.Configuration["SearchApi:BaseUrl"];
    return new SerpApiProductLinkResolver(apiKey, baseUrl);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "pricefinder_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    // /product-link her çağrıda ek bir SerpApi isteği tüketir (ücretsiz kota sınırlı) —
    // aşırı tıklama/otomatik istekle kotanın hızla bitmesini önlemek için sınırlanır.
    options.AddFixedWindowLimiter("product-link", limiterOptions =>
    {
        limiterOptions.PermitLimit = 20;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Tüm API endpoint'leri /api altında gruplanır — böylece tek bir servis
// içinde derlenmiş React uygulamasının statik dosyalarıyla (aşağıda,
// wwwroot'tan) çakışmadan yan yana durabilirler.
var api = app.MapGroup("/api");

api.MapGet("/trending", async (IPriceHistoryStore historyStore, CancellationToken cancellationToken) =>
{
    var trending = await historyStore.GetTrendingAsync(12, cancellationToken);

    var dtos = trending
        .Select(t => new TrendingItemDto(t.Query, t.ProductName, t.StoreName, t.Price, t.ImageUrl, t.Url, t.Rating, t.ReviewCount))
        .ToList();

    return Results.Ok(dtos);
})
.WithName("GetTrending")
.WithSummary("Ana sayfa için son taranan ürünlerden bir öneri listesi döner");

api.MapGet("/recently-viewed", async (
    ClaimsPrincipal user,
    IViewHistoryStore viewHistoryStore,
    CancellationToken cancellationToken) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
    var viewed = await viewHistoryStore.GetRecentlyViewedAsync(userId, 8, cancellationToken);

    var dtos = viewed
        .Select(t => new TrendingItemDto(t.Query, t.ProductName, t.StoreName, t.Price, t.ImageUrl, t.Url, t.Rating, t.ReviewCount))
        .ToList();

    return Results.Ok(dtos);
})
.RequireAuthorization()
.WithName("GetRecentlyViewed")
.WithSummary("Giriş yapmış kullanıcının en son aradığı ürünleri döner");

api.MapGet("/price-history", async (
    string? product,
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

    var history = await historyStore.GetPriceHistoryAsync(product, days: 30, cancellationToken);

    var dtos = history
        .Select(p => new PriceHistoryPointDto(p.Date, p.LowestPrice))
        .ToList();

    return Results.Ok(dtos);
})
.WithName("GetPriceHistory")
.WithSummary("Bir ürünün son 30 gündeki günlük en düşük fiyat serisini döner");

api.MapGet("/search", async (
    string? product,
    string? sort,
    ClaimsPrincipal user,
    IConfiguration configuration,
    ILoggerFactory loggerFactory,
    IPriceHistoryStore historyStore,
    IViewHistoryStore viewHistoryStore,
    IMemoryCache cache,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(product))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["product"] = ["Ürün adı boş olamaz."]
        });
    }

    // EnsureTrackedAsync saf bir DB sorgusu (dış ağ çağrısı yok) — önbellek
    // isabetinde bile "son bakılanlar" için tracked product id'ye ihtiyaç var.
    var trackedProductId = await historyStore.EnsureTrackedAsync(product, cancellationToken);

    // Aynı ürün kısa süre içinde tekrar aranırsa (örn. kullanıcı sıralamayı
    // değiştirip geri dönerse, ya da birden fazla kullanıcı aynı popüler ürünü
    // ararsa) canlı SerpApi/Teknosa çağrısı atlanır — ücretsiz SerpApi kotası
    // (~100/ay) çok hızlı tükenmesin diye.
    var cacheKey = $"search:{ProductMatchingService.Normalize(product)}";
    var cacheMinutes = configuration.GetValue("PriceTracking:LiveSearchCacheMinutes", 15);

    if (!cache.TryGetValue(cacheKey, out (PriceSearchOutcome Outcome, DateTime GeneratedAt) cached))
    {
        var apiKey = configuration["SearchApi:ApiKey"];
        var baseUrl = configuration["SearchApi:BaseUrl"];
        var logger = loggerFactory.CreateLogger("Search");

        IReadOnlyList<IPriceProvider> providers =
        [
            new WebSearchPriceProvider(apiKey, baseUrl),
            new TeknosaProvider()
        ];

        var pipeline = new PriceSearchPipeline();
        var freshOutcome = await pipeline.RunAsync(product, providers, logger, cancellationToken);
        cached = (freshOutcome, DateTime.UtcNow);

        cache.Set(cacheKey, cached, TimeSpan.FromMinutes(cacheMinutes));

        await historyStore.RecordSnapshotAsync(product, freshOutcome.Results, cancellationToken);
    }

    var outcome = cached.Outcome;

    if (user.Identity?.IsAuthenticated == true && trackedProductId is int viewedTrackedProductId)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
            await viewHistoryStore.RecordViewAsync(userId, viewedTrackedProductId, cancellationToken);
    }

    var last30DaysLowest = await historyStore.GetLowestPricesLast30DaysAsync(product, cancellationToken);

    var dtos = outcome.Results
        .Select(x => new SearchResultDto(
            x.StoreName,
            x.ProductName,
            x.TotalPrice,
            x.ProductUrl,
            SellerTrustCatalog.GetScore(x.StoreName),
            last30DaysLowest.TryGetValue(x.StoreName, out var lowest) ? lowest : null,
            ProductConditionCatalog.IsRefurbished(x.ProductName),
            x.ImageUrl,
            x.StoreIconUrl,
            x.ImmersiveProductToken,
            x.Rating,
            x.ReviewCount))
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
        Results: sortedResults,
        GeneratedAt: cached.GeneratedAt));
})
.WithName("SearchProducts")
.WithSummary("Bir ürünün fiyatlarını farklı mağazalarda arar")
.WithDescription("Verilen ürün adına göre yapılandırılmış sağlayıcılarda (web araması, Teknosa) fiyat arar, alakasız sonuçları eler ve en ucuzu öne çıkarır. Opsiyonel `sort` parametresi (asc/desc) sonuçların sırasını belirler.")
.Produces<SearchResponse>(StatusCodes.Status200OK)
.ProducesValidationProblem();

api.MapPost("/auth/register", async (RegisterRequest request, AuthService authService, HttpContext http, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["email"] = ["E-posta ve şifre zorunludur."]
        });
    }

    var (outcome, user) = await authService.RegisterAsync(request.Email, request.Password, cancellationToken);

    switch (outcome)
    {
        case RegisterOutcome.WeakPassword:
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["password"] = [$"Şifre en az {AuthService.MinPasswordLength} karakter olmalı."]
            });
        case RegisterOutcome.EmailAlreadyRegistered:
            return Results.Conflict(new { message = "Bu e-posta adresi zaten kayıtlı." });
    }

    await SignInUserAsync(http, user!);
    return Results.Ok(new AuthUserDto(user!.Id, user.Email));
})
.RequireRateLimiting("auth")
.WithName("Register");

api.MapPost("/auth/login", async (LoginRequest request, AuthService authService, HttpContext http, CancellationToken cancellationToken) =>
{
    var (outcome, user) = await authService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);

    if (outcome == LoginOutcome.InvalidCredentials)
        return Results.Unauthorized();

    await SignInUserAsync(http, user!);
    return Results.Ok(new AuthUserDto(user!.Id, user.Email));
})
.RequireRateLimiting("auth")
.WithName("Login");

api.MapPost("/auth/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
})
.RequireAuthorization()
.WithName("Logout");

api.MapGet("/auth/me", (ClaimsPrincipal user) =>
{
    var dto = new AuthUserDto(
        user.FindFirstValue(ClaimTypes.NameIdentifier)!,
        user.FindFirstValue(ClaimTypes.Email)!);
    return Results.Ok(dto);
})
.RequireAuthorization()
.WithName("Me");

api.MapGet("/favorites", async (
    ClaimsPrincipal user,
    IFavoriteStore favoriteStore,
    IPriceHistoryStore historyStore,
    CancellationToken cancellationToken) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
    var favorites = await favoriteStore.GetForUserAsync(userId, cancellationToken);

    var latestByTrackedProduct = new Dictionary<int, IReadOnlyDictionary<string, decimal>>();
    foreach (var trackedProductId in favorites.Select(f => f.TrackedProductId).Distinct())
    {
        latestByTrackedProduct[trackedProductId] = await historyStore.GetLatestPricesAsync(trackedProductId, cancellationToken);
    }

    var dtos = favorites
        .Select(f => new FavoriteDto(
            f.Id,
            f.StoreName,
            f.ProductName,
            f.Url,
            f.PriceAtFavoriteTime,
            latestByTrackedProduct[f.TrackedProductId].TryGetValue(f.StoreName, out var currentPrice) ? currentPrice : null,
            f.TargetPrice,
            f.CreatedAt))
        .ToList();

    return Results.Ok(dtos);
})
.RequireAuthorization()
.WithName("GetFavorites");

api.MapPost("/favorites", async (
    CreateFavoriteRequest request,
    ClaimsPrincipal user,
    IFavoriteStore favoriteStore,
    IPriceHistoryStore historyStore,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Query) || string.IsNullOrWhiteSpace(request.StoreName)
        || string.IsNullOrWhiteSpace(request.ProductName) || string.IsNullOrWhiteSpace(request.Url))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["query"] = ["Tüm alanlar zorunludur."]
        });
    }

    var trackedProductId = await historyStore.EnsureTrackedAsync(request.Query, cancellationToken);
    if (trackedProductId is null)
        return Results.Conflict(new { message = "Bu ürün şu anda takibe alınamıyor." });

    var latestPrices = await historyStore.GetLatestPricesAsync(trackedProductId.Value, cancellationToken);
    if (!latestPrices.TryGetValue(request.StoreName, out var currentPrice))
        return Results.Conflict(new { message = "Bu mağaza için henüz fiyat verisi yok." });

    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
    var favorite = new Favorite
    {
        UserId = userId,
        TrackedProductId = trackedProductId.Value,
        StoreName = request.StoreName,
        ProductName = request.ProductName,
        Url = request.Url,
        PriceAtFavoriteTime = currentPrice,
        TargetPrice = request.TargetPrice,
        CreatedAt = DateTime.UtcNow
    };

    var (outcome, created) = await favoriteStore.AddAsync(favorite, cancellationToken);

    return outcome switch
    {
        AddFavoriteOutcome.AlreadyFavorited => Results.Conflict(new { message = "Bu ürün zaten favorilerinde." }),
        AddFavoriteOutcome.LimitReached => Results.Conflict(new { message = "Favori limitine ulaşıldı." }),
        _ => Results.Ok(new FavoriteDto(
            created!.Id,
            created.StoreName,
            created.ProductName,
            created.Url,
            created.PriceAtFavoriteTime,
            currentPrice,
            created.TargetPrice,
            created.CreatedAt))
    };
})
.RequireAuthorization()
.WithName("AddFavorite");

api.MapDelete("/favorites/{id:int}", async (
    int id,
    ClaimsPrincipal user,
    IFavoriteStore favoriteStore,
    CancellationToken cancellationToken) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
    var removed = await favoriteStore.RemoveAsync(id, userId, cancellationToken);
    return removed ? Results.Ok() : Results.NotFound();
})
.RequireAuthorization()
.WithName("RemoveFavorite");

api.MapPut("/favorites/{id:int}/target-price", async (
    int id,
    SetTargetPriceRequest request,
    ClaimsPrincipal user,
    IFavoriteStore favoriteStore,
    CancellationToken cancellationToken) =>
{
    if (request.TargetPrice is decimal target && target <= 0)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["targetPrice"] = ["Hedef fiyat sıfırdan büyük olmalı."]
        });
    }

    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
    var updated = await favoriteStore.SetTargetPriceAsync(id, userId, request.TargetPrice, cancellationToken);
    return updated ? Results.Ok() : Results.NotFound();
})
.RequireAuthorization()
.WithName("SetFavoriteTargetPrice");

api.MapGet("/product-link", async (
    string? token,
    string? store,
    IProductLinkResolver linkResolver,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(store))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["token"] = ["token ve store parametreleri zorunludur."]
        });
    }

    var directLink = await linkResolver.ResolveDirectLinkAsync(token, store, cancellationToken);

    return directLink is not null
        ? Results.Ok(new ProductLinkDto(directLink))
        : Results.NotFound();
})
.RequireRateLimiting("product-link")
.WithName("GetProductLink")
.WithSummary("Google Shopping token'ından mağazanın gerçek ürün linkini çözer");

// Üretimde derlenmiş React uygulaması wwwroot'tan sunulur (bkz. Dockerfile) —
// /api dışındaki her GET isteği index.html'e düşer (istemci tarafı yönlendirme
// olmasa da, doğrudan bir alt yola girilirse 404 yerine uygulama yüklensin diye).
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();

static async Task SignInUserAsync(HttpContext http, User user)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id),
        new(ClaimTypes.Email, user.Email)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
}
