import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'

export function AdminProductsPage() {
  return (
    <DashboardShell
      title="Products"
      subtitle="Manage all products"
      nav={[
        { to: '/admin', label: 'Overview' },
        { to: '/admin/users', label: 'Users' },
        { to: '/admin/sellers', label: 'Seller Approvals' },
        { to: '/admin/products', label: 'Products', end: true },
        { to: '/admin/orders', label: 'Orders' },
        { to: '/admin/promos', label: 'Promo Codes' },
        { to: '/admin/banners', label: 'Banners' },
      ]}
    >
      <div className="card p-8 text-center">
        <Spinner label="Loading products..." />
      </div>
    </DashboardShell>
  )
}
