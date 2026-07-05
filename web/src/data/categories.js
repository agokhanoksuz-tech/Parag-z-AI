import {
  TechIcon,
  HomeIcon,
  ToyIcon,
  WatchIcon,
  BookIcon,
  DumbbellIcon,
  DropletIcon,
  CarIcon,
  PawIcon,
} from "../components/icons";

// Cimri'nin gerçek ana kategori gruplandırmasına göre (bkz. cimri.com) —
// "Süpermarket" hariç, çünkü orada kupon/broşür tabanlı ayrı bir akış,
// bizim tek-ürün-fiyat-karşılaştırma modelimize uymuyor.
export const CATEGORIES = [
  {
    name: "Elektronik & Cep Telefonu",
    Icon: TechIcon,
    items: [
      { label: "Cep Telefonu", term: "cep telefonu" },
      { label: "Tablet", term: "tablet" },
      { label: "Bilgisayar", term: "laptop" },
      { label: "Kulaklık", term: "kablosuz kulaklık" },
      { label: "Televizyon", term: "led tv" },
      { label: "Oyun Konsolu", term: "playstation 5" },
      { label: "Yazıcı", term: "yazıcı" },
    ],
  },
  {
    name: "Ev, Yaşam & Ofis",
    Icon: HomeIcon,
    items: [
      { label: "Robot Süpürge", term: "robot süpürge" },
      { label: "Kahve Makinesi", term: "kahve makinesi" },
      { label: "Airfryer", term: "airfryer" },
      { label: "Çamaşır Makinesi", term: "çamaşır makinası" },
      { label: "Buzdolabı", term: "buzdolabı" },
      { label: "Klima", term: "klima" },
    ],
  },
  {
    name: "Anne, Bebek & Oyuncak",
    Icon: ToyIcon,
    items: [
      { label: "Bebek Arabası", term: "bebek arabası" },
      { label: "Bebek Bezi", term: "bebek bezi" },
      { label: "Mama Sandalyesi", term: "mama sandalyesi" },
      { label: "Akülü Araba", term: "akülü araba" },
      { label: "Lego", term: "lego yapı seti" },
    ],
  },
  {
    name: "Saat, Moda & Ayakkabı",
    Icon: WatchIcon,
    items: [
      { label: "Akıllı Saat", term: "akıllı saat" },
      { label: "Kol Saati", term: "kol saati" },
      { label: "Spor Ayakkabı", term: "erkek spor ayakkabı" },
      { label: "Güneş Gözlüğü", term: "güneş gözlüğü" },
    ],
  },
  {
    name: "Kitap, Müzik & Hobi",
    Icon: BookIcon,
    items: [
      { label: "Drone", term: "drone" },
      { label: "Gitar", term: "klasik gitar" },
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
    name: "Sağlık, Bakım & Kozmetik",
    Icon: DropletIcon,
    items: [
      { label: "Saç Kurutma Makinesi", term: "saç kurutma makinesi" },
      { label: "Elektrikli Diş Fırçası", term: "elektrikli diş fırçası" },
      { label: "Tıraş Makinesi", term: "tıraş makinesi" },
    ],
  },
  {
    name: "Oto, Bahçe & Yapı Market",
    Icon: CarIcon,
    items: [
      { label: "Oto Lastik", term: "oto lastik" },
      { label: "Araç Multimedya", term: "araç multimedya" },
    ],
  },
  {
    name: "Petshop",
    Icon: PawIcon,
    items: [
      { label: "Kedi Maması", term: "kuru kedi maması" },
      { label: "Köpek Maması", term: "köpek maması" },
    ],
  },
];
