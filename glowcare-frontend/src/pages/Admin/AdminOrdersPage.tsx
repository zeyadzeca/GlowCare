import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'

export function AdminOrdersPage() {
  return (
    <DashboardShell
      title="Orders"
      subtitle="View and manage all orders"
      nav={[
        { to: '/admin', label: 'Overview' },
        { to: '/admin/users', label: 'Users' },
        { to: '/admin/sellers', label: 'Seller Approvals' },
        { to: '/admin/products', label: 'Products' },
        { to: '/admin/orders', label: 'Orders', end: true },
        { to: '/admin/promos', label: 'Promo Codes' },
        { to: '/admin/banners', label: 'Banners' },
      ]}
    >
      <div className="card p-8 text-center">
        <Spinner label="Loading orders..." />
      </div>
    </DashboardShell>
  )
}
