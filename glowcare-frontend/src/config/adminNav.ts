import type { NavItem } from '@/components/dashboard/DashboardShell'

/** Shared Admin navigation - used across all Admin pages for consistent sidebar */
export const ADMIN_NAV: NavItem[] = [
  { to: '/admin', label: 'Overview', end: true },
  { to: '/admin/users', label: 'Users' },
  { to: '/admin/sellers', label: 'Seller Approvals' },
  { to: '/admin/products', label: 'Products' },
  { to: '/admin/orders', label: 'Orders' },
  { to: '/admin/promos', label: 'Promo Codes' },
  { to: '/admin/categories', label: 'Categories' },
  { to: '/admin/brands', label: 'Brands' },
  { to: '/admin/skintypes', label: 'Skin Types' },
]
