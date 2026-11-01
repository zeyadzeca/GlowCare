import { DashboardShell, StatCard } from '@/components/dashboard/DashboardShell'

export function AdminDashboardPage() {
  return (
    <DashboardShell
      title="Admin Dashboard"
      subtitle="Manage users, approvals, and content"
      nav={[
        { to: '/admin', label: 'Overview', end: true },
        { to: '/admin/users', label: 'Users' },
        { to: '/admin/sellers', label: 'Seller Approvals' },
        { to: '/admin/products', label: 'Products' },
        { to: '/admin/orders', label: 'Orders' },
        { to: '/admin/promos', label: 'Promo Codes' },
        { to: '/admin/banners', label: 'Banners' },
      ]}
    >
      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard label="Total Users" value="0" />
        <StatCard label="Pending Sellers" value="0" />
        <StatCard label="Total Products" value="0" />
        <StatCard label="Total Orders" value="0" />
      </div>
    </DashboardShell>
  )
}
