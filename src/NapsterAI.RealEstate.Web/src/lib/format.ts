/** Best-effort currency formatting - falls back gracefully for currency codes Intl doesn't recognize (e.g. none seeded, but defensive). */
export function formatPrice(amount: number, currency: string): string {
  try {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: currency || "USD",
      maximumFractionDigits: 0,
    }).format(amount);
  } catch {
    return `${currency} ${amount.toLocaleString()}`;
  }
}

export function formatNumber(amount: number): string {
  return amount.toLocaleString();
}
