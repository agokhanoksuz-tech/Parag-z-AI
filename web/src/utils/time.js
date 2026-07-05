export function formatRelativeTime(isoString) {
  const diffMs = Date.now() - new Date(isoString).getTime();
  const minutes = Math.round(diffMs / 60000);

  if (minutes < 1) return "az önce";
  if (minutes < 60) return `${minutes} dakika önce`;

  const hours = Math.round(minutes / 60);
  if (hours < 24) return `${hours} saat önce`;

  const days = Math.round(hours / 24);
  return `${days} gün önce`;
}
