using System.Text.RegularExpressions;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Services;

public sealed class ProductMatchingService
{
    private static readonly string[] BadWords =
    [
        "replika", "replica", "çakma", "cakma",
        "kılıf", "kilif", "case", "kapak",
        "cam", "koruyucu", "ekran koruyucu",
        "şarj", "sarj", "adaptör", "adapter",
        "kablo", "kulaklık", "airpods",
        "yd", "yurt dışı", "yurtdışı", "yurt disi", "yurtdisi",
        "imei", "kayıtsız", "kayitsiz"
    ];

    /// <summary>
    /// Gerçek bir veri hatasıydı: "laptop" aratıldığında Türkçe ilanların çoğu
    /// "dizüstü bilgisayar" veya sadece "bilgisayar" diyor, "laptop" kelimesi
    /// hiç geçmiyor — birebir kelime eşleşmesi bu ilanların tamamını eliyordu
    /// (40 gerçek sonuçtan 7'ye düşüyordu). Her sorgu kelimesi için, kendisi
    /// veya eş anlamlılarından biri başlıkta geçerse eşleşme kabul edilir.
    /// </summary>
    private static readonly Dictionary<string, string[]> SynonymGroups = new()
    {
        ["laptop"] = ["notebook", "dizustu", "bilgisayar"],
        ["notebook"] = ["laptop", "dizustu", "bilgisayar"],
        ["tv"] = ["televizyon"],
        ["televizyon"] = ["tv"],
        ["telefon"] = ["akilli telefon", "cep telefonu"],
    };

    public IReadOnlyList<PriceResult> Filter(string query, IReadOnlyList<PriceResult> products)
    {
        var normalizedQuery = Normalize(query);
        var queryWords = normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var queryVariant = ExtractVariant(normalizedQuery);

        // Sorgunun kendisi bir "kötü kelime" içeriyorsa (örn. kullanıcı doğrudan
        // "airpods" veya "kulaklık" arıyorsa), o kelime bu arama için istenmeyen
        // bir aksesuar göstergesi değil, doğrudan arananın kendisidir — yoksa
        // "airpods pro" araması kendi sonuçlarını bloke eder.
        var applicableBadWords = BadWords
            .Where(bad => !normalizedQuery.Contains(Normalize(bad)))
            .ToArray();

        var strictMatches = products
            .Where(p => IsValidProduct(p, queryWords, queryVariant, applicableBadWords, enforceVariant: true))
            .ToList();

        if (strictMatches.Count > 0)
            return strictMatches;

        // Gerçek bir veri hatası: "Poco X8" gibi bazı modeller piyasada sadece
        // "Pro" varyantıyla satılıyor — base model hiç yok. Varyant eşleşmesi
        // katı tutulduğunda bu durumda TÜM sonuçlar elenip "bulunamadı"
        // gösteriliyordu, oysa ürün gerçekten satışta. Kesin varyant eşleşmesi
        // hiç sonuç vermediğinde, kelime/kötü-kelime kontrollerini koruyarak
        // varyant kısıtlaması gevşetilir — kartlarda gerçek başlık zaten
        // gösterildiği için (örn. "Poco X8 Pro"), kullanıcı ne aldığını görür.
        return products
            .Where(p => IsValidProduct(p, queryWords, queryVariant, applicableBadWords, enforceVariant: false))
            .ToList();
    }

    private static bool IsValidProduct(
        PriceResult product,
        string[] queryWords,
        PhoneVariant queryVariant,
        string[] badWords,
        bool enforceVariant)
    {
        var title = Normalize(product.ProductName);
        var titleWords = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (badWords.Any(bad => MatchesBadWordPhrase(titleWords, Normalize(bad))))
            return false;

        foreach (var word in queryWords)
        {
            if (!title.Contains(word) && !(SynonymGroups.TryGetValue(word, out var synonyms) && synonyms.Any(title.Contains)))
                return false;
        }

        // "iphone 15" arandığında "iphone 15 pro max" gibi farklı bir
        // varyantın sızmaması için: sorgu belirli bir varyant istiyorsa
        // (veya hiç istemiyorsa) başlık aynı varyanda ait olmalı — ama sadece
        // bu kontrol tüm sonuçları elemediği sürece (bkz. Filter).
        if (enforceVariant && ExtractVariant(title) != queryVariant)
            return false;

        return true;
    }

    /// <summary>
    /// Kötü kelime öbeğinin başlıkta kelime bazlı geçip geçmediğini kontrol eder.
    /// Düz Contains kullanılsaydı "kablo" kötü kelimesi "kablosuz" (wireless)
    /// içinde de eşleşirdi; ama saf kelime-eşleşmesi de "kılıf" ile "kılıfı" gibi
    /// Türkçe çekim ekli halleri (aynı kelime, farklı ek) kaçırırdı. Bu yüzden her
    /// kelime için "önekle başlar ama '-sız/-siz/-suz/-süz' olumsuzluk ekiyle
    /// devam etmez" kuralı uygulanır (bkz. <see cref="MatchesBadWord"/>).
    /// </summary>
    private static bool MatchesBadWordPhrase(string[] titleWords, string normalizedPhrase)
    {
        var phraseWords = normalizedPhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (phraseWords.Length == 0)
            return false;

        for (var i = 0; i + phraseWords.Length <= titleWords.Length; i++)
        {
            var isMatch = true;

            for (var j = 0; j < phraseWords.Length; j++)
            {
                if (!MatchesBadWord(titleWords[i + j], phraseWords[j]))
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
                return true;
        }

        return false;
    }

    private static bool MatchesBadWord(string titleWord, string badWord)
    {
        if (!titleWord.StartsWith(badWord, StringComparison.Ordinal))
            return false;

        var suffix = titleWord[badWord.Length..];

        // Normalize() sonrası Türkçe olumsuzluk eki ("-sız/-siz/-suz/-süz") iki
        // forma iner: "siz" ve "suz" (ı->i, ü->u dönüşümü nedeniyle).
        return !suffix.StartsWith("siz", StringComparison.Ordinal)
            && !suffix.StartsWith("suz", StringComparison.Ordinal);
    }

    private enum PhoneVariant
    {
        Base,
        Plus,
        Pro,
        ProMax
    }

    private static PhoneVariant ExtractVariant(string normalizedText)
    {
        if (normalizedText.Contains("pro max"))
            return PhoneVariant.ProMax;

        if (normalizedText.Contains("pro"))
            return PhoneVariant.Pro;

        if (normalizedText.Contains("plus"))
            return PhoneVariant.Plus;

        return PhoneVariant.Base;
    }

    public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // ToLowerInvariant() büyük Türkçe "İ" harfini küçültmüyor (olduğu gibi
        // bırakıyor); aşağıdaki regex ASCII olmayan bu karakteri sonradan
        // sessizce siler ve kelimeyi ikiye böler (örn. "İkinci" -> "kinci").
        // Bu yüzden küçültmeden önce açıkça "i"ye çevriliyor.
        text = text.Replace("İ", "i");

        text = text.ToLowerInvariant();

        text = text
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c");

        text = Regex.Replace(text, @"[^a-z0-9\s]", " ");
        text = Regex.Replace(text, @"\s+", " ").Trim();

        // "128 gb" ve "128gb" aynı ürünü ifade eder; boşluklu/bitişik yazım
        // farkı yüzünden alt-dize eşleşmesinin kırılmaması için birleştir.
        text = Regex.Replace(text, @"(\d+)\s+(gb|tb)\b", "$1$2");

        return text;
    }
}