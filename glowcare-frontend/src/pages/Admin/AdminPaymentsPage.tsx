import { useEffect, useState } from 'react'
import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'
import { adminApi } from '@/services/api'
import { getErrorMessage } from '@/utils/errors'
import { formatPrice } from '@/utils/format'

export function AdminPaymentsPage() {
  const [payments, setPayments] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = () => {
    setLoading(true)
    setError(null)
    adminApi.getAllPayments()
      .then(setPayments)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }

  useEffect(() => { load() }, [])

  return (
    <DashboardShell
      title="Payments"
      subtitle="View all payment records"
      nav={[
        { to: '/admin', label: 'Overview' },
        { to: '/admin/payments', label: 'Payments', end: true },
        { to: '/admin/reviews', label: 'Reviews' },
        { to: '/admin/orders', label: 'Orders' },
      ]}
    >
      {error && <p className="text-sm text-red-600">{error}</p>}
      {loading ? <div className="card p-8 text-center"><Spinner label="Loading..." /></div> : (
        <div className="card overflow-hidden">
          <table className="w-full">
            <thead className="bg-navy/5">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">ID</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Order ID</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Amount</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Method</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Status</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Date</th>
              </tr>
            </thead>
            <tbody>
              {payments.map((p) => (
                <tr key={p.id} className="border-t border-navy/10">
                  <td className="px-4 py-3 text-sm">{p.id}</td>
                  <td className="px-4 py-3 text-sm">{p.orderId}</td>
                  <td className="px-4 py-3 text-sm">{formatPrice(p.amount)}</td>
                  <td className="px-4 py-3 text-sm">{p.paymentMethod}</td>
                  <td className="px-4 py-3 text-sm">{p.paymentStatus}</td>
                  <td className="px-4 py-3 text-sm">{new Date(p.createdAt).toLocaleDateString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </DashboardShell>
  )
}
