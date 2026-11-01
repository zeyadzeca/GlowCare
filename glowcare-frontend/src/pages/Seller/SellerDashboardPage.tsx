import { DashboardShell, StatCard } from '@/components/dashboard/DashboardShell'

export function SellerDashboardPage() {
  return (
    <DashboardShell
      title="Seller Dashboard"
      subtitle="Manage your products, inventory, and orders"
      nav={[
        { to: '/seller', label: 'Overview', end: true },
        { to: '/seller/products', label: 'Products' },
        { to: '/seller/inventory', label: 'Inventory' },
        { to: '/seller/orders', label: 'Orders' },
      ]}
    >
      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard label="Total Products" value="0" />
        <StatCard label="Total Sales" value="0 EGP" />
        <StatCard label="Pending Orders" value="0" />
        <StatCard label="Total Orders" value="0" />
      </div>
    </DashboardShell>
  )
}
