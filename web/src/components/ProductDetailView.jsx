import { useState } from "react";
import { CATEGORIES } from "../data/categories";
import { formatRelativeTime } from "../utils/time";
import { parseVariant } from "../utils/variant";
import GoToProductLink from "./GoToProductLink";
import PriceAlarmButton from "./PriceAlarmButton";
import PriceHistoryChart from "./PriceHistoryChart";
import ProductImage from "./ProductImage";
import RatingStars from "./RatingStars";
import { CheapestBadge, LowestPriceBadge, RefurbishedBadge, TrustBadge } from "./ResultBadges";
import StoreLine from "./StoreLine";

function findBreadcrumb(searchedProduct) {
  if (!searchedProduct) return null;

  const normalized = searchedProduct.toLocaleLowerCase("tr-TR").trim();

  for (const category of CATEGORIES) {
    for (const item of category.items) {
      const term = item.term.toLocaleLowerCase("tr-TR").trim();
      if (normalized === term || normalized.includes(term) || term.includes(normalized)) {
        return { categoryName: category.name, itemLabel: item.label };
      }
    }
  }

  return null;
}

export default function ProductDetailView({
  data,
  selectedItem,
  isFavorited,
  favoriteIdFor,
  targetPriceFor,
  onToggleFavorite,
  onSetPriceAlarm,
  onBack,
}) {
  const initialVariant = parseVariant(selectedItem.product);
  const [selectedCapacity, setSelectedCapacity] = useState(initialVariant.capacity);
  const [selectedColor, setSelectedColor] = useState(initialVariant.color);

  const annotated = data.results.map((item) => ({ item, variant: parseVariant(item.product) }));

  const capacities = [...new Set(annotated.map((x) => x.variant.capacity).filter(Boolean))];

  const withinCapacity = selectedCapacity
    ? annotated.filter((x) => x.variant.capacity === selectedCapacity)
    : annotated;

  const colors = [...new Set(withinCapacity.map((x) => x.variant.color).filter(Boolean))];

  const withinColor = selectedColor ? withinCapacity.filter((x) => x.variant.color === selectedColor) : withinCapacity;

  // Bir seçim gerçek sonuçlarda karşılığı olmayan bir kombinasyona düşerse
  // (örn. bu renk bu kapasitede bulunamadı), boş bir "sonuç yok" durumu
  // göstermek yerine kapasite düzeyindeki listeye geri düşülür.
  const subsetItems = (withinColor.length > 0 ? withinColor : withinCapacity).map((x) => x.item);
  const sortedSubset = [...subsetItems].sort((a, b) => a.price - b.price);
  const cheapest = sortedSubset[0] ?? selectedItem;

  const breadcrumb = findBreadcrumb(data.searchedProduct);

  function handleCapacityClick(capacity) {
    setSelectedCapacity(capacity);
    setSelectedColor(null);
  }

  return (
    <div className="product-detail">
      <button type="button" className="detail-back" onClick={onBack}>
        ← Tüm sonuçlara dön
      </button>

      {breadcrumb && (
        <p className="detail-breadcrumb">
          {breadcrumb.categoryName} <span>›</span> {breadcrumb.itemLabel}
        </p>
      )}

      <div className="detail-main">
        <ProductImage
          item={cheapest}
          isFavorited={isFavorited(cheapest)}
          onToggleFavorite={(item) => onToggleFavorite(item, data.searchedProduct)}
        />

        <div className="detail-info">
          <StoreLine item={cheapest} />
          <h2 className="detail-title">{cheapest.product}</h2>
          <RatingStars rating={cheapest.rating} reviewCount={cheapest.reviewCount} />

          {capacities.length > 1 && (
            <div className="variant-group">
              <span className="variant-group-label">Depolama</span>
              <div className="variant-pills">
                {capacities.map((capacity) => (
                  <button
                    key={capacity}
                    type="button"
                    className={`variant-pill${capacity === selectedCapacity ? " is-selected" : ""}`}
                    onClick={() => handleCapacityClick(capacity)}
                  >
                    {capacity}
                  </button>
                ))}
              </div>
            </div>
          )}

          {colors.length > 1 && (
            <div className="variant-group">
              <span className="variant-group-label">Renk</span>
              <div className="variant-pills">
                {colors.map((color) => (
                  <button
                    key={color}
                    type="button"
                    className={`variant-pill${color === selectedColor ? " is-selected" : ""}`}
                    onClick={() => setSelectedColor(color)}
                  >
                    {color}
                  </button>
                ))}
              </div>
            </div>
          )}

          <div className="badge-row">
            <CheapestBadge />
            <TrustBadge score={cheapest.trustScore} />
            {cheapest.isRefurbished && <RefurbishedBadge />}
            {cheapest.last30DaysLowestPrice != null && cheapest.price <= cheapest.last30DaysLowestPrice && (
              <LowestPriceBadge />
            )}
          </div>

          <p className="detail-seller-count">{sortedSubset.length} mağaza içinde kargo dahil en ucuz fiyat</p>
          <p className="detail-price">{cheapest.price.toLocaleString("tr-TR")} TL</p>

          {data.generatedAt && (
            <p className="result-freshness">Son güncelleme: {formatRelativeTime(data.generatedAt)}</p>
          )}

          <div className="detail-actions">
            <PriceAlarmButton
              item={cheapest}
              favoriteId={favoriteIdFor(cheapest)}
              targetPrice={targetPriceFor(cheapest)}
              onSetAlarm={(item, targetPrice) => onSetPriceAlarm(item, targetPrice, data.searchedProduct)}
            />
            <GoToProductLink item={cheapest} />
          </div>

          <PriceHistoryChart query={data.searchedProduct} />
        </div>
      </div>

      <h3 className="section-title">{sortedSubset.length} satıcı arasında fiyat karşılaştırması</h3>

      <div className="results-list detail-seller-list">
        {sortedSubset.map((item, i) => (
          <div className="result-row" key={i}>
            <ProductImage
              item={item}
              isFavorited={isFavorited(item)}
              onToggleFavorite={(it) => onToggleFavorite(it, data.searchedProduct)}
            />

            <div className="row-main">
              <StoreLine item={item} />
              <p className="card-title">{item.product}</p>
              <div className="badge-row">
                <TrustBadge score={item.trustScore} />
                {item.isRefurbished && <RefurbishedBadge />}
              </div>
            </div>

            <div className="row-price-block">
              <p className="card-price">{item.price.toLocaleString("tr-TR")} TL</p>
              <PriceAlarmButton
                item={item}
                favoriteId={favoriteIdFor(item)}
                targetPrice={targetPriceFor(item)}
                onSetAlarm={(it, targetPrice) => onSetPriceAlarm(it, targetPrice, data.searchedProduct)}
              />
              <GoToProductLink item={item} />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
