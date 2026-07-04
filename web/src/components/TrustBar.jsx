import { SearchIcon, HistoryIcon, ShieldIcon, MailIcon } from "./icons";

const FEATURES = [
  { Icon: SearchIcon, label: "Gerçek zamanlı arama" },
  { Icon: HistoryIcon, label: "30 günlük fiyat geçmişi" },
  { Icon: ShieldIcon, label: "Satıcı güven puanı" },
  { Icon: MailIcon, label: "Fiyat düşünce e-posta" },
];

const STORES = ["Teknosa", "Hepsiburada", "Trendyol", "Vatan Bilgisayar", "MediaMarkt", "Amazon", "N11"];

export function TrustFeatures() {
  return (
    <div className="trust-row">
      {FEATURES.map(({ Icon, label }) => (
        <span key={label} className="trust-item">
          <Icon />
          {label}
        </span>
      ))}
    </div>
  );
}

export function StoreStrip() {
  return (
    <div className="store-strip">
      <span className="store-strip-label">Karşılaştırdığımız mağazalardan bazıları</span>
      <div className="store-strip-list">
        {STORES.map((store) => (
          <span key={store} className="store-chip">
            {store}
          </span>
        ))}
      </div>
    </div>
  );
}
