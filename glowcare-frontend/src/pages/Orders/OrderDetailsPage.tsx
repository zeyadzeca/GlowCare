import { useCallback, useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { orderApi } from '@/services/api'
import type { Order } from '@/types/api.types'
import { OrderStatus } from '@/types/enums'
import { formatDateTime, formatPrice } from '@/utils/format'
import { getErrorMessage } from '@/utils/errors'
import { orderStatusLabel, paymentMethodLabel } from '@/utils/labels'
import { Button } from '@/components/common/Button'
import { ErrorState, Spinner } from '@/components/common/States'
import { FulfillmentBadge, OrderStatusBadge, PaymentStatusBadge } from '@/components/common/StatusBadge'

export function OrderDetailsPage() {
  const { id } = useParams()
  const orderId = Number(id)
  const [order, setOrder] = useState<Order | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  const load = useCallback(async () => {
    if (!Number.isFinite(orderId)) {
      setError('That order does not exist.')
      setLoading(false)
      return
    }
    setLoading(true)
    setError(null)
    try {
      setOrder(await orderApi.getOrder(orderId))
    } catch (err) {
      setError(getErrorMessage(err, 'Could not load this order.'))
    } finally {
      setLoading(false)
    }
  }, [orderId])

  useEffect(() => {
    void load()
  }, [load])

  async function cancel() {
    if (!order) return
    setBusy(true)
    try {
      setOrder(await orderApi.cancelOrder(order.id))
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  if (loading) return <div className="shell"><Spinner label="Loading order..." /></div>
  if (error || !order) {
    return (
      <div className="shell py-12">
        <ErrorState message={error ?? 'Order not found.'} onRetry={() => void load()} />
        <div className="mt-6 text-center">
          <Link to="/orders" className="btn-ghost">Back to orders</Link>
        </div>
      </div>
    )
  }

  const canCancel = order.status === OrderStatus.Pending

  return (
    <div className="shell py-10">
      <Link to="/orders" className="text-sm font-semibold text-royal hover:underline">← All orders</Link>
      <div className="mt-4 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="heading-display uppercase">Order #{order.id}</h1>
          <p className="mt-2 text-navy/60">{formatDateTime(order.orderDate)}</p>
        </div>
        <div className="flex flex-wrap gap-2">
          <OrderStatusBadge status={order.status} />
          <PaymentStatusBadge status={order.paymentStatus} />
        </div>
      </div>

      {error && <p className="mt-4 text-sm text-red-700">{error}</p>}

      <div className="mt-8 grid gap-6 lg:grid-cols-2">
        <section className="card p-6">
          <h2 className="font-display text-xl text-royal">Items</h2>
          <ul className="mt-4 space-y-4">
            {order.items.map((item) => (
              <li key={item.id} className="flex gap-3">
                <div className="h-16 w-16 overflow-hidden rounded-xl bg-sky/40">
                  {item.productImageUrl && <img src={item.productImageUrl} alt="" className="h-full w-full object-cover" />}
                </div>
                <div className="flex-1">
                  <Link to={`/products/${item.productId}`} className="font-semibold text-royal hover:underline">
                    {item.productName}
                  </Link>
                  <p className="text-sm text-navy/60">
                    {item.quantity} × {formatPrice(item.unitPrice)} · {item.sellerStoreName}
                  </p>
                  <FulfillmentBadge status={item.fulfillmentStatus} />
                </div>
                <p className="font-semibold">{formatPrice(item.lineTotal)}</p>
              </li>
            ))}
          </ul>
        </section>

        <section className="card p-6">
          <h2 className="font-display text-xl text-royal">Summary</h2>
          <dl className="mt-4 space-y-2 text-sm">
            <div className="flex justify-between"><dt>Subtotal</dt><dd>{formatPrice(order.subtotal)}</dd></div>
            <div className="flex justify-between"><dt>Shipping</dt><dd>{formatPrice(order.shippingCost)}</dd></div>
            <div className="flex justify-between"><dt>Tax</dt><dd>{formatPrice(order.taxAmount)}</dd></div>
            <div className="flex justify-between"><dt>Discount</dt><dd>-{formatPrice(order.discountAmount)}</dd></div>
            <div className="flex justify-between font-display text-lg text-royal">
              <dt>Total</dt>
              <dd>{formatPrice(order.totalAmount)}</dd>
            </div>
          </dl>
          <p className="mt-4 text-sm text-navy/70">
            {paymentMethodLabel[order.paymentMethod]}
            {order.promoCode ? ` · Promo ${order.promoCode}` : ''}
          </p>
          <p className="mt-2 text-sm text-navy/70">Ship to: {order.shippingAddress}</p>
          <p className="mt-1 text-sm text-navy/70">Contact: {order.phoneNumber}</p>
          {canCancel && (
            <Button type="button" variant="ghost" loading={busy} className="mt-6" onClick={() => void cancel()}>
              Cancel order
            </Button>
          )}
        </section>
      </div>

      <section className="card mt-6 p-6">
        <h2 className="font-display text-xl text-royal">Status tracking</h2>
        <ol className="mt-4 space-y-3">
          {order.statusHistory.map((entry) => (
            <li key={entry.id} className="border-l-2 border-yellow pl-4">
              <p className="font-semibold text-navy">{orderStatusLabel[entry.status]}</p>
              <p className="text-sm text-navy/60">{formatDateTime(entry.changedAt)}</p>
              {entry.notes && <p className="text-sm text-navy/70">{entry.notes}</p>}
            </li>
          ))}
        </ol>
      </section>
    </div>
  )
}
