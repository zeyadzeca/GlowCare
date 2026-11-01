import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'

export function AdminSellersPage() {
  return (
    <DashboardShell
      title="Seller Approvals"
      subtitle="Review and approve seller applications"
      nav={[
        { to: '/admin', label: 'Overview' },
        { to: '/admin/users', label: 'Users' },
        { to: '/admin/sellers', label: 'Seller Approvals', end: true },
        { to: '/admin/products', label: 'Products' },
        { to: '/admin/orders', label: 'Orders' },
        { to: '/admin/promos', label: 'Promo Codes' },
        { to: '/admin/banners', label: 'Banners' },
      ]}
    >
      <div className="card p-8 text-center">
        <Spinner label="Loading seller applications..." />
      </div>
    </DashboardShell>
  )
}
