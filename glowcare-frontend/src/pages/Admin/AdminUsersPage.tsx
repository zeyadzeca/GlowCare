import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'

export function AdminUsersPage() {
  return (
    <DashboardShell
      title="Users"
      subtitle="Manage user accounts"
      nav={[
        { to: '/admin', label: 'Overview' },
        { to: '/admin/users', label: 'Users', end: true },
        { to: '/admin/sellers', label: 'Seller Approvals' },
        { to: '/admin/products', label: 'Products' },
        { to: '/admin/orders', label: 'Orders' },
        { to: '/admin/promos', label: 'Promo Codes' },
        { to: '/admin/banners', label: 'Banners' },
      ]}
    >
      <div className="card p-8 text-center">
        <Spinner label="Loading users..." />
      </div>
    </DashboardShell>
  )
}
