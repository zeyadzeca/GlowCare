import type { ReactNode } from 'react'

/** Centered page shell used by every page section. */
export function Container({ children, className = '' }: { children: ReactNode; className?: string }) {
  return <div className={`shell ${className}`}>{children}</div>
}
