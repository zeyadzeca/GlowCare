import { useEffect, useState } from 'react'
import { DashboardShell, StatCard } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'
import { sellerApi } from '@/services/api'
import { formatPrice } from '@/utils/format'
import type { SellerDashboardStats } from '@/types/api.types'

export function SellerDashboardPage() {
  const [stats, setStats] = useState<SellerDashboardStats | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    sellerApi.getSellerDashboard()
      .then((data) => {
        if (!cancelled) setStats(data)
      })
      .catch(() => {
        if (!cancelled) setStats(null)
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [])

  return (
    <DashboardShell
      title="Seller Dashboard"
      subtitle="Manage your products and orders"
      nav={[
        { to: '/seller', label: 'Overview', end: true },
        { to: '/seller/products', label: 'Products' },
        { to: '/seller/orders', label: 'Orders' },
      ]}
    >
      {loading ? (
        <Spinner label="Loading dashboard..." />
      ) : stats ? (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
          <StatCard label="Total Products" value={stats.totalProducts.toString()} />
          <StatCard label="Active Products" value={stats.activeProducts.toString()} />
          <StatCard label="Total Sales" value={formatPrice(stats.totalEarnings)} />
          <StatCard label="Confirmed Sales" value={formatPrice(stats.confirmedEarnings)} />
          <StatCard label="Pending Orders" value={stats.pendingOrderItems.toString()} />
          <StatCard label="Total Orders" value={stats.totalOrderItems.toString()} />
        </div>
      ) : (
        <p className="text-navy/60">Failed to load dashboard statistics.</p>
      )}
    </DashboardShell>
  )
}
