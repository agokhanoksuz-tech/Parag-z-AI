import { useState } from "react";
import { CATEGORIES } from "../data/categories";

function ChevronIcon({ open }) {
  return (
    <svg
      width="12"
      height="12"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      style={{ transform: open ? "rotate(180deg)" : "none", transition: "transform 0.15s ease" }}
    >
      <polyline points="6 9 12 15 18 9" />
    </svg>
  );
}

export default function CategorySidebarTree({ onSelect }) {
  const [openSet, setOpenSet] = useState(() => new Set());

  function toggle(index) {
    setOpenSet((prev) => {
      const next = new Set(prev);
      if (next.has(index)) next.delete(index);
      else next.add(index);
      return next;
    });
  }

  return (
    <div className="category-tree">
      {CATEGORIES.map(({ name, Icon, items }, index) => {
        const isOpen = openSet.has(index);
        return (
          <div className="category-tree-group" key={name}>
            <button type="button" className="category-tree-header" onClick={() => toggle(index)}>
              <Icon />
              <span>{name}</span>
              <ChevronIcon open={isOpen} />
            </button>
            {isOpen && (
              <div className="category-tree-items">
                {items.map(({ label, term }) => (
                  <button
                    key={label}
                    type="button"
                    className="category-tree-item"
                    onClick={() => onSelect(term)}
                  >
                    {label}
                  </button>
                ))}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
