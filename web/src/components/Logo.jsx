// "Paragöz" kelimesindeki "ö" harfinin yerine geçen tek bir göz+dolar glifi —
// çevredeki metnin yazı tipi boyutuna göre orantılı ölçeklenmesi için em
// birimiyle boyutlandırılır, ayrı bir ikon değil kelimenin bir parçası gibi.
export default function Logo() {
  return (
    <svg
      viewBox="0 0 20 20"
      fill="none"
      aria-hidden="true"
      style={{ width: "0.72em", height: "0.72em", verticalAlign: "-0.05em" }}
    >
      <path d="M1 10 Q10 0 19 10 Q10 20 1 10 Z" stroke="currentColor" strokeWidth="2" />
      <text
        x="10"
        y="13.5"
        textAnchor="middle"
        fontSize="10"
        fontWeight="700"
        fill="currentColor"
        fontFamily="system-ui, sans-serif"
      >
        $
      </text>
    </svg>
  );
}
