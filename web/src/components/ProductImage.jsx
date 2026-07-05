import { useState } from "react";
import FavoriteButton from "./FavoriteButton";
import { BellIcon } from "./icons";

export default function ProductImage({ item, isFavorited, onToggleFavorite }) {
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
        <div className="card-image-placeholder">{item.store.charAt(0).toUpperCase()}</div>
      )}
      <FavoriteButton isFavorited={isFavorited} onClick={() => onToggleFavorite(item)} />
      <span className="tracking-badge" title="Fiyat geçmişi takibe alındı">
        <BellIcon />
      </span>
    </div>
  );
}
