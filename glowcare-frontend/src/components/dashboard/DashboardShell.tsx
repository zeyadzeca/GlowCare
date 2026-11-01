import { NavLink } from 'react-router-dom'
import type { ReactNode } from 'react'

/**
 * Shared seller/admin workspace: identity colors, sidebar nav, content panel.
 * The storefront Navbar/Footer still wrap these pages from App.tsx.
 */
export function DashboardShell({
  title,
  subtitle,
  nav,
  children,
}: {
  title: string
  subtitle?: string
  nav: { to: string; label: string; end?: boolean }[]
  children: ReactNode
}) {
  return (
    <div className="shell py-8 lg:py-10">
      <div className="mb-8">
        <p className="text-xs font-bold uppercase tracking-widest text-royal">Workspace</p>
        <h1 className="heading-display mt-1 uppercase">{title}</h1>
        {subtitle && <p className="mt-2 max-w-2xl text-navy/70">{subtitle}</p>}
      </div>

      <div className="grid gap-8 lg:grid-cols-[14rem_1fr]">
        <nav className="h-fit card p-3" aria-label="Dashboard">
          <ul className="space-y-1">
            {nav.map((item) => (
              <li key={item.to}>
                <NavLink
                  to={item.to}
                  end={item.end}
                  className={({ isActive }) =>
                    `block rounded-xl px-4 py-2.5 text-sm font-semibold transition-colors ${
                      isActive ? 'bg-yellow text-royal' : 'text-navy/70 hover:bg-off-white hover:text-royal'
                    }`
                  }
                >
                  {item.label}
                </NavLink>
              </li>
            ))}
          </ul>
        </nav>
        <div>{children}</div>
      </div>
    </div>
  )
}

export function StatCard({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="card p-5">
      <p className="text-xs font-bold uppercase tracking-widest text-navy/50">{label}</p>
      <p className="mt-2 font-display text-3xl text-royal">{value}</p>
    </div>
  )
}
