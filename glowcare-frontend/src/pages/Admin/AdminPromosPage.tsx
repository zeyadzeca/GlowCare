import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'

export function AdminPromosPage() {
  return (
    <DashboardShell
      title="Promo Codes"
      subtitle="Manage discount codes"
      nav={[
        { to: '/admin', label: 'Overview' },
        { to: '/admin/users', label: 'Users' },
        { to: '/admin/sellers', label: 'Seller Approvals' },
        { to: '/admin/products', label: 'Products' },
        { to: '/admin/orders', label: 'Orders' },
        { to: '/admin/promos', label: 'Promo Codes', end: true },
        { to: '/admin/banners', label: 'Banners' },
      ]}
    >
      <div className="card p-8 text-center">
        <Spinner label="Loading promo codes..." />
      </div>
    </DashboardShell>
  )
}
