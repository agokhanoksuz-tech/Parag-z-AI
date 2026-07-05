import { useState } from "react";
import FavoriteButton from "./FavoriteButton";
import PriceAlarmButton from "./PriceAlarmButton";
import PriceHistoryChart from "./PriceHistoryChart";
import RatingStars from "./RatingStars";

function FeaturedImage({ item }) {
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

export default function FeaturedProductGrid({
  items,
  isFavorited,
  onToggleFavorite,
  favoriteIdFor,
  targetPriceFor,
  onSetPriceAlarm,
  onSelect,
}) {
  if (!items.length) return null;

  return (
    <section>
      <h2 className="section-title">Öne Çıkan Ürünler</h2>
      <div className="featured-grid">
        {items.map((item) => {
          const asResultItem = { store: item.storeName, product: item.productName, url: item.url, price: item.price };

          return (
            <div className="featured-card" key={item.query}>
              <div className="featured-card-image" onClick={() => onSelect(item.query)}>
                <FeaturedImage item={item} />
                <FavoriteButton
                  isFavorited={isFavorited(asResultItem)}
                  onClick={() => onToggleFavorite(asResultItem, item.query)}
                />
              </div>

              <div className="card-body">
                <p className="card-store">{item.storeName}</p>
                <button type="button" className="featured-card-title" onClick={() => onSelect(item.query)}>
                  {item.productName}
                </button>
                <RatingStars rating={item.rating} reviewCount={item.reviewCount} />
                <PriceHistoryChart query={item.query} />
                <p className="card-price">{item.price.toLocaleString("tr-TR")} TL</p>
                <PriceAlarmButton
                  item={asResultItem}
                  favoriteId={favoriteIdFor(asResultItem)}
                  targetPrice={targetPriceFor(asResultItem)}
                  onSetAlarm={(resultItem, targetPrice) => onSetPriceAlarm(resultItem, targetPrice, item.query)}
                />
              </div>
            </div>
          );
        })}
      </div>
    </section>
  );
}
