import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { orderApi } from '@/services/api'
import type { Order } from '@/types/api.types'
import { formatDate, formatPrice } from '@/utils/format'
import { getErrorMessage } from '@/utils/errors'
import { paymentMethodLabel } from '@/utils/labels'
import { EmptyState, ErrorState, Spinner } from '@/components/common/States'
import { OrderStatusBadge, PaymentStatusBadge } from '@/components/common/StatusBadge'

export function OrdersPage() {
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      setOrders(await orderApi.getMyOrders())
    } catch (err) {
      setError(getErrorMessage(err, 'Could not load orders.'))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  if (loading) return <div className="shell"><Spinner label="Loading orders..." /></div>
  if (error) {
    return (
      <div className="shell py-12">
        <ErrorState message={error} onRetry={() => void load()} />
      </div>
    )
  }

  return (
    <div className="shell py-10">
      <h1 className="heading-display uppercase">My orders</h1>
      <p className="mt-2 text-navy/60">Track every GlowCare order from this account.</p>

      {orders.length === 0 ? (
        <div className="mt-8">
          <EmptyState
            title="No orders yet"
            message="When you check out, your orders will appear here."
            actionLabel="Start shopping"
            actionTo="/products"
          />
        </div>
      ) : (
        <ul className="mt-8 space-y-4">
          {orders.map((order) => (
            <li key={order.id} className="card p-5">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <Link to={`/orders/${order.id}`} className="font-display text-xl text-royal hover:underline">
                    Order #{order.id}
                  </Link>
                  <p className="mt-1 text-sm text-navy/60">{formatDate(order.orderDate)}</p>
                </div>
                <div className="flex flex-wrap gap-2">
                  <OrderStatusBadge status={order.status} />
                  <PaymentStatusBadge status={order.paymentStatus} />
                </div>
              </div>
              <p className="mt-3 text-sm text-navy/70">
                {order.items.length} item{order.items.length === 1 ? '' : 's'} · {paymentMethodLabel[order.paymentMethod]}
              </p>
              <p className="mt-1 font-display text-lg text-royal">{formatPrice(order.totalAmount)}</p>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
