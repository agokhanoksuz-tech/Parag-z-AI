export function TrustBadge({ score }) {
  const isVerified = score >= 4;

  return (
    <span className={`badge ${isVerified ? "badge-trust-high" : "badge-trust-low"}`}>
      Güven {score.toFixed(1)}/5
    </span>
  );
}

export function RefurbishedBadge() {
  return <span className="badge badge-refurbished">Yenilenmiş / İkinci El</span>;
}

export function CheapestBadge() {
  return <span className="badge badge-cheapest">En Ucuz</span>;
}

export function LowestPriceBadge() {
  return <span className="badge badge-lowest">30 Günün En Düşüğü</span>;
}
