import { StarIcon } from "./icons";

export default function RatingStars({ rating, reviewCount }) {
  if (rating == null) return null;

  return (
    <span className="rating-stars">
      <StarIcon />
      {rating.toFixed(1)}
      <span className="rating-count">({reviewCount.toLocaleString("tr-TR")})</span>
    </span>
  );
}
