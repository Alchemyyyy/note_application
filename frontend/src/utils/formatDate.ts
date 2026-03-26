export function formatDate(dateIso: string) {
  return new Date(dateIso).toLocaleString()
}
