import { useEffect, useState } from 'react'
import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'
import { Button } from '@/components/common/Button'
import { sellerApi } from '@/services/api'
import { getErrorMessage } from '@/utils/errors'
import { formatPrice } from '@/utils/format'
import type { SellerOrderItem } from '@/types/api.types'

export function SellerOrdersPage() {
  const [items, setItems] = useState<SellerOrderItem[] | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)

  const load = () => {
    setLoading(true)
    setError(null)
    sellerApi.getSellerOrderItems()
      .then(setItems)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    load()
  }, [])

  async function handleFulfillment(id: number, fulfilled: boolean) {
    setBusyId(id)
    setError(null)
    try {
      await sellerApi.updateFulfillment(id, fulfilled)
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  return (
    <DashboardShell
      title="My Orders"
      subtitle="View and fulfill customer orders"
      nav={[
        { to: '/seller', label: 'Overview' },
        { to: '/seller/products', label: 'Products' },
        { to: '/seller/orders', label: 'Orders', end: true },
      ]}
    >
      {error && <p className="text-sm text-red-600">{error}</p>}
      {loading ? (
        <div className="card p-8 text-center">
          <Spinner label="Loading orders..." />
        </div>
      ) : items && items.length > 0 ? (
        <div className="space-y-4">
          {items.map((item) => (
            <div key={item.orderItemId} className="card p-6">
              <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                <div>
                  <h3 className="font-display text-lg text-royal">{item.productName}</h3>
                  <p className="text-sm text-navy/70">Order #{item.orderId}</p>
                  <p className="text-sm text-navy/70">Customer: {item.customerName}</p>
                  <p className="text-sm text-navy/70">Qty: {item.quantity} × {formatPrice(item.unitPrice)}</p>
                  <p className="text-sm font-semibold text-royal">Total: {formatPrice(item.unitPrice * item.quantity)}</p>
                  <p className="mt-2 text-xs text-navy/50">Ordered: {new Date(item.orderDate).toLocaleString()}</p>
                  <p className="text-xs text-navy/50">Ship to: {item.shippingAddress}</p>
                  <p className="text-xs font-semibold">
                    Order Status: <span className={item.orderStatus === 3 ? 'text-green-600' : 'text-yellow-600'}>{item.orderStatus === 0 ? 'Pending' : item.orderStatus === 1 ? 'Processing' : item.orderStatus === 2 ? 'Shipped' : 'Delivered'}</span>
                  </p>
                  <p className="text-xs font-semibold">
                    Fulfillment: <span className={item.fulfillmentStatus === 1 ? 'text-green-600' : 'text-yellow-600'}>{item.fulfillmentStatus === 1 ? 'Fulfilled' : 'Not Fulfilled'}</span>
                  </p>
                </div>
                <Button
                  variant="ghost"
                  className="text-sm"
                  loading={busyId === item.orderItemId}
                  onClick={() => void handleFulfillment(item.orderItemId, item.fulfillmentStatus !== 1)}
                >
                  {item.fulfillmentStatus === 1 ? 'Mark Unfulfilled' : 'Mark Fulfilled'}
                </Button>
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
