import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { orderApi, promoBannerApi, userApi } from '@/services/api'
import { useCart } from '@/context/CartContext'
import { PaymentMethod } from '@/types/enums'
import type { Address, AddressRequest, OrderSummary } from '@/types/api.types'
import { formatPrice } from '@/utils/format'
import { getErrorMessage } from '@/utils/errors'
import { Button } from '@/components/common/Button'
import { Input } from '@/components/common/FormField'
import { EmptyState, Spinner } from '@/components/common/States'

const emptyAddress: AddressRequest = { city: '', street: '', buildingNumber: '' }

function trimAddress(form: AddressRequest): AddressRequest {
  return {
    city: form.city.trim(),
    street: form.street.trim(),
    buildingNumber: form.buildingNumber.trim(),
  }
}

function isCompleteAddress(form: AddressRequest): boolean {
  const trimmed = trimAddress(form)
  return Boolean(trimmed.city && trimmed.street && trimmed.buildingNumber)
}

function sameAddress(a: AddressRequest, b: AddressRequest): boolean {
  const left = trimAddress(a)
  const right = trimAddress(b)
  return (
    left.city.toLowerCase() === right.city.toLowerCase() &&
    left.street.toLowerCase() === right.street.toLowerCase() &&
    left.buildingNumber.toLowerCase() === right.buildingNumber.toLowerCase()
  )
}

export function CheckoutPage() {
  const { cart, loading: cartLoading, refresh, isGuest } = useCart()
  const navigate = useNavigate()

  const [addresses, setAddresses] = useState<Address[]>([])
  const [addressId, setAddressId] = useState<number | ''>('')
  const [newAddress, setNewAddress] = useState<AddressRequest>(emptyAddress)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [promoCode, setPromoCode] = useState('')
  const [promoNotice, setPromoNotice] = useState<string | null>(null)
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>(PaymentMethod.CashOnDelivery)
  const [summary, setSummary] = useState<OrderSummary | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [addressBusy, setAddressBusy] = useState(false)
  const [loading, setLoading] = useState(true)

  const selectAddress = useCallback((id: number) => {
    setAddressId(id)
    setError(null)
  }, [])

  const loadAddresses = useCallback(async () => {
    const list = await userApi.getAddresses()
    setAddresses(list)
    setAddressId((current) => {
      if (current !== '' && list.some((address) => address.id === current)) return current
      return list[0]?.id ?? ''
    })
  }, [])

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    loadAddresses()
      .catch((err) => {
        if (!cancelled) setError(getErrorMessage(err))
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [loadAddresses])

  const preview = useCallback(async () => {
    if (addressId === '') {
      setSummary(null)
      return
    }
    try {
      const result = await orderApi.previewOrder({
        addressId,
        paymentMethod,
        promoCode: promoCode.trim() || null,
      })
      setSummary(result)
      setError(null)
    } catch (err) {
      setSummary(null)
      setError(getErrorMessage(err, 'Could not calculate totals.'))
    }
  }, [addressId, paymentMethod, promoCode])

  useEffect(() => {
    void preview()
  }, [preview])

  function applySavedAddress(saved: Address) {
    setAddresses((list) => {
      const exists = list.some((address) => address.id === saved.id)
      return exists ? list.map((address) => (address.id === saved.id ? saved : address)) : [...list, saved]
    })
    selectAddress(saved.id)
    setNewAddress(emptyAddress)
    setEditingId(null)
  }

  async function persistDraft(): Promise<Address | null> {
    const payload = trimAddress(newAddress)
    if (!isCompleteAddress(payload)) {
      setError('Enter city, street, and building number.')
      return null
    }

    const duplicate = addresses.find((address) => address.id !== editingId && sameAddress(address, payload))
    if (duplicate && editingId === null) {
      applySavedAddress(duplicate)
      return duplicate
    }

    setAddressBusy(true)
    setError(null)
    try {
      const saved = editingId
        ? await userApi.updateAddress(editingId, payload)
        : await userApi.addAddress(payload)
      applySavedAddress(saved)
      return saved
    } catch (err) {
      setError(getErrorMessage(err, 'Could not save the address.'))
      return null
    } finally {
      setAddressBusy(false)
    }
  }

  async function removeAddress(id: number) {
    setAddressBusy(true)
    setError(null)
    try {
      await userApi.deleteAddress(id)
      const remaining = addresses.filter((address) => address.id !== id)
      setAddresses(remaining)
      setAddressId((current) => (current === id ? remaining[0]?.id ?? '' : current))
      if (editingId === id) {
        setEditingId(null)
        setNewAddress(emptyAddress)
      }
    } catch (err) {
      setError(getErrorMessage(err, 'Could not delete the address.'))
    } finally {
      setAddressBusy(false)
    }
  }

  async function applyPromo() {
    setPromoNotice(null)
    if (!cart) return
    try {
      const result = await promoBannerApi.validatePromoCode({
        code: promoCode.trim(),
        subtotal: cart.subtotal,
      })
      setPromoNotice(result.message)
      await preview()
    } catch (err) {
      setPromoNotice(getErrorMessage(err, 'Could not validate that code.'))
    }
  }

  async function placeOrder(event: FormEvent) {
    event.preventDefault()
    let selectedId = addressId
    if (selectedId === '') {
      if (!isCompleteAddress(newAddress)) {
        setError('Choose a shipping address or enter a new one.')
        return
      }
      const saved = await persistDraft()
      if (!saved) return
      selectedId = saved.id
    }

    setSubmitting(true)
    setError(null)
    const payload = {
      addressId: selectedId,
      paymentMethod,
      promoCode: promoCode.trim() || null,
    }
    try {
      if (paymentMethod === PaymentMethod.CreditCard) {
        const initiation = await orderApi.initiatePayment(payload)
        await refresh()
        window.location.href = initiation.paymentUrl
        return
      }
      const order = await orderApi.placeCodOrder(payload)
      await refresh()
      navigate(`/orders/${order.id}`, { replace: true })
    } catch (err) {
      setError(getErrorMessage(err, 'Could not place the order.'))
    } finally {
      setSubmitting(false)
    }
  }

  if (cartLoading || loading) return <div className="shell"><Spinner label="Preparing checkout..." /></div>

  if (isGuest) {
    return (
      <div className="shell py-12">
        <EmptyState
          title="Sign in to checkout"
          message="Please create an account or sign in to complete your purchase."
          actionLabel="Sign in"
          actionTo="/login"
        />
      </div>
    )
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="shell py-12">
        <EmptyState
          title="Nothing to check out"
          message="Add products to your cart before placing an order."
          actionLabel="Shop products"
          actionTo="/products"
        />
      </div>
    )
  }

  const busy = submitting || addressBusy

  return (
    <div className="shell py-10">
      <h1 className="heading-display uppercase">Checkout</h1>
      <p className="mt-2 text-navy/60">Totals are calculated by the server. Card details never touch GlowCare.</p>

      {error && (
        <div className="mt-4 rounded-xl bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
          {error}
        </div>
      )}

      <form onSubmit={placeOrder} className="mt-8 grid gap-8 lg:grid-cols-[1fr_22rem]">
        <div className="space-y-6">
          <section className="card p-6">
            <h2 className="font-display text-xl text-royal">Shipping address</h2>
            {addresses.length === 0 ? (
              <p className="mt-2 text-sm text-navy/70">Add an address to continue, or enter one below and proceed.</p>
            ) : (
              <ul className="mt-4 space-y-2">
                {addresses.map((address) => (
                  <li key={address.id}>
                    <label className="flex cursor-pointer items-start gap-3 rounded-xl border border-navy/10 p-3 has-[:checked]:border-royal has-[:checked]:bg-royal/5">
                      <input
                        type="radio"
                        name="address"
                        checked={addressId === address.id}
                        onChange={() => selectAddress(address.id)}
                        className="mt-1"
                      />
                      <span className="flex-1">
                        <span className="font-semibold text-navy">{address.street}, {address.buildingNumber}</span>
                        <span className="block text-sm text-navy/60">{address.city}</span>
                      </span>
                      <span className="flex gap-2">
                        <button
                          type="button"
                          className="text-sm font-semibold text-royal"
                          disabled={addressBusy}
                          onClick={() => {
                            setEditingId(address.id)
                            setNewAddress({
                              city: address.city,
                              street: address.street,
                              buildingNumber: address.buildingNumber,
                            })
                            selectAddress(address.id)
                          }}
                        >
                          Edit
                        </button>
                        <button
                          type="button"
                          className="text-sm text-navy/60"
                          disabled={addressBusy}
                          onClick={() => void removeAddress(address.id)}
                        >
                          Delete
                        </button>
                      </span>
                    </label>
                  </li>
                ))}
              </ul>
            )}

            <div className="mt-6 border-t border-navy/10 pt-4">
              <p className="text-sm font-semibold text-royal">{editingId ? 'Edit address' : 'Add address'}</p>
              <div className="mt-3 grid gap-3 sm:grid-cols-3">
                <Input label="City" value={newAddress.city} onChange={(e) => setNewAddress({ ...newAddress, city: e.target.value })} />
                <Input label="Street" value={newAddress.street} onChange={(e) => setNewAddress({ ...newAddress, street: e.target.value })} />
                <Input label="Building" value={newAddress.buildingNumber} onChange={(e) => setNewAddress({ ...newAddress, buildingNumber: e.target.value })} />
              </div>
              <div className="flex flex-wrap gap-3">
                <Button type="button" variant="ghost" className="py-2 text-sm" loading={addressBusy} disabled={busy} onClick={() => void persistDraft()}>
                  {editingId ? 'Update address' : 'Save address'}
                </Button>
                {editingId && (
                  <Button
                    type="button"
                    variant="ghost"
                    className="py-2 text-sm"
                    disabled={busy}
                    onClick={() => {
                      setEditingId(null)
                      setNewAddress(emptyAddress)
                    }}
                  >
                    Cancel
                  </Button>
                )}
              </div>
            </div>
          </section>

          <section className="card p-6">
            <h2 className="font-display text-xl text-royal">Payment</h2>
            <div className="mt-4 space-y-2">
              <label className="flex cursor-pointer items-center gap-3 rounded-xl border border-navy/10 p-3 has-[:checked]:border-royal">
                <input
                  type="radio"
                  name="pay"
                  checked={paymentMethod === PaymentMethod.CashOnDelivery}
                  onChange={() => setPaymentMethod(PaymentMethod.CashOnDelivery)}
                />
                Cash on delivery
              </label>
              <label className="flex cursor-pointer items-center gap-3 rounded-xl border border-navy/10 p-3 has-[:checked]:border-royal">
                <input
                  type="radio"
                  name="pay"
                  checked={paymentMethod === PaymentMethod.CreditCard}
                  onChange={() => setPaymentMethod(PaymentMethod.CreditCard)}
                />
                Credit card (Paymob test mode)
              </label>
            </div>
            {paymentMethod === PaymentMethod.CreditCard && (
              <p className="mt-3 text-sm text-navy/60">
                You will be redirected to Paymob. GlowCare only marks the order paid after a verified webhook.
              </p>
            )}
          </section>

          <section className="card p-6">
            <h2 className="font-display text-xl text-royal">Promo code</h2>
            <div className="mt-4 flex gap-3">
              <input
                className="field"
                value={promoCode}
                onChange={(e) => setPromoCode(e.target.value)}
                placeholder="Optional code"
                aria-label="Promo code"
              />
              <Button type="button" variant="ghost" onClick={() => void applyPromo()}>
                Apply
              </Button>
            </div>
            {promoNotice && <p className="mt-2 text-sm text-royal">{promoNotice}</p>}
          </section>
        </div>

        <aside className="h-fit card p-6">
          <h2 className="font-display text-xl text-royal">Order summary</h2>
          <ul className="mt-4 space-y-2 text-sm">
            {cart.items.map((item) => (
              <li key={item.id} className="flex justify-between gap-3">
                <span className="text-navy/70">{item.productName} × {item.quantity}</span>
                <span>{formatPrice(item.lineTotal)}</span>
              </li>
            ))}
          </ul>
          {summary ? (
            <dl className="mt-5 space-y-2 border-t border-navy/10 pt-4 text-sm">
              <div className="flex justify-between"><dt>Subtotal</dt><dd>{formatPrice(summary.subtotal)}</dd></div>
              <div className="flex justify-between"><dt>Shipping</dt><dd>{formatPrice(summary.shippingCost)}</dd></div>
              <div className="flex justify-between"><dt>Tax</dt><dd>{formatPrice(summary.taxAmount)}</dd></div>
              <div className="flex justify-between"><dt>Discount</dt><dd>-{formatPrice(summary.discountAmount)}</dd></div>
              <div className="flex justify-between font-display text-lg text-royal">
                <dt>Total</dt>
                <dd>{formatPrice(summary.totalAmount)}</dd>
              </div>
              <p className="text-xs text-navy/50">Free shipping from {formatPrice(summary.freeShippingThreshold)}.</p>
            </dl>
          ) : (
            <p className="mt-4 text-sm text-navy/60">Select or save an address to see server-calculated totals.</p>
          )}

          <Button type="submit" loading={submitting} disabled={busy || (addressId === '' && !isCompleteAddress(newAddress))} className="mt-6 w-full">
            {paymentMethod === PaymentMethod.CreditCard ? 'Pay with card' : 'Place COD order'}
          </Button>
          <Link to="/cart" className="mt-3 block text-center text-sm font-semibold text-royal hover:underline">
            Back to cart
          </Link>
        </aside>
      </form>
    </div>
  )
}
