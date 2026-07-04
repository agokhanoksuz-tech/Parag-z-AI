import { SearchIcon, HistoryIcon, ShieldIcon, MailIcon } from "./icons";

const PROPS = [
  {
    Icon: SearchIcon,
    title: "Gerçek zamanlı arama",
    description: "Onlarca mağazadaki fiyatları anlık olarak karşılaştırıyoruz, bekletmiyoruz.",
  },
  {
    Icon: HistoryIcon,
    title: "30 günlük fiyat geçmişi",
    description: "Gördüğün fiyatın gerçekten ucuz mu pahalı mı olduğunu hemen anlarsın.",
  },
  {
    Icon: ShieldIcon,
    title: "Satıcı güven puanı",
    description: "Bilinen, güvenilir satıcıları öne çıkarıyor, şüpheli sonuçları işaretliyoruz.",
  },
  {
    Icon: MailIcon,
    title: "Fiyat düşünce haberdar ol",
    description: "Favorilediğin ürünün fiyatı düşünce doğrudan e-postana bildirim gönderiyoruz.",
  },
];

export default function ValueProps() {
  return (
    <section>
      <h2 className="section-title">Neden Paragöz AI?</h2>
      <div className="value-grid">
        {PROPS.map(({ Icon, title, description }) => (
          <div className="value-card" key={title}>
            <div className="value-icon">
              <Icon />
            </div>
            <h3>{title}</h3>
            <p>{description}</p>
          </div>
        ))}
      </div>
    </section>
  );
}
