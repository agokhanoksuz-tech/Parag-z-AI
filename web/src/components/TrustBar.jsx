import { SearchIcon, HistoryIcon, ShieldIcon, MailIcon } from "./icons";

const FEATURES = [
  { Icon: SearchIcon, label: "Gerçek zamanlı arama" },
  { Icon: HistoryIcon, label: "30 günlük fiyat geçmişi" },
  { Icon: ShieldIcon, label: "Satıcı güven puanı" },
  { Icon: MailIcon, label: "Fiyat düşünce e-posta" },
];

const STORES = [
  { name: "Teknosa", logo: "/logos/teknosa.png" },
  { name: "Hepsiburada", logo: "/logos/hepsiburada.png" },
  { name: "Trendyol", logo: "/logos/trendyol.jpg" },
  { name: "Vatan Bilgisayar", logo: "/logos/vatan.png" },
  { name: "MediaMarkt", logo: "/logos/mediamarkt.png" },
  { name: "Amazon", logo: "/logos/amazon.png" },
  { name: "N11", logo: "/logos/n11.png" },
];

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
        {STORES.map(({ name, logo }) => (
          <img key={name} className="store-strip-logo" src={logo} alt={name} title={name} loading="lazy" />
        ))}
      </div>
    </div>
  );
}
