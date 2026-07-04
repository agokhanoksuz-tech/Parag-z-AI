import { useState } from "react";
import {
  TechIcon,
  KitchenIcon,
  ClothingIcon,
  HomeIcon,
  DropletIcon,
  DumbbellIcon,
  ToyIcon,
  CarIcon,
} from "./icons";

const CATEGORIES = [
  {
    name: "Teknoloji",
    Icon: TechIcon,
    items: [
      { label: "Telefon", term: "iphone 15 128gb" },
      { label: "Tablet", term: "ipad 10. nesil" },
      { label: "Bilgisayar", term: "macbook air m2" },
      { label: "Kulaklık", term: "airpods pro" },
      { label: "Akıllı Saat", term: "apple watch se" },
      { label: "Televizyon", term: "samsung 55 inç tv" },
      { label: "Oyun Konsolu", term: "playstation 5" },
      { label: "Yazıcı", term: "hp yazıcı" },
    ],
  },
  {
    name: "Mutfak",
    Icon: KitchenIcon,
    items: [
      { label: "Robot Süpürge", term: "robot süpürge" },
      { label: "Blender", term: "blender" },
      { label: "Kahve Makinesi", term: "kahve makinesi" },
      { label: "Airfryer", term: "airfryer" },
      { label: "Tencere Seti", term: "tencere seti" },
      { label: "Su Isıtıcısı", term: "su ısıtıcısı" },
    ],
  },
  {
    name: "Giyim",
    Icon: ClothingIcon,
    items: [
      { label: "Erkek Ayakkabı", term: "erkek spor ayakkabı" },
      { label: "Kadın Ayakkabı", term: "kadın spor ayakkabı" },
      { label: "Mont", term: "erkek mont" },
      { label: "Sırt Çantası", term: "sırt çantası" },
    ],
  },
  {
    name: "Ev & Yaşam",
    Icon: HomeIcon,
    items: [
      { label: "Çamaşır Makinesi", term: "çamaşır makinası" },
      { label: "Buzdolabı", term: "buzdolabı" },
      { label: "Klima", term: "klima" },
      { label: "Elektrikli Süpürge", term: "elektrikli süpürge" },
    ],
  },
  {
    name: "Kişisel Bakım",
    Icon: DropletIcon,
    items: [
      { label: "Saç Kurutma Makinesi", term: "saç kurutma makinesi" },
      { label: "Elektrikli Diş Fırçası", term: "elektrikli diş fırçası" },
      { label: "Tıraş Makinesi", term: "tıraş makinesi" },
    ],
  },
  {
    name: "Spor & Outdoor",
    Icon: DumbbellIcon,
    items: [
      { label: "Bisiklet", term: "bisiklet" },
      { label: "Koşu Bandı", term: "koşu bandı" },
      { label: "Kamp Çadırı", term: "kamp çadırı" },
    ],
  },
  {
    name: "Bebek & Oyuncak",
    Icon: ToyIcon,
    items: [
      { label: "Bebek Arabası", term: "bebek arabası" },
      { label: "Lego", term: "lego yapı seti" },
    ],
  },
  {
    name: "Otomobil",
    Icon: CarIcon,
    items: [
      { label: "Oto Lastik", term: "oto lastik" },
      { label: "Araç Multimedya", term: "araç multimedya" },
    ],
  },
];

export default function CategoryChips({ onSelect }) {
  const [activeIndex, setActiveIndex] = useState(0);
  const active = CATEGORIES[activeIndex];

  return (
    <div className="category-browser">
      <div className="category-tabs">
        {CATEGORIES.map(({ name, Icon }, index) => (
          <button
            key={name}
            type="button"
            className={`category-tab${index === activeIndex ? " is-active" : ""}`}
            onClick={() => setActiveIndex(index)}
          >
            <Icon />
            <span>{name}</span>
          </button>
        ))}
      </div>

      <div className="category-row">
        {active.items.map(({ label, term }) => (
          <button key={label} type="button" className="category-chip" onClick={() => onSelect(term)}>
            {label}
          </button>
        ))}
      </div>
    </div>
  );
}
