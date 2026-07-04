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

export function TechIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" {...defaultProps}>
      <rect x="7" y="7" width="10" height="10" rx="1.5" />
      <rect x="10" y="10" width="4" height="4" rx="0.5" />
      <line x1="12" y1="2" x2="12" y2="7" />
      <line x1="12" y1="17" x2="12" y2="22" />
      <line x1="2" y1="12" x2="7" y2="12" />
      <line x1="17" y1="12" x2="22" y2="12" />
    </svg>
  );
}

export function KitchenIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" {...defaultProps}>
      <path d="M4 11h16v3a5 5 0 0 1-5 5h-6a5 5 0 0 1-5-5v-3z" />
      <path d="M2 11h2M20 11h2" />
      <path d="M9 6c0-1 .5-1.5 0-3M15 6c0-1 .5-1.5 0-3" />
    </svg>
  );
}

export function ClothingIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" {...defaultProps}>
      <path d="M8 4l4 2 4-2 4 4-3 3v9H7v-9L4 8z" />
    </svg>
  );
}

export function HomeIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" {...defaultProps}>
      <path d="M4 11l8-7 8 7" />
      <path d="M6 10v10h12V10" />
      <line x1="10" y1="20" x2="10" y2="13" />
      <line x1="14" y1="20" x2="14" y2="13" />
    </svg>
  );
}

export function DropletIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" {...defaultProps}>
      <path d="M12 3c4 5 7 8.5 7 12a7 7 0 0 1-14 0c0-3.5 3-7 7-12z" />
    </svg>
  );
}

export function DumbbellIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" {...defaultProps}>
      <rect x="2" y="9" width="3" height="6" rx="1" />
      <rect x="19" y="9" width="3" height="6" rx="1" />
      <line x1="5" y1="12" x2="19" y2="12" />
      <rect x="6" y="7" width="2" height="10" rx="1" />
      <rect x="16" y="7" width="2" height="10" rx="1" />
    </svg>
  );
}

export function ToyIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" {...defaultProps}>
      <circle cx="12" cy="7" r="4" />
      <path d="M8 10c-3 1-5 3-5 7v3h18v-3c0-4-2-6-5-7" />
    </svg>
  );
}

export function CarIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" {...defaultProps}>
      <path d="M3 13l2-5a2 2 0 0 1 2-1h10a2 2 0 0 1 2 1l2 5" />
      <path d="M3 13h18v4a1 1 0 0 1-1 1h-1a1 1 0 0 1-1-1v-1H6v1a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1v-4z" />
      <circle cx="7" cy="16" r="1.3" fill="currentColor" stroke="none" />
      <circle cx="17" cy="16" r="1.3" fill="currentColor" stroke="none" />
    </svg>
  );
}
