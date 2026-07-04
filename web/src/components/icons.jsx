const defaultProps = {
  fill: "none",
  stroke: "currentColor",
  strokeWidth: 1.8,
  strokeLinecap: "round",
  strokeLinejoin: "round",
  "aria-hidden": true,
};

export function SearchIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" {...defaultProps}>
      <circle cx="11" cy="11" r="7" />
      <line x1="21" y1="21" x2="16.65" y2="16.65" />
    </svg>
  );
}

export function HistoryIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" {...defaultProps}>
      <polyline points="4 6 10 12 14 8 20 18" />
      <circle cx="20" cy="18" r="1.4" fill="currentColor" stroke="none" />
    </svg>
  );
}

export function ShieldIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" {...defaultProps}>
      <path d="M12 3l7 3v6c0 4.5-3 8-7 9-4-1-7-4.5-7-9V6l7-3z" />
      <path d="M9 12l2 2 4-4" />
    </svg>
  );
}

export function MailIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" {...defaultProps}>
      <rect x="3" y="5" width="18" height="14" rx="2" />
      <polyline points="3 7 12 13 21 7" />
    </svg>
  );
}

export function PhoneIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" {...defaultProps}>
      <rect x="7" y="2" width="10" height="20" rx="2" />
      <line x1="11" y1="18" x2="13" y2="18" />
    </svg>
  );
}

export function LaptopIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" {...defaultProps}>
      <rect x="4" y="4" width="16" height="11" rx="1.5" />
      <path d="M2 19h20" />
    </svg>
  );
}

export function HeadphonesIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" {...defaultProps}>
      <path d="M4 14v-2a8 8 0 0 1 16 0v2" />
      <rect x="2" y="14" width="5" height="7" rx="1.5" />
      <rect x="17" y="14" width="5" height="7" rx="1.5" />
    </svg>
  );
}

export function WatchIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" {...defaultProps}>
      <rect x="7" y="7" width="10" height="10" rx="2.5" />
      <path d="M9 7V4h6v3M9 17v3h6v-3" />
    </svg>
  );
}

export function TvIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" {...defaultProps}>
      <rect x="3" y="5" width="18" height="12" rx="1.5" />
      <path d="M8 21h8M12 17v4" />
    </svg>
  );
}

export function GamepadIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" {...defaultProps}>
      <path d="M6 9h12l2 7a2.5 2.5 0 0 1-4.5 1.6L14 16h-4l-1.5 1.6A2.5 2.5 0 0 1 4 16l2-7z" />
      <line x1="8" y1="12" x2="8" y2="14" />
      <line x1="7" y1="13" x2="9" y2="13" />
      <circle cx="16" cy="12" r="0.8" fill="currentColor" stroke="none" />
      <circle cx="18" cy="14" r="0.8" fill="currentColor" stroke="none" />
    </svg>
  );
}
