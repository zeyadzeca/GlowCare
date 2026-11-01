import {
  approvalStatusBadge,
  approvalStatusLabel,
  fulfillmentStatusLabel,
  orderStatusBadge,
  orderStatusLabel,
  paymentStatusBadge,
  paymentStatusLabel,
} from '@/utils/labels'

function Badge({ className, children }: { className: string; children: string }) {
  return <span className={`badge ${className}`}>{children}</span>
}

export function OrderStatusBadge({ status }: { status: number }) {
  return <Badge className={orderStatusBadge[status] ?? 'bg-off-white-deep text-navy'}>{orderStatusLabel[status] ?? 'Unknown'}</Badge>
}

export function PaymentStatusBadge({ status }: { status: number }) {
  return <Badge className={paymentStatusBadge[status] ?? 'bg-off-white-deep text-navy'}>{paymentStatusLabel[status] ?? 'Unknown'}</Badge>
}

export function ApprovalBadge({ status }: { status: number }) {
  return <Badge className={approvalStatusBadge[status] ?? 'bg-off-white-deep text-navy'}>{approvalStatusLabel[status] ?? 'Unknown'}</Badge>
}

export function FulfillmentBadge({ status }: { status: number }) {
  const fulfilled = status === 1
  return (
    <Badge className={fulfilled ? 'bg-green-200 text-green-900' : 'bg-yellow text-royal'}>
      {fulfillmentStatusLabel[status] ?? 'Unknown'}
    </Badge>
  )
}
