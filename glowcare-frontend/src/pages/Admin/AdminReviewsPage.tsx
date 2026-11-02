import { useEffect, useState } from 'react'
import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'
import { Button } from '@/components/common/Button'
import { adminApi } from '@/services/api'
import { getErrorMessage } from '@/utils/errors'

export function AdminReviewsPage() {
  const [reviews, setReviews] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)

  const load = () => {
    setLoading(true)
    setError(null)
    adminApi.getAllReviews()
      .then(setReviews)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }

  useEffect(() => { load() }, [])

  async function handleDelete(id: number) {
    if (!confirm('Delete this review?')) return
    setBusyId(id)
    setError(null)
    try {
      await adminApi.adminDeleteReview(id)
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  return (
    <DashboardShell
      title="Reviews"
      subtitle="Manage customer reviews"
      nav={[
        { to: '/admin', label: 'Overview' },
        { to: '/admin/payments', label: 'Payments' },
        { to: '/admin/reviews', label: 'Reviews', end: true },
        { to: '/admin/orders', label: 'Orders' },
      ]}
    >
      {error && <p className="text-sm text-red-600">{error}</p>}
      {loading ? <div className="card p-8 text-center"><Spinner label="Loading..." /></div> : (
        <div className="card overflow-hidden">
          <table className="w-full">
            <thead className="bg-navy/5">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Product</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">User</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Rating</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Comment</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Date</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Actions</th>
              </tr>
            </thead>
            <tbody>
              {reviews.map((r) => (
                <tr key={r.id} className="border-t border-navy/10">
                  <td className="px-4 py-3 text-sm">{r.productId}</td>
                  <td className="px-4 py-3 text-sm">{r.userName}</td>
                  <td className="px-4 py-3 text-sm">{'★'.repeat(r.rating)}</td>
                  <td className="px-4 py-3 text-sm max-w-xs truncate">{r.comment}</td>
                  <td className="px-4 py-3 text-sm">{new Date(r.createdAt).toLocaleDateString()}</td>
                  <td className="px-4 py-3">
                    <Button variant="ghost" className="text-sm text-red-600" loading={busyId === r.id} onClick={() => void handleDelete(r.id)}>Delete</Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </DashboardShell>
  )
}
