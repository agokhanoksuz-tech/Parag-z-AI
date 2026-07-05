import { useEffect, useState } from "react";
import { api } from "../api";
import { CATEGORIES } from "../data/categories";

function matchesQuery(text, normalizedQuery) {
  return text.toLocaleLowerCase("tr-TR").includes(normalizedQuery);
}

function getCategoryMatches(query) {
  const normalizedQuery = query.toLocaleLowerCase("tr-TR");
  const matches = [];

  for (const category of CATEGORIES) {
    for (const item of category.items) {
      if (matchesQuery(item.term, normalizedQuery) || matchesQuery(item.label, normalizedQuery)) {
        matches.push({ label: item.label, term: item.term, categoryName: category.name });
      }
      if (matches.length >= 5) return matches;
    }
  }

  return matches;
}

export default function SearchSuggestions({ query, onSelect }) {
  const [products, setProducts] = useState([]);

  useEffect(() => {
    const trimmed = query.trim();
    if (trimmed.length < 2) {
      setProducts([]);
      return;
    }

    let cancelled = false;
    const timer = setTimeout(async () => {
      try {
        const items = await api.get(`/search-suggestions?q=${encodeURIComponent(trimmed)}`);
        if (!cancelled) setProducts(items);
      } catch {
        if (!cancelled) setProducts([]);
      }
    }, 250);

    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [query]);

  const trimmed = query.trim();
  if (trimmed.length < 2) return null;

  const categoryMatches = getCategoryMatches(trimmed);

  if (categoryMatches.length === 0 && products.length === 0) return null;

  return (
    <div className="search-suggestions" onMouseDown={(e) => e.preventDefault()}>
      {categoryMatches.length > 0 && (
        <div className="search-suggestions-section">
          <p className="search-suggestions-label">Kategoriler</p>
          {categoryMatches.map((match) => (
            <button
              key={match.label}
              type="button"
              className="search-suggestion-row"
              onClick={() => onSelect(match.term)}
            >
              <span className="search-suggestion-title">{match.label}</span>
              <span className="search-suggestion-meta">{match.categoryName}</span>
            </button>
          ))}
        </div>
      )}

      {products.length > 0 && (
        <div className="search-suggestions-section">
          <p className="search-suggestions-label">Daha Önce Aranan Ürünler</p>
          {products.map((item) => (
            <button
              key={item.query}
              type="button"
              className="search-suggestion-row search-suggestion-product"
              onClick={() => onSelect(item.query)}
            >
              {item.imageUrl ? (
                <img className="search-suggestion-image" src={item.imageUrl} alt="" loading="lazy" />
              ) : (
                <span className="search-suggestion-image-placeholder">{item.storeName.charAt(0).toUpperCase()}</span>
              )}
              <span className="search-suggestion-product-info">
                <span className="search-suggestion-title">{item.productName}</span>
                <span className="search-suggestion-meta">{item.price.toLocaleString("tr-TR")} TL</span>
              </span>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
