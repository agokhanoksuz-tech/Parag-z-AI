function HeartIcon({ filled }) {
  return (
    <svg
      width="16"
      height="16"
      viewBox="0 0 24 24"
      fill={filled ? "currentColor" : "none"}
      stroke="currentColor"
      strokeWidth="2"
      aria-hidden="true"
    >
      <path d="M12 21s-6.7-4.35-9.3-8.1C1 10.1 1.7 6.6 4.6 5.1c2.4-1.25 4.9-.4 6.4 1.4l1 1.2 1-1.2c1.5-1.8 4-2.65 6.4-1.4 2.9 1.5 3.6 5 1.9 7.8C18.7 16.65 12 21 12 21z" />
    </svg>
  );
}

export default function FavoriteButton({ isFavorited, onClick, disabled }) {
  return (
    <button
      type="button"
      className={`favorite-button ${isFavorited ? "is-favorited" : ""}`}
      onClick={(e) => {
        e.stopPropagation();
        onClick();
      }}
      disabled={disabled}
      title={isFavorited ? "Favorilerden çıkar" : "Favorilere ekle"}
      aria-label={isFavorited ? "Favorilerden çıkar" : "Favorilere ekle"}
    >
      <HeartIcon filled={isFavorited} />
    </button>
  );
}
