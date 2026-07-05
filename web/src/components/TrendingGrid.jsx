import { useRef, useState } from "react";
import PriceHistoryChart from "./PriceHistoryChart";

function TrendingImage({ item }) {
  const [imageFailed, setImageFailed] = useState(false);

  return (
    <div className="card-image-box">
      {item.imageUrl && !imageFailed ? (
        <img
          className="card-image"
          src={item.imageUrl}
          alt=""
          loading="lazy"
          onError={() => setImageFailed(true)}
        />
      ) : (
        <div className="card-image-placeholder">{item.storeName.charAt(0).toUpperCase()}</div>
      )}
    </div>
  );
}

function ArrowIcon({ direction }) {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round">
      <polyline points={direction === "left" ? "15 18 9 12 15 6" : "9 18 15 12 9 6"} />
    </svg>
  );
}

export default function TrendingGrid({ items, onSelect, title = "Son bakılan ürünler" }) {
  const trackRef = useRef(null);

  if (!items.length) return null;

  function scrollBy(amount) {
    trackRef.current?.scrollBy({ left: amount, behavior: "smooth" });
  }

  return (
    <section className="trending-section">
      <div className="trending-section-header">
        <h2 className="section-title">{title}</h2>
        <div className="trending-arrows">
          <button type="button" className="trending-arrow" onClick={() => scrollBy(-320)} aria-label="Geri kaydır">
            <ArrowIcon direction="left" />
          </button>
          <button type="button" className="trending-arrow" onClick={() => scrollBy(320)} aria-label="İleri kaydır">
            <ArrowIcon direction="right" />
          </button>
        </div>
      </div>

      <div className="results-track" ref={trackRef}>
        {items.map((item) => (
          <button
            key={item.query}
            type="button"
            className="trending-card"
            onClick={() => onSelect(item.query)}
          >
            <TrendingImage item={item} />
            <div className="card-body">
              <p className="card-store">{item.storeName}</p>
              <p className="card-title">{item.productName}</p>
              <p className="card-price">{item.price.toLocaleString("tr-TR")} TL</p>
              <PriceHistoryChart query={item.query} />
            </div>
          </button>
        ))}
      </div>
    </section>
  );
}
