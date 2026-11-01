import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'

export function SellerProductsPage() {
  return (
    <DashboardShell
      title="My Products"
      subtitle="Manage your product catalog"
      nav={[
        { to: '/seller', label: 'Overview' },
        { to: '/seller/products', label: 'Products', end: true },
        { to: '/seller/inventory', label: 'Inventory' },
        { to: '/seller/orders', label: 'Orders' },
      ]}
    >
      <div className="card p-8 text-center">
        <Spinner label="Loading products..." />
      </div>
    </DashboardShell>
  )
}
