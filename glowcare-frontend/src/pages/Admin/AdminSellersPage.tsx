import { useEffect, useState } from 'react'
import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'
import { Button } from '@/components/common/Button'
import { adminApi } from '@/services/api'
import { getErrorMessage } from '@/utils/errors'
import { ADMIN_NAV } from '@/config/adminNav'
import type { SellerApplication } from '@/types/api.types'

export function AdminSellersPage() {
  const [applications, setApplications] = useState<SellerApplication[] | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)

  const load = () => {
    setLoading(true)
    setError(null)
    adminApi.getSellerApplications()
      .then(setApplications)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    load()
  }, [])

  async function handleApproval(id: number, approved: boolean) {
    setBusyId(id)
    setError(null)
    try {
      await adminApi.updateSellerApproval(id, approved)
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  return (
    <DashboardShell
      title="Seller Approvals"
      subtitle="Review and approve seller applications"
      nav={ADMIN_NAV}
    >
      {error && <p className="text-sm text-red-600">{error}</p>}
      {loading ? (
        <div className="card p-8 text-center">
          <Spinner label="Loading seller applications..." />
        </div>
      ) : applications && applications.length > 0 ? (
        <div className="space-y-4">
          {applications.map((app) => (
            <div key={app.sellerProfileId} className="card p-6">
              <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                <div className="flex gap-4">
                  {app.logo && (
                    <img src={app.logo} alt="" className="h-16 w-16 rounded-full object-cover" />
                  )}
                  <div>
                    <h3 className="font-display text-lg text-royal">{app.storeName}</h3>
                    <p className="text-sm text-navy/70">{app.fullName}</p>
                    <p className="text-sm text-navy/70">{app.email}</p>
                    <p className="mt-2 text-sm text-navy">{app.description}</p>
                    <p className="mt-1 text-xs text-navy/50">Applied: {new Date(app.createdAt).toLocaleDateString()}</p>
                    <p className="text-xs font-semibold">
                      Status: <span className={app.approvalStatus === 1 ? 'text-green-600' : app.approvalStatus === 2 ? 'text-red-600' : 'text-yellow-600'}>{app.approvalStatus === 0 ? 'Pending' : app.approvalStatus === 1 ? 'Approved' : 'Rejected'}</span>
                    </p>
                  </div>
                </div>
                {app.approvalStatus === 0 && (
                  <div className="flex gap-2">
                    <Button
                      variant="ghost"
                      className="text-sm"
                      loading={busyId === app.sellerProfileId}
                      onClick={() => void handleApproval(app.sellerProfileId, true)}
                    >
                      Approve
                    </Button>
                    <Button
                      variant="ghost"
                      className="text-sm text-red-600"
                      loading={busyId === app.sellerProfileId}
                      onClick={() => void handleApproval(app.sellerProfileId, false)}
                    >
                      Reject
                    </Button>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="text-navy/60">No seller applications found.</p>
      )}
    </DashboardShell>
  )
}
