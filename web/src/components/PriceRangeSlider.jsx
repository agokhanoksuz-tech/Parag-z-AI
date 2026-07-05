export default function PriceRangeSlider({ min, max, valueMin, valueMax, onChangeMin, onChangeMax }) {
  const lowerPercent = ((Number(valueMin ?? min) - min) / (max - min || 1)) * 100;
  const upperPercent = ((Number(valueMax ?? max) - min) / (max - min || 1)) * 100;

  return (
    <div className="price-range-slider">
      <div className="price-range-track">
        <div
          className="price-range-fill"
          style={{ left: `${lowerPercent}%`, right: `${100 - upperPercent}%` }}
        />
      </div>
      <input
        type="range"
        min={min}
        max={max}
        value={valueMin ?? min}
        onChange={(e) => onChangeMin(Math.min(Number(e.target.value), Number(valueMax ?? max)))}
      />
      <input
        type="range"
        min={min}
        max={max}
        value={valueMax ?? max}
        onChange={(e) => onChangeMax(Math.max(Number(e.target.value), Number(valueMin ?? min)))}
      />
    </div>
  );
}
