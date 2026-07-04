import { useEffect, useState } from "react";
import { api } from "../api";

const WIDTH = 220;
const HEIGHT = 56;

export default function PriceHistoryChart({ query }) {
  const [points, setPoints] = useState(null);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const data = await api.get(`/price-history?product=${encodeURIComponent(query)}`);
        if (!cancelled) setPoints(data);
      } catch {
        if (!cancelled) setPoints([]);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [query]);

  if (!points || points.length < 2) return null;

  const prices = points.map((p) => p.lowestPrice);
  const min = Math.min(...prices);
  const max = Math.max(...prices);
  const range = max - min || 1;
  const stepX = WIDTH / (points.length - 1);

  const coords = points.map((p, i) => [
    i * stepX,
    HEIGHT - ((p.lowestPrice - min) / range) * HEIGHT,
  ]);

  const pathD = coords.map(([x, y], i) => `${i === 0 ? "M" : "L"}${x.toFixed(1)},${y.toFixed(1)}`).join(" ");
  const [lastX, lastY] = coords[coords.length - 1];

  return (
    <div className="price-history-chart">
      <svg width={WIDTH} height={HEIGHT} viewBox={`0 0 ${WIDTH} ${HEIGHT}`} preserveAspectRatio="none">
        <path className="price-history-line" d={pathD} />
        <circle className="price-history-dot" cx={lastX} cy={lastY} r="3" />
      </svg>
      <div className="price-history-labels">
        <span>30 gün en düşük: {min.toLocaleString("tr-TR")} TL</span>
        <span>en yüksek: {max.toLocaleString("tr-TR")} TL</span>
      </div>
    </div>
  );
}
