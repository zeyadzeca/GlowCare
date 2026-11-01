import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'

/**
 * The three states every data-driven screen must handle: still loading, loaded
 * but empty, and failed. Keeping them here means no page reinvents them and no
 * page forgets one.
 */

export function Spinner({ label = 'Loading...' }: { label?: string }) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 py-16 text-navy/60">
      <span className="h-8 w-8 animate-spin rounded-full border-2 border-royal border-t-transparent" />
      <p className="text-sm">{label}</p>
    </div>
  )
}

export function EmptyState({
  title,
  message,
  actionLabel,
  actionTo,
  children,
}: {
  title: string
  message: string
  actionLabel?: string
  actionTo?: string
  children?: ReactNode
}) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 rounded-2xl bg-white px-6 py-16 text-center">
      <h3 className="heading-panel">{title}</h3>
      <p className="max-w-md text-navy/60">{message}</p>
      {actionLabel && actionTo && (
        <Link to={actionTo} className="btn-primary mt-2">
          {actionLabel}
        </Link>
      )}
      {children}
    </div>
  )
}

export function ErrorState({
  message,
  onRetry,
}: {
  message: string
  onRetry?: () => void
}) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 rounded-2xl bg-red-50 px-6 py-16 text-center">
      <h3 className="font-display text-2xl text-red-900">Something went wrong</h3>
      <p className="max-w-md text-red-800">{message}</p>
      {onRetry && (
        <button type="button" onClick={onRetry} className="btn-ghost mt-2">
          Try again
        </button>
      )}
    </div>
  )
}
