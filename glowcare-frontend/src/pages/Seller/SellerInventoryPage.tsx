import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'

export function SellerInventoryPage() {
  return (
    <DashboardShell
      title="Inventory"
      subtitle="Manage stock levels"
      nav={[
        { to: '/seller', label: 'Overview' },
        { to: '/seller/products', label: 'Products' },
        { to: '/seller/inventory', label: 'Inventory', end: true },
        { to: '/seller/orders', label: 'Orders' },
      ]}
    >
      <div className="card p-8 text-center">
        <Spinner label="Loading inventory..." />
      </div>
    </DashboardShell>
  )
}
