import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'

export function SellerOrdersPage() {
  return (
    <DashboardShell
      title="My Orders"
      subtitle="View and fulfill customer orders"
      nav={[
        { to: '/seller', label: 'Overview' },
        { to: '/seller/products', label: 'Products' },
        { to: '/seller/inventory', label: 'Inventory' },
        { to: '/seller/orders', label: 'Orders', end: true },
      ]}
    >
      <div className="card p-8 text-center">
        <Spinner label="Loading orders..." />
      </div>
    </DashboardShell>
  )
}
