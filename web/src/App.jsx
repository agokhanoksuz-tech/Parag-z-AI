import { useState } from "react";

function TrustBadge({ score }) {
  const isVerified = score >= 4;

  return (
    <span
      style={{
        display: "inline-block",
        padding: "2px 8px",
        borderRadius: 999,
        fontSize: 13,
        marginRight: 8,
        color: isVerified ? "var(--accent)" : "var(--text)",
        background: isVerified ? "var(--accent-bg)" : "var(--code-bg)",
        border: `1px solid ${isVerified ? "var(--accent-border)" : "var(--border)"}`,
      }}
    >
      Güven: {score.toFixed(1)}/5
    </span>
  );
}

function ResultCard({ item, highlight }) {
  return (
    <div
      style={{
        padding: highlight ? 15 : 10,
        border: `1px solid ${highlight ? "var(--accent-border)" : "var(--border)"}`,
        borderRadius: 8,
        marginBottom: highlight ? 0 : 10,
        textAlign: "left",
      }}
    >
      <b>{item.product}</b>
      <p style={{ color: "var(--text)" }}>{item.store}</p>
      <h3 style={{ margin: "8px 0" }}>{item.price.toLocaleString("tr-TR")} TL</h3>

      <div style={{ margin: "8px 0" }}>
        <TrustBadge score={item.trustScore} />
        {item.last30DaysLowestPrice != null && (
          <span style={{ fontSize: 13, color: "var(--text)" }}>
            Son 30 gün en düşük: {item.last30DaysLowestPrice.toLocaleString("tr-TR")} TL
          </span>
        )}
      </div>

      <a href={item.url} target="_blank" rel="noreferrer">
        Ürüne git
      </a>
    </div>
  );
}

export default function App() {
  const [product, setProduct] = useState("");
  const [sort, setSort] = useState("asc");
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function search(nextSort = sort) {
    if (!product.trim()) return;

    setLoading(true);
    setError("");
    setData(null);

    try {
      const res = await fetch(
        `http://localhost:5269/search?product=${encodeURIComponent(product)}&sort=${nextSort}`
      );

      if (!res.ok) {
        throw new Error(`API hatası: ${res.status}`);
      }

      const json = await res.json();
      setData(json);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  function handleSortChange(e) {
    const value = e.target.value;
    setSort(value);
    if (data) {
      search(value);
    }
  }

  return (
    <div style={{ maxWidth: 900, margin: "40px auto" }}>
      <h1>PriceFinder AI</h1>

      <input
        value={product}
        onChange={(e) => setProduct(e.target.value)}
        onKeyDown={(e) => e.key === "Enter" && search()}
        placeholder="Ürün ara..."
        style={{ width: "60%", padding: 12, fontSize: 16 }}
      />

      <button onClick={() => search()} style={{ marginLeft: 10, padding: "12px 20px" }}>
        Ara
      </button>

      {data && (
        <select
          value={sort}
          onChange={handleSortChange}
          style={{ marginLeft: 10, padding: 12, fontSize: 14 }}
        >
          <option value="asc">Ucuzdan pahalıya</option>
          <option value="desc">Pahalıdan ucuza</option>
        </select>
      )}

      {loading && <p>Aranıyor...</p>}
      {error && <p style={{ color: "red" }}>{error}</p>}

      {data && (
        <>
          <p>Bulunan sonuç: {data.resultCount}</p>

          {data.cheapest && (
            <>
              <h2>En Ucuz Sonuç</h2>
              <ResultCard item={data.cheapest} highlight />
            </>
          )}

          <h2 style={{ marginTop: 24 }}>Tüm Sonuçlar</h2>

          {data.results?.map((item, i) => (
            <ResultCard key={i} item={item} />
          ))}
        </>
      )}
    </div>
  );
}
