import { PhoneIcon, LaptopIcon, HeadphonesIcon, WatchIcon, TvIcon, GamepadIcon } from "./icons";

const CATEGORIES = [
  { label: "Telefon", term: "iphone 15 128gb", Icon: PhoneIcon },
  { label: "Bilgisayar", term: "macbook air m2", Icon: LaptopIcon },
  { label: "Kulaklık", term: "airpods pro", Icon: HeadphonesIcon },
  { label: "Akıllı Saat", term: "apple watch se", Icon: WatchIcon },
  { label: "Televizyon", term: "samsung 55 inç tv", Icon: TvIcon },
  { label: "Oyun Konsolu", term: "playstation 5", Icon: GamepadIcon },
];

export default function CategoryChips({ onSelect }) {
  return (
    <div className="category-row">
      {CATEGORIES.map(({ label, term, Icon }) => (
        <button key={label} type="button" className="category-chip" onClick={() => onSelect(term)}>
          <Icon />
          <span>{label}</span>
        </button>
      ))}
    </div>
  );
}
