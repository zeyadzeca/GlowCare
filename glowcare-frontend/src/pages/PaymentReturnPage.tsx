import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { orderApi } from '@/services/api'
import { PaymentStatus } from '@/types/enums'
import { Spinner } from '@/components/common/States'

/**
 * Landing page after the Paymob hosted checkout redirects back.
 *
 * IMPORTANT: the payment is NOT finalized here. Paymob calls the backend's
 * anonymous HMAC-verified callback (/api/payments/callback) server-to-server,
 * and that is the only thing that marks an order Paid. This page simply checks
 * my most recent order's payment status and reports the result.
 */
export function PaymentReturnPage() {
  const navigate = useNavigate()
  const [status, setStatus] = useState<'processing' | 'success' | 'failed'>('processing')

  useEffect(() => {
    let cancelled = false

    async function checkStatus() {
      // The callback can land a moment after we do - poll a few times.
      for (let attempt = 0; attempt < 5; attempt++) {
        try {
          const orders = await orderApi.getMyOrders()
          const latest = orders[0]
          if (latest && latest.paymentStatus === PaymentStatus.Paid) {
            if (!cancelled) {
              setStatus('success')
              setTimeout(() => navigate(`/orders/${latest.id}`, { replace: true }), 2000)
            }
            return
          }
        } catch {
          // Keep polling; final failure is reported after the last attempt.
        }
        await new Promise((resolve) => setTimeout(resolve, 1500))
      }
      if (!cancelled) {
        setStatus('failed')
        setTimeout(() => navigate('/orders', { replace: true }), 3000)
      }
    }

    void checkStatus()
    return () => {
      cancelled = true
    }
  }, [navigate])

  if (status === 'processing') {
    return (
      <div className="shell flex min-h-[50vh] items-center justify-center">
        <Spinner label="Processing payment..." />
      </div>
    )
  }

  return (
    <div className="shell flex min-h-[50vh] items-center justify-center">
      <div className="card max-w-md p-8 text-center">
        {status === 'success' ? (
          <>
            <h1 className="heading-display text-royal">Payment Successful</h1>
            <p className="mt-4 text-navy/70">Redirecting to your order...</p>
          </>
        ) : (
          <>
            <h1 className="heading-display text-royal">Payment Pending</h1>
            <p className="mt-4 text-navy/70">
              We could not confirm your payment yet. Check your orders page in a moment.
            </p>
            <p className="mt-2 text-sm text-navy/50">Redirecting to your orders...</p>
          </>
        )}
      </div>
    </div>
  )
}
