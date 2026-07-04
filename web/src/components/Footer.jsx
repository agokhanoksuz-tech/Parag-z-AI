export default function Footer() {
  return (
    <footer className="site-footer">
      <div className="footer-brand">Paragöz AI</div>
      <p className="footer-tagline">
        Türkiye'deki mağazalardaki fiyatları karşılaştırıp en uygun seçeneği bulmana yardımcı oluyoruz.
      </p>
      <p className="footer-disclaimer">
        Fiyat ve stok bilgileri ilgili mağazalar tarafından güncellenir; Paragöz AI doğruluğunu garanti etmez.
      </p>
      <p className="footer-copyright">© {new Date().getFullYear()} Paragöz AI</p>
    </footer>
  );
}
