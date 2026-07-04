using System.Globalization;
using System.Net;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Services;

public static class PriceDropEmailTemplate
{
    public static (string Subject, string HtmlBody) Build(Favorite favorite, decimal newPrice)
    {
        var productName = WebUtility.HtmlEncode(favorite.ProductName);
        var storeName = WebUtility.HtmlEncode(favorite.StoreName);
        var url = WebUtility.HtmlEncode(favorite.Url);
        var priceText = newPrice.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));

        var subject = $"Fiyat düştü: {favorite.ProductName}";
        var htmlBody = $"""
            <p><strong>{productName}</strong> ürününün <strong>{storeName}</strong> mağazasındaki fiyatı düştü.</p>
            <p>Yeni fiyat: <strong>{priceText} TL</strong></p>
            <p><a href="{url}">Ürüne git</a></p>
            """;

        return (subject, htmlBody);
    }
}
