import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'

export function AdminBannersPage() {
  return (
    <DashboardShell
      title="Banners"
      subtitle="Manage promotional banners"
      nav={[
        { to: '/admin', label: 'Overview' },
        { to: '/admin/users', label: 'Users' },
        { to: '/admin/sellers', label: 'Seller Approvals' },
        { to: '/admin/products', label: 'Products' },
        { to: '/admin/orders', label: 'Orders' },
        { to: '/admin/promos', label: 'Promo Codes' },
        { to: '/admin/banners', label: 'Banners', end: true },
      ]}
    >
      <div className="card p-8 text-center">
        <Spinner label="Loading banners..." />
      </div>
    </DashboardShell>
  )
}
