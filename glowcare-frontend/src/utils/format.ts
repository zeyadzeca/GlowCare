/** Currency formatter for the Egyptian-pound storefront (StoreSettings: EGP). */
const egp = new Intl.NumberFormat('en-EG', {
  style: 'currency',
  currency: 'EGP',
  minimumFractionDigits: 2,
})

/** Formats a money value from the API, e.g. 250 -> "EGP 250.00". */
export function formatPrice(value: number | null | undefined): string {
  if (value === null || value === undefined || Number.isNaN(value)) {
    return egp.format(0)
  }
  return egp.format(value)
}

/** Short date for order cards, e.g. "31 Oct 2026". */
export function formatDate(iso: string | null | undefined): string {
  if (!iso) return ''
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return ''
  return date.toLocaleDateString('en-GB', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  })
}

/** Date + time for tracking timelines. */
export function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return ''
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return ''
  return date.toLocaleString('en-GB', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

/** Renders a 0..5 star row from the API's average rating. */
export function clampRating(rating: number): number {
  if (Number.isNaN(rating)) return 0
  return Math.min(5, Math.max(0, rating))
}

/** Builds a URL query string from a filter object, skipping empty values. */
export function toQueryString(params: Record<string, unknown>): string {
  const query = new URLSearchParams()

  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') continue
    query.set(key, String(value))
  }

  const result = query.toString()
  return result ? `?${result}` : ''
}
