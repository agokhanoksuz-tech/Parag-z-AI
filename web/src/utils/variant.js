// Sadece yaygın depolama kademeleriyle eşleşir (64/128/256/512 GB, 1/2/4 TB) —
// rastgele "\d+GB" eşleştirmek RAM miktarıyla (8GB, 16GB gibi) karışıyor,
// özellikle dizüstü bilgisayar başlıklarında ("M2 8GB 256GB SSD" gibi).
const CAPACITY_PATTERN = /\b(64|128|256|512)\s?gb\b|\b(1|2|4)\s?tb\b/i;

const COLOR_KEYWORDS = [
  "kozmik turuncu",
  "rose gold",
  "uzay grisi",
  "titanyum",
  "siyah",
  "beyaz",
  "gümüş",
  "gumus",
  "gri",
  "mavi",
  "lacivert",
  "kırmızı",
  "kirmizi",
  "yeşil",
  "yesil",
  "mor",
  "altın",
  "altin",
  "pembe",
  "sarı",
  "sari",
  "turuncu",
];

export function parseVariant(productName) {
  const normalized = productName.toLocaleLowerCase("tr-TR");

  const capacityMatch = normalized.match(CAPACITY_PATTERN);
  const capacity = capacityMatch ? (capacityMatch[1] ? `${capacityMatch[1]}GB` : `${capacityMatch[2]}TB`) : null;

  const color = COLOR_KEYWORDS.find((keyword) => normalized.includes(keyword)) ?? null;

  return { capacity, color };
}
