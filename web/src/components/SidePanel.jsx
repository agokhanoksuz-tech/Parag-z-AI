export default function SidePanel({ user, favorites, trending, onSelect }) {
  const priceAlarms = favorites.filter((f) => f.targetPrice != null);
  const trendingPreview = trending.slice(0, 5);

  return (
    <aside className="side-panel">
      <div className="side-panel-card">
        <h3>Fiyat Alarmların</h3>
        {!user ? (
          <p className="side-panel-empty">Fiyat düşünce e-posta ile haber almak için giriş yap.</p>
        ) : priceAlarms.length === 0 ? (
          <p className="side-panel-empty">
            Henüz aktif bir fiyat alarmın yok. Bir üründeki "Fiyat Alarmı" butonuna tıklayıp hedef fiyat
            belirleyebilirsin.
          </p>
        ) : (
          <ul className="side-panel-list">
            {priceAlarms.map((f) => (
              <li key={f.id} className="side-panel-item">
                <p className="side-panel-item-title">{f.productName}</p>
                <p className="side-panel-item-meta">
                  Hedef: {f.targetPrice.toLocaleString("tr-TR")} TL
                  {f.currentPrice != null && ` · Şu an: ${f.currentPrice.toLocaleString("tr-TR")} TL`}
                </p>
              </li>
            ))}
          </ul>
        )}
      </div>

      {trendingPreview.length > 0 && (
        <div className="side-panel-card">
          <h3>Trend Ürünler</h3>
          <ul className="side-panel-list">
            {trendingPreview.map((item) => (
              <li key={item.query}>
                <button type="button" className="side-panel-trend-item" onClick={() => onSelect(item.query)}>
                  <span className="side-panel-item-title">{item.productName}</span>
                  <span className="side-panel-item-meta">{item.price.toLocaleString("tr-TR")} TL</span>
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}

      <div className="side-panel-card">
        <h3>Neden Paragöz AI?</h3>
        <ul className="side-panel-trust-list">
          <li>Onlarca mağazayı aynı anda tarar</li>
          <li>Gerçek kullanıcı puanlarını gösterir</li>
          <li>30 günlük fiyat geçmişiyle gerçek indirimi ayırt eder</li>
          <li>Fiyat düşünce e-posta ile haber verir</li>
        </ul>
      </div>
    </aside>
  );
}
