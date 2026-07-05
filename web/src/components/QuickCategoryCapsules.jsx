import { CATEGORIES } from "./CategoryChips";

export default function QuickCategoryCapsules({ onSelect }) {
  return (
    <div className="quick-capsules">
      {CATEGORIES.map(({ name, Icon, items }) => (
        <button
          key={name}
          type="button"
          className="quick-capsule"
          onClick={() => onSelect(items[0].term)}
        >
          <Icon />
          <span>{name}</span>
        </button>
      ))}
    </div>
  );
}
