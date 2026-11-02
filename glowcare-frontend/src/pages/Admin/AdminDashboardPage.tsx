import { useEffect, useState } from 'react'
import { DashboardShell, StatCard } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'
import { adminApi } from '@/services/api'
import { formatPrice } from '@/utils/format'
import { ADMIN_NAV } from '@/config/adminNav'
import type { AdminDashboardStats } from '@/types/api.types'

export function AdminDashboardPage() {
  const [stats, setStats] = useState<AdminDashboardStats | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    adminApi.getAdminDashboard()
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
      title="Admin Dashboard"
      subtitle="Manage users, approvals, and content"
      nav={ADMIN_NAV}
    >
      {loading ? (
        <Spinner label="Loading dashboard..." />
      ) : stats ? (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
          <StatCard label="Total Users" value={stats.totalUsers.toString()} />
          <StatCard label="Pending Sellers" value={stats.pendingSellerApplications.toString()} />
          <StatCard label="Total Products" value={stats.totalProducts.toString()} />
          <StatCard label="Total Orders" value={stats.totalOrders.toString()} />
          <StatCard label="Active Products" value={stats.activeProducts.toString()} />
          <StatCard label="Total Sellers" value={stats.totalSellers.toString()} />
          <StatCard label="Pending Orders" value={stats.pendingOrders.toString()} />
          <StatCard label="Total Revenue" value={formatPrice(stats.totalRevenue)} />
        </div>
      ) : (
        <p className="text-navy/60">Failed to load dashboard statistics.</p>
      )}
    </DashboardShell>
  )
}
