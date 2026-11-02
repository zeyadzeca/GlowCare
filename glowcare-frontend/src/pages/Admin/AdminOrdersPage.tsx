import { useEffect, useState } from 'react'
import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'
import { Button } from '@/components/common/Button'
import { adminApi } from '@/services/api'
import { getErrorMessage } from '@/utils/errors'
import { formatPrice } from '@/utils/format'
import { ADMIN_NAV } from '@/config/adminNav'
import type { Order } from '@/types/api.types'

export function AdminOrdersPage() {
  const [orders, setOrders] = useState<Order[] | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)

  const load = () => {
    setLoading(true)
    setError(null)
    adminApi.getAllOrders()
      .then(setOrders)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    load()
  }, [])

  async function handleStatusUpdate(id: number, status: number) {
    setBusyId(id)
    setError(null)
    try {
      await adminApi.updateOrderStatus(id, status)
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  const statusLabels: Record<number, string> = { 0: 'Pending', 1: 'Processing', 2: 'Shipped', 3: 'Delivered', 4: 'Cancelled' }

  return (
    <DashboardShell
      title="Orders"
      subtitle="View and manage all orders"
      nav={ADMIN_NAV}
    >
      {error && <p className="text-sm text-red-600">{error}</p>}
      {loading ? (
        <div className="card p-8 text-center">
          <Spinner label="Loading orders..." />
        </div>
      ) : orders && orders.length > 0 ? (
        <div className="space-y-4">
          {orders.map((order) => (
            <div key={order.id} className="card p-6">
              <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                <div>
                  <h3 className="font-display text-lg text-royal">Order #{order.id}</h3>
                  <p className="text-sm text-navy/70">Total: {formatPrice(order.totalAmount)}</p>
                  <p className="mt-2 text-xs text-navy/50">Placed: {new Date(order.orderDate).toLocaleString()}</p>
                  <p className="text-xs text-navy/50">Ship to: {order.shippingAddress}</p>
                  <p className="text-xs font-semibold">
                    Status: <span className={order.status === 3 ? 'text-green-600' : order.status === 4 ? 'text-red-600' : 'text-yellow-600'}>{statusLabels[order.status]}</span>
                  </p>
                  <p className="text-xs font-semibold">
                    Payment: <span className={order.paymentStatus === 1 ? 'text-green-600' : 'text-yellow-600'}>{order.paymentStatus === 1 ? 'Paid' : 'Pending'}</span>
                  </p>
                </div>
                <div className="flex gap-2">
                  {order.status === 0 && (
                    <Button
                      variant="ghost"
                      className="text-sm"
                      loading={busyId === order.id}
                      onClick={() => void handleStatusUpdate(order.id, 1)}
                    >
                      Mark Processing
                    </Button>
                  )}
                  {order.status === 1 && (
                    <Button
                      variant="ghost"
                      className="text-sm"
                      loading={busyId === order.id}
                      onClick={() => void handleStatusUpdate(order.id, 2)}
                    >
                      Mark Shipped
                    </Button>
                  )}
                  {order.status === 2 && (
                    <Button
                      variant="ghost"
                      className="text-sm"
                      loading={busyId === order.id}
                      onClick={() => void handleStatusUpdate(order.id, 3)}
                    >
                      Mark Delivered
                    </Button>
                  )}
                  {order.status !== 4 && order.status !== 3 && (
                    <Button
                      variant="ghost"
                      className="text-sm text-red-600"
                      loading={busyId === order.id}
                      onClick={() => void handleStatusUpdate(order.id, 4)}
                    >
                      Cancel
                    </Button>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="text-navy/60">No orders found.</p>
      )}
    </DashboardShell>
  )
}
