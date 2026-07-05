export default function PriceRangeBar({ price, min, max }) {
  if (min == null || max == null || max <= min) return null;

  const percent = Math.min(100, Math.max(0, ((price - min) / (max - min)) * 100));

  return (
    <div className="price-range-bar" title="Bu ürün için taranan mağazalar arasındaki fiyat aralığı">
      <div className="price-range-bar-track">
        <span className="price-range-bar-dot" style={{ left: `${percent}%` }} />
      </div>
      <div className="price-range-bar-labels">
        <span>{min.toLocaleString("tr-TR")} TL</span>
        <span>{max.toLocaleString("tr-TR")} TL</span>
      </div>
    </div>
  );
}
