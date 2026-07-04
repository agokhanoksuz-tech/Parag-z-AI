export default function Logo({ size = 22 }) {
  return (
    <svg
      width={size * 2.2}
      height={size}
      viewBox="0 0 48 20"
      fill="none"
      aria-hidden="true"
    >
      <path d="M2 10 Q10 0 18 10 Q10 20 2 10 Z" stroke="currentColor" strokeWidth="2" />
      <text
        x="10"
        y="13.5"
        textAnchor="middle"
        fontSize="9"
        fontWeight="700"
        fill="currentColor"
        fontFamily="system-ui, sans-serif"
      >
        $
      </text>

      <path d="M30 10 Q38 0 46 10 Q38 20 30 10 Z" stroke="currentColor" strokeWidth="2" />
      <text
        x="38"
        y="13.5"
        textAnchor="middle"
        fontSize="9"
        fontWeight="700"
        fill="currentColor"
        fontFamily="system-ui, sans-serif"
      >
        $
      </text>
    </svg>
  );
}
