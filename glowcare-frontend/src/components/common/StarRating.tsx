import { clampRating } from '@/utils/format'

/**
 * Star rating display used on product cards and the details page.
 * Purely presentational - the value always comes from the API.
 */
export function StarRating({
  rating,
  count,
  size = 'sm',
}: {
  rating: number
  count?: number
  size?: 'sm' | 'md'
}) {
  const value = clampRating(rating)
  const full = Math.floor(value)
  const hasHalf = value - full >= 0.5
  const textClass = size === 'sm' ? 'text-xs' : 'text-sm'

  return (
    <div className="flex items-center gap-1" aria-label={`Rated ${value.toFixed(1)} out of 5`}>
      <span className={`text-yellow-gold ${size === 'sm' ? 'text-sm' : 'text-base'}`} aria-hidden="true">
        {'★'.repeat(full)}
        {hasHalf ? '½' : ''}
        <span className="text-navy/25">{'★'.repeat(5 - full - (hasHalf ? 1 : 0))}</span>
      </span>
      <span className={`${textClass} text-navy/60`}>
        {value.toFixed(1)}
        {typeof count === 'number' && ` (${count})`}
      </span>
    </div>
  )
}
