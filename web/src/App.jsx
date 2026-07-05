import { useEffect, useState } from "react";
import "./App.css";
import { api } from "./api";
import CategoryChips from "./components/CategoryChips";
import CategorySidebarTree from "./components/CategorySidebarTree";
import FavoriteButton from "./components/FavoriteButton";
import FeaturedProductGrid from "./components/FeaturedProductGrid";
import Footer from "./components/Footer";
import HowItWorks from "./components/HowItWorks";
import { BellIcon } from "./components/icons";
import LoginForm from "./components/LoginForm";
import Logo from "./components/Logo";
import PriceAlarmButton from "./components/PriceAlarmButton";
import PriceHistoryChart from "./components/PriceHistoryChart";
import PriceRangeBar from "./components/PriceRangeBar";
import PriceRangeSlider from "./components/PriceRangeSlider";
import QuickCategoryCapsules from "./components/QuickCategoryCapsules";
import RatingStars from "./components/RatingStars";
import RegisterForm from "./components/RegisterForm";
import { StoreStrip, TrustFeatures } from "./components/TrustBar";
import TrendingGrid from "./components/TrendingGrid";
import ValueProps from "./components/ValueProps";

function formatRelativeTime(isoString) {
  const diffMs = Date.now() - new Date(isoString).getTime();
  const minutes = Math.round(diffMs / 60000);

  if (minutes < 1) return "az önce";
  if (minutes < 60) return `${minutes} dakika önce`;

  const hours = Math.round(minutes / 60);
  if (hours < 24) return `${hours} saat önce`;

  const days = Math.round(hours / 24);
  return `${days} gün önce`;
}

function TrustBadge({ score }) {
  const isVerified = score >= 4;

  return (
    <span className={`badge ${isVerified ? "badge-trust-high" : "badge-trust-low"}`}>
      Güven {score.toFixed(1)}/5
    </span>
  );
}

function RefurbishedBadge() {
  return <span className="badge badge-refurbished">Yenilenmiş / İkinci El</span>;
}

function CheapestBadge() {
  return <span className="badge badge-cheapest">En Ucuz</span>;
}

function LowestPriceBadge() {
  return <span className="badge badge-lowest">30 Günün En Düşüğü</span>;
}

function ProductImage({ item, isFavorited, onToggleFavorite }) {
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

function StoreLine({ item }) {
  const [iconFailed, setIconFailed] = useState(false);

  return (
    <p className="card-store">
      {item.storeIconUrl && !iconFailed && (
        <img
          className="card-store-icon"
          src={item.storeIconUrl}
          alt=""
          loading="lazy"
          onError={() => setIconFailed(true)}
        />
      )}
      {item.store}
    </p>
  );
}

function GoToProductLink({ item }) {
  const [loading, setLoading] = useState(false);

  if (!item.immersiveProductToken) {
    return (
      <a href={item.url} target="_blank" rel="noreferrer" className="card-link">
        Ürüne git →
      </a>
    );
  }

  async function handleClick(e) {
    e.preventDefault();
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

function ResultCard({
  item,
  highlight,
  isFavorited,
  onToggleFavorite,
  searchedProduct,
  favoriteId,
  targetPrice,
  onSetPriceAlarm,
  priceRangeMin,
  priceRangeMax,
}) {
  if (highlight) {
    return (
      <div className="hero-card">
        <ProductImage item={item} isFavorited={isFavorited} onToggleFavorite={onToggleFavorite} />

        <div className="card-body">
          <StoreLine item={item} />
          <p className="card-title">{item.product}</p>
          <RatingStars rating={item.rating} reviewCount={item.reviewCount} />

          <div className="badge-row">
            <CheapestBadge />
            <TrustBadge score={item.trustScore} />
            {item.isRefurbished && <RefurbishedBadge />}
            {item.last30DaysLowestPrice != null && item.price <= item.last30DaysLowestPrice && (
              <LowestPriceBadge />
            )}
          </div>

          {item.last30DaysLowestPrice != null && (
            <p className="card-history">
              Son 30 gün en düşük: {item.last30DaysLowestPrice.toLocaleString("tr-TR")} TL
            </p>
          )}

          <p className="card-price">{item.price.toLocaleString("tr-TR")} TL</p>
          <PriceRangeBar price={item.price} min={priceRangeMin} max={priceRangeMax} />

          {searchedProduct && <PriceHistoryChart key={searchedProduct} query={searchedProduct} />}

          <PriceAlarmButton
            item={item}
            favoriteId={favoriteId}
            targetPrice={targetPrice}
            onSetAlarm={onSetPriceAlarm}
          />
        </div>

        <GoToProductLink item={item} />
      </div>
    );
  }

  return (
    <div className="result-row">
      <ProductImage item={item} isFavorited={isFavorited} onToggleFavorite={onToggleFavorite} />

      <div className="row-main">
        <StoreLine item={item} />
        <p className="card-title">{item.product}</p>
        <RatingStars rating={item.rating} reviewCount={item.reviewCount} />

        <div className="badge-row">
          <TrustBadge score={item.trustScore} />
          {item.isRefurbished && <RefurbishedBadge />}
          {item.last30DaysLowestPrice != null && item.price <= item.last30DaysLowestPrice && (
            <LowestPriceBadge />
          )}
        </div>

        <PriceRangeBar price={item.price} min={priceRangeMin} max={priceRangeMax} />
      </div>

      <div className="row-price-block">
        {item.last30DaysLowestPrice != null && (
          <p className="card-history">
            Son 30 gün: {item.last30DaysLowestPrice.toLocaleString("tr-TR")} TL
          </p>
        )}
        <p className="card-price">{item.price.toLocaleString("tr-TR")} TL</p>
        <PriceAlarmButton
          item={item}
          favoriteId={favoriteId}
          targetPrice={targetPrice}
          onSetAlarm={onSetPriceAlarm}
        />
        <GoToProductLink item={item} />
      </div>
    </div>
  );
}

function TargetPriceField({ favorite, onSave }) {
  const [editing, setEditing] = useState(false);
  const [value, setValue] = useState(favorite.targetPrice ?? "");
  const [saving, setSaving] = useState(false);

  async function handleSave() {
    setSaving(true);
    try {
      const parsed = value === "" ? null : Number(value);
      await onSave(favorite.id, parsed);
      setEditing(false);
    } finally {
      setSaving(false);
    }
  }

  if (!editing) {
    return (
      <button type="button" className="link-button" onClick={() => setEditing(true)}>
        {favorite.targetPrice != null
          ? `Hedef fiyat: ${favorite.targetPrice.toLocaleString("tr-TR")} TL`
          : "Hedef fiyat belirle"}
      </button>
    );
  }

  return (
    <div className="target-price-editor">
      <input
        type="number"
        min="1"
        step="0.01"
        placeholder="Hedef fiyat (TL)"
        value={value}
        onChange={(e) => setValue(e.target.value)}
      />
      <button type="button" className="btn-secondary" onClick={handleSave} disabled={saving}>
        {saving ? "Kaydediliyor..." : "Kaydet"}
      </button>
    </div>
  );
}

function FavoritesView({ favorites, onRemove, onSetTargetPrice }) {
  if (favorites.length === 0) {
    return (
      <div className="empty-state">
        <p>Henüz favori ürünün yok. Bir ürün arayıp kalp ikonuna tıklayarak ekleyebilirsin.</p>
      </div>
    );
  }

  return (
    <div className="results-list">
      {favorites.map((f) => (
        <div className="result-row" key={f.id}>
          <div className="card-image-box">
            <div className="card-image-placeholder">{f.storeName.charAt(0).toUpperCase()}</div>
          </div>

          <div className="row-main">
            <p className="card-store">{f.storeName}</p>
            <p className="card-title">{f.productName}</p>
            <p className="card-history">
              Favoriye eklendiğinde: {f.priceAtFavoriteTime.toLocaleString("tr-TR")} TL
            </p>
            <TargetPriceField favorite={f} onSave={onSetTargetPrice} />
          </div>

          <div className="row-price-block">
            <p className="card-price">
              {f.currentPrice != null ? `${f.currentPrice.toLocaleString("tr-TR")} TL` : "Fiyat bekleniyor"}
            </p>
            <a href={f.url} target="_blank" rel="noreferrer" className="card-link">
              Ürüne git →
            </a>
            <button className="btn-secondary" onClick={() => onRemove(f.id)}>
              Favoriden çıkar
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}

export default function App() {
  const [product, setProduct] = useState("");
  const [sort, setSort] = useState("asc");
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [hasSearched, setHasSearched] = useState(false);

  const [user, setUser] = useState(null);
  const [authPanel, setAuthPanel] = useState(null); // null | "login" | "register"
  const [favorites, setFavorites] = useState([]);
  const [trending, setTrending] = useState([]);
  const [recentlyViewed, setRecentlyViewed] = useState([]);
  const [priceMin, setPriceMin] = useState(null);
  const [priceMax, setPriceMax] = useState(null);
  const [condition, setCondition] = useState("all"); // all | new | refurbished
  const [selectedStores, setSelectedStores] = useState([]);
  const [showFavorites, setShowFavorites] = useState(false);

  useEffect(() => {
    (async () => {
      try {
        const me = await api.get("/auth/me");
        setUser(me);
      } catch {
        setUser(null);
      }
    })();

    (async () => {
      try {
        const items = await api.get("/trending");
        setTrending(items);
      } catch {
        setTrending([]);
      }
    })();
  }, []);

  useEffect(() => {
    if (!user) return;

    (async () => {
      try {
        const list = await api.get("/favorites");
        setFavorites(list);
      } catch {
        setFavorites([]);
      }
    })();

    loadRecentlyViewed();
  }, [user]);

  async function loadRecentlyViewed() {
    try {
      const items = await api.get("/recently-viewed");
      setRecentlyViewed(items);
    } catch {
      setRecentlyViewed([]);
    }
  }

  async function search(nextSort = sort, term = product) {
    if (!term.trim()) return;

    setShowFavorites(false);
    setLoading(true);
    setError("");
    setHasSearched(true);
    setPriceMin(null);
    setPriceMax(null);
    setCondition("all");
    setSelectedStores([]);

    try {
      const json = await api.get(`/search?product=${encodeURIComponent(term)}&sort=${nextSort}`);
      setData(json);
      if (user) loadRecentlyViewed();
    } catch (err) {
      setError(err.message);
      setData(null);
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

  function handleSuggestionClick(term) {
    setProduct(term);
    search(sort, term);
  }

  function handleGoHome() {
    setShowFavorites(false);
    setHasSearched(false);
    setData(null);
    setError("");
  }

  async function handleLogout() {
    try {
      await api.post("/auth/logout");
    } catch {
      // yerel oturum yine de temizlenir
    }
    setUser(null);
    setFavorites([]);
    setRecentlyViewed([]);
    setShowFavorites(false);
  }

  function isFavorited(item) {
    return favorites.some((f) => f.storeName === item.store && f.url === item.url);
  }

  function favoriteIdFor(item) {
    return favorites.find((f) => f.storeName === item.store && f.url === item.url)?.id ?? null;
  }

  function targetPriceFor(item) {
    return favorites.find((f) => f.storeName === item.store && f.url === item.url)?.targetPrice ?? null;
  }

  async function handleSetPriceAlarm(item, targetPrice, query) {
    if (!user) {
      setAuthPanel("login");
      return false;
    }

    const existingId = favoriteIdFor(item);

    try {
      if (existingId) {
        await api.put(`/favorites/${existingId}/target-price`, { targetPrice });
        setFavorites((prev) => prev.map((f) => (f.id === existingId ? { ...f, targetPrice } : f)));
      } else {
        const created = await api.post("/favorites", {
          query: query ?? data?.searchedProduct,
          storeName: item.store,
          productName: item.product,
          url: item.url,
          targetPrice,
        });
        setFavorites((prev) => [...prev, created]);
      }
      return true;
    } catch (err) {
      setError(err.message);
      return false;
    }
  }

  async function handleToggleFavorite(item, query) {
    if (!user) {
      setAuthPanel("login");
      return;
    }

    const existingId = favoriteIdFor(item);

    try {
      if (existingId) {
        await api.del(`/favorites/${existingId}`);
        setFavorites((prev) => prev.filter((f) => f.id !== existingId));
      } else {
        const created = await api.post("/favorites", {
          query: query ?? data?.searchedProduct,
          storeName: item.store,
          productName: item.product,
          url: item.url,
        });
        setFavorites((prev) => [...prev, created]);
      }
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleRemoveFavorite(id) {
    try {
      await api.del(`/favorites/${id}`);
      setFavorites((prev) => prev.filter((f) => f.id !== id));
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleSetTargetPrice(id, targetPrice) {
    await api.put(`/favorites/${id}/target-price`, { targetPrice });
    setFavorites((prev) => prev.map((f) => (f.id === id ? { ...f, targetPrice } : f)));
  }

  function toggleStore(store) {
    setSelectedStores((prev) => (prev.includes(store) ? prev.filter((s) => s !== store) : [...prev, store]));
  }

  const storeCounts = (data?.results ?? []).reduce((counts, item) => {
    counts[item.store] = (counts[item.store] ?? 0) + 1;
    return counts;
  }, {});
  const storeEntries = Object.entries(storeCounts).sort((a, b) => b[1] - a[1]);

  const resultPrices = (data?.results ?? []).map((r) => r.price);
  const priceBounds =
    resultPrices.length > 0
      ? {
          min: Math.floor(Math.min(...resultPrices) / 100) * 100,
          max: Math.ceil(Math.max(...resultPrices) / 100) * 100,
        }
      : { min: 0, max: 100000 };

  const filteredResults = (data?.results ?? []).filter((item) => {
    if (priceMin != null && item.price < priceMin) return false;
    if (priceMax != null && item.price > priceMax) return false;
    if (condition === "new" && item.isRefurbished) return false;
    if (condition === "refurbished" && !item.isRefurbished) return false;
    if (selectedStores.length > 0 && !selectedStores.includes(item.store)) return false;
    return true;
  });

  const filteredCheapest =
    filteredResults.length > 0
      ? filteredResults.reduce((cheapest, item) => (item.price < cheapest.price ? item : cheapest))
      : null;

  const otherResults = filteredResults.filter((item) => item.url !== filteredCheapest?.url);

  return (
    <>
      <div className="site-header-group">
        <div className="site-header-inner">
          <header className="site-header">
            <button type="button" className="brand" onClick={handleGoHome}>
              Parag
              <Logo />z AI
            </button>

            <nav className="header-nav">
              {user ? (
                <>
                  <button type="button" className="nav-link" onClick={() => setShowFavorites(true)}>
                    Favorilerim{favorites.length > 0 ? ` (${favorites.length})` : ""}
                  </button>
                  <span className="nav-user">{user.email}</span>
                  <button type="button" className="btn-secondary" onClick={handleLogout}>
                    Çıkış yap
                  </button>
                </>
              ) : (
                <>
                  <button type="button" className="btn-secondary" onClick={() => setAuthPanel("login")}>
                    Giriş yap
                  </button>
                  <button type="button" className="btn-primary" onClick={() => setAuthPanel("register")}>
                    Kayıt ol
                  </button>
                </>
              )}
            </nav>
          </header>

          <CategoryChips onSelect={handleSuggestionClick} />
        </div>
      </div>

      <div className="app">
      {authPanel && (
        <div className="auth-overlay" onClick={() => setAuthPanel(null)}>
          <div className="auth-panel" onClick={(e) => e.stopPropagation()}>
            <button type="button" className="auth-close" onClick={() => setAuthPanel(null)} aria-label="Kapat">
              ×
            </button>
            {authPanel === "login" ? (
              <LoginForm
                onSuccess={(u) => {
                  setUser(u);
                  setAuthPanel(null);
                }}
                onSwitchToRegister={() => setAuthPanel("register")}
              />
            ) : (
              <RegisterForm
                onSuccess={(u) => {
                  setUser(u);
                  setAuthPanel(null);
                }}
                onSwitchToLogin={() => setAuthPanel("login")}
              />
            )}
          </div>
        </div>
      )}

      <div className="hero">
        <p className="eyebrow">Türkiye'nin akıllı fiyat karşılaştırma platformu</p>
        <h1>Paragöz AI</h1>
        <p>Bir ürün adı yaz, onlarca mağazayı aynı anda tara, en ucuz fiyatı bul.</p>

        <div className="search-row">
          <input
            className="search-input"
            value={product}
            onChange={(e) => setProduct(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && search()}
            placeholder="Örn. iphone 15 128gb"
          />
          <button className="btn-primary" onClick={() => search()} disabled={loading}>
            {loading ? "Aranıyor..." : "Ara"}
          </button>
          {data && (
            <select className="sort-select" value={sort} onChange={handleSortChange}>
              <option value="asc">Ucuzdan pahalıya</option>
              <option value="desc">Pahalıdan ucuza</option>
            </select>
          )}
        </div>

        <TrustFeatures />
      </div>

      <StoreStrip />

      <div className="page-layout">
        <aside className="search-filters">
          <h3>Gelişmiş Filtreler &amp; Navigasyon</h3>

          <div className="filter-group">
            <label className="filter-label">Kategoriler</label>
            <CategorySidebarTree onSelect={handleSuggestionClick} />
          </div>

          {data && data.results.length > 0 && (
            <>
              <div className="filter-group">
                <label className="filter-label">Fiyat Aralığı (TL)</label>
                <PriceRangeSlider
                  min={priceBounds.min}
                  max={priceBounds.max}
                  valueMin={priceMin}
                  valueMax={priceMax}
                  onChangeMin={setPriceMin}
                  onChangeMax={setPriceMax}
                />
                <div className="price-range-labels">
                  <span>{(priceMin ?? priceBounds.min).toLocaleString("tr-TR")} TL</span>
                  <span>{(priceMax ?? priceBounds.max).toLocaleString("tr-TR")} TL</span>
                </div>
              </div>

              <div className="filter-group">
                <label className="filter-label">Durum</label>
                <div className="filter-radio-group">
                  <label className="filter-radio">
                    <input
                      type="radio"
                      name="condition"
                      checked={condition === "all"}
                      onChange={() => setCondition("all")}
                    />
                    Tümü
                  </label>
                  <label className="filter-radio">
                    <input
                      type="radio"
                      name="condition"
                      checked={condition === "new"}
                      onChange={() => setCondition("new")}
                    />
                    Sadece yeni
                  </label>
                  <label className="filter-radio">
                    <input
                      type="radio"
                      name="condition"
                      checked={condition === "refurbished"}
                      onChange={() => setCondition("refurbished")}
                    />
                    Sadece yenilenmiş
                  </label>
                </div>
              </div>

              {storeEntries.length > 0 && (
                <div className="filter-group">
                  <label className="filter-label">Mağaza</label>
                  <div className="filter-checkbox-list">
                    {storeEntries.map(([store, count]) => (
                      <label className="filter-checkbox" key={store}>
                        <input
                          type="checkbox"
                          checked={selectedStores.includes(store)}
                          onChange={() => toggleStore(store)}
                        />
                        {store} <span className="filter-count">({count})</span>
                      </label>
                    ))}
                  </div>
                </div>
              )}
            </>
          )}
        </aside>

        <div className="main-column">
          {showFavorites ? (
            <>
              <h2 className="section-title">Favorilerim</h2>
              <FavoritesView
                favorites={favorites}
                onRemove={handleRemoveFavorite}
                onSetTargetPrice={handleSetTargetPrice}
              />
            </>
          ) : (
            <>
              {loading && (
                <p className="status-row">
                  <span className="spinner" />
                  Fiyatlar taranıyor...
                </p>
              )}

              {error && <div className="error-banner">{error}</div>}

              {!hasSearched && !loading && !error && (
                <>
                  {user && recentlyViewed.length > 0 && (
                    <TrendingGrid
                      items={recentlyViewed}
                      title="Son baktıkların"
                      onSelect={(query) => {
                        setProduct(query);
                        search(sort, query);
                      }}
                    />
                  )}

                  {trending.length > 0 && (
                    <TrendingGrid
                      items={trending}
                      title="Günün En İyi Fiyat Düşüşleri"
                      onSelect={(query) => {
                        setProduct(query);
                        search(sort, query);
                      }}
                    />
                  )}

                  <QuickCategoryCapsules onSelect={handleSuggestionClick} />

                  <FeaturedProductGrid
                    items={trending}
                    isFavorited={isFavorited}
                    onToggleFavorite={handleToggleFavorite}
                    favoriteIdFor={favoriteIdFor}
                    targetPriceFor={targetPriceFor}
                    onSetPriceAlarm={handleSetPriceAlarm}
                    onSelect={(query) => {
                      setProduct(query);
                      search(sort, query);
                    }}
                  />

                  <HowItWorks />
                  <ValueProps />
                </>
              )}

              {data && !loading && (
                <>
                  {data.resultCount === 0 ? (
                    <div className="empty-state">
                      <p>Bu ürün için uygun bir sonuç bulunamadı.</p>
                    </div>
                  ) : (
                    <div className="search-results">
                      <p className="result-count">
                        {filteredResults.length} sonuç bulundu
                        {data.generatedAt && (
                          <span className="result-freshness">
                            {" "}
                            · Son güncelleme: {formatRelativeTime(data.generatedAt)}
                          </span>
                        )}
                      </p>

                      {filteredResults.length === 0 ? (
                        <div className="empty-state">
                          <p>Filtrelere uyan sonuç yok. Filtreleri genişletmeyi dene.</p>
                        </div>
                      ) : (
                        <>
                          {filteredCheapest && (
                            <ResultCard
                              item={filteredCheapest}
                              highlight
                              isFavorited={isFavorited(filteredCheapest)}
                              onToggleFavorite={handleToggleFavorite}
                              searchedProduct={data.searchedProduct}
                              favoriteId={favoriteIdFor(filteredCheapest)}
                              targetPrice={targetPriceFor(filteredCheapest)}
                              onSetPriceAlarm={handleSetPriceAlarm}
                              priceRangeMin={priceBounds.min}
                              priceRangeMax={priceBounds.max}
                            />
                          )}

                          {otherResults.length > 0 && (
                            <>
                              <h2 className="section-title">Diğer Sonuçlar</h2>
                              <div className="results-list">
                                {otherResults.map((item, i) => (
                                  <ResultCard
                                    key={i}
                                    item={item}
                                    isFavorited={isFavorited(item)}
                                    onToggleFavorite={handleToggleFavorite}
                                    favoriteId={favoriteIdFor(item)}
                                    targetPrice={targetPriceFor(item)}
                                    onSetPriceAlarm={handleSetPriceAlarm}
                                    priceRangeMin={priceBounds.min}
                                    priceRangeMax={priceBounds.max}
                                  />
                                ))}
                              </div>
                            </>
                          )}
                        </>
                      )}
                    </div>
                  )}
                </>
              )}
            </>
          )}
        </div>
      </div>

      <Footer />
      </div>
    </>
  );
}
