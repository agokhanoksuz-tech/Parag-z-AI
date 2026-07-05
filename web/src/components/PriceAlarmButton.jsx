import { useState } from "react";
import { BellIcon } from "./icons";

export default function PriceAlarmButton({ item, favoriteId, targetPrice, onSetAlarm }) {
  const [open, setOpen] = useState(false);
  const [value, setValue] = useState(targetPrice ?? "");
  const [saving, setSaving] = useState(false);

  async function handleSave() {
    setSaving(true);
    try {
      const parsed = value === "" ? null : Number(value);
      const success = await onSetAlarm(item, parsed);
      if (success) setOpen(false);
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="price-alarm" onClick={(e) => e.stopPropagation()}>
      <button type="button" className="price-alarm-toggle" onClick={() => setOpen((o) => !o)}>
        <BellIcon />
        {favoriteId && targetPrice != null
          ? `Alarm: ${targetPrice.toLocaleString("tr-TR")} TL`
          : "Fiyat Alarmı"}
      </button>
      {open && (
        <div className="price-alarm-popover">
          <input
            type="number"
            min="1"
            step="0.01"
            placeholder="Hedef fiyat (TL)"
            value={value}
            onChange={(e) => setValue(e.target.value)}
          />
          <button type="button" className="btn-secondary" onClick={handleSave} disabled={saving}>
            {saving ? "Kaydediliyor..." : "Kaydet"}
          </button>
        </div>
      )}
    </div>
  );
}
