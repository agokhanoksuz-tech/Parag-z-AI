namespace PriceFinderAI.Application.Interfaces;

public interface IProductLinkResolver
{
    /// <summary>
    /// Google Shopping'in kendi ürün token'ından, belirtilen mağazanın gerçek
    /// (doğrudan mağaza sitesine giden) ürün linkini çözer. Bulunamazsa null döner.
    /// </summary>
    Task<string?> ResolveDirectLinkAsync(string token, string storeName, CancellationToken cancellationToken = default);
}
