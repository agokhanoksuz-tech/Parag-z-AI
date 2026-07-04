import { useState } from "react";

export default function App() {
  const [product, setProduct] = useState("");
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function search() {
    if (!product.trim()) return;

    setLoading(true);
    setError("");
    setData(null);

    try {
      const res = await fetch(
        `http://localhost:5269/search?product=${encodeURIComponent(product)}`
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

  return (
    <div style={{ maxWidth: 900, margin: "40px auto", fontFamily: "Arial" }}>
      <h1>PriceFinder AI</h1>

      <input
        value={product}
        onChange={(e) => setProduct(e.target.value)}
        placeholder="Ürün ara..."
        style={{ width: "70%", padding: 12, fontSize: 16 }}
      />

      <button onClick={search} style={{ marginLeft: 10, padding: "12px 20px" }}>
        Ara
      </button>

      {loading && <p>Aranıyor...</p>}
      {error && <p style={{ color: "red" }}>{error}</p>}

      {data && (
        <>
          <p>Bulunan sonuç: {data.resultCount}</p>

          {data.cheapest && (
            <>
              <h2>En Ucuz Sonuç</h2>
              <div style={{ padding: 15, border: "1px solid #ccc", borderRadius: 8 }}>
                <h3>{data.cheapest.product}</h3>
                <p>{data.cheapest.store}</p>
                <h2>{data.cheapest.price} TL</h2>
                <a href={data.cheapest.url} target="_blank" rel="noreferrer">
                  Ürüne Git
                </a>
              </div>
            </>
          )}

          <h2>Tüm Sonuçlar</h2>

          {data.results?.map((item, i) => (
            <div key={i} style={{ border: "1px solid #ddd", padding: 10, marginBottom: 10, borderRadius: 8 }}>
              <b>{item.product}</b>
              <p>{item.store}</p>
              <h3>{item.price} TL</h3>
              <a href={item.url} target="_blank" rel="noreferrer">
                Aç
              </a>
            </div>
          ))}
        </>
      )}
    </div>
  );
}