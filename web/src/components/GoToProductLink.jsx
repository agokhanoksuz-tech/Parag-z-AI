import { useState } from "react";
import { api } from "../api";

export default function GoToProductLink({ item }) {
  const [loading, setLoading] = useState(false);

  if (!item.immersiveProductToken) {
    return (
      <a
        href={item.url}
        target="_blank"
        rel="noreferrer"
        className="card-link"
        onClick={(e) => e.stopPropagation()}
      >
        Ürüne git →
      </a>
    );
  }

  async function handleClick(e) {
    e.preventDefault();
    e.stopPropagation();
    setLoading(true);

    try {
      const resolved = await api.get(
        `/product-link?token=${encodeURIComponent(item.immersiveProductToken)}&store=${encodeURIComponent(item.store)}`
      );
      window.open(resolved.url, "_blank", "noopener,noreferrer");
    } catch {
      // Doğrudan mağaza linki çözülemedi (örn. eşleşme bulunamadı) —
      // Google Shopping sayfasına düşmek hiç açılmamaktan iyidir.
      window.open(item.url, "_blank", "noopener,noreferrer");
    } finally {
      setLoading(false);
    }
  }

  return (
    <a href={item.url} onClick={handleClick} className="card-link">
      {loading ? "Yönlendiriliyor..." : "Ürüne git →"}
    </a>
  );
}
