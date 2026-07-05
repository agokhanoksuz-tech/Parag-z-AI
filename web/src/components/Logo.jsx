// "Paragöz" kelimesindeki "ö" harfinin yerine geçen tek bir göz+dolar glifi —
// çevredeki metnin yazı tipi boyutuna göre orantılı ölçeklenmesi için em
// birimiyle boyutlandırılır, ayrı bir ikon değil kelimenin bir parçası gibi.
export default function Logo() {
  return (
    <svg
      viewBox="0 0 20 20"
      aria-hidden="true"
      style={{ width: "0.9em", height: "0.9em", verticalAlign: "-0.1em" }}
    >
      <path
        d="M1 10 Q10 -1.5 19 10 Q10 21.5 1 10 Z"
        fill="currentColor"
        fillOpacity="0.14"
        stroke="currentColor"
        strokeWidth="2.2"
      />
      <circle cx="10" cy="10" r="5.6" fill="currentColor" />
      <text
        x="10"
        y="13.1"
        textAnchor="middle"
        fontSize="7.6"
        fontWeight="800"
        fill="#fff"
        fontFamily="system-ui, sans-serif"
      >
        $
      </text>
    </svg>
  );
}
