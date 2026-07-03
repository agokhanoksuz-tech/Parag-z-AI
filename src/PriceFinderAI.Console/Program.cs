using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Services;
using PriceFinderAI.Infrastructure.Providers;

Console.OutputEncoding = System.Text.Encoding.UTF8;

IReadOnlyList<IPriceProvider> providers =
[
    new FakePriceProvider(),
    new WebSearchPriceProvider()
];
var aggregator = new PriceAggregatorService(providers);

Console.WriteLine("=== Paragöz AI ===");

Console.Write("Ürün adı gir: ");
var productName = Console.ReadLine();

if (string.IsNullOrWhiteSpace(productName))
{
    Console.WriteLine("Ürün adı boş olamaz.");
    return;
}

var results = await aggregator.SearchAllAsync(productName);

Console.WriteLine("\n--- Sonuçlar ---");

foreach (var item in results)
{
    Console.WriteLine($"{item.StoreName}");
    Console.WriteLine($"Ürün: {item.ProductName}");
    Console.WriteLine($"Toplam: {item.TotalPrice:N0} TL");
    Console.WriteLine($"Link: {item.ProductUrl}");
    Console.WriteLine();
}
