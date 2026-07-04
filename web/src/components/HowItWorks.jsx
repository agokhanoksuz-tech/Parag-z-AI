const STEPS = [
  {
    number: "1",
    title: "Ürün adını yaz",
    description: "İstediğin ürünün adını arama kutusuna yaz ve Enter'a bas.",
  },
  {
    number: "2",
    title: "Onlarca mağaza taransın",
    description: "Sistem aynı anda birçok mağazadaki fiyatları senin için karşılaştırır.",
  },
  {
    number: "3",
    title: "En ucuza git ya da favorile",
    description: "En ucuz sonuca git veya favorileyip fiyat düşünce e-posta ile haberdar ol.",
  },
];

export default function HowItWorks() {
  return (
    <section>
      <h2 className="section-title">Nasıl çalışır?</h2>
      <div className="steps-row">
        {STEPS.map((step) => (
          <div className="step-card" key={step.number}>
            <span className="step-number">{step.number}</span>
            <h3>{step.title}</h3>
            <p>{step.description}</p>
          </div>
        ))}
      </div>
    </section>
  );
}
