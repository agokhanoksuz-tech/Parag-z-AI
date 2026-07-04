import { useState } from "react";

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

export default function TrendingGrid({ items, onSelect, title = "Son bakılan ürünler" }) {
  if (!items.length) return null;

  return (
    <section>
      <h2 className="section-title">{title}</h2>
      <div className="results-grid">
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
            </div>
          </button>
        ))}
      </div>
    </section>
  );
}
