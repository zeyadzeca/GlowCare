import { useEffect, useState } from 'react'
import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'
import { Button } from '@/components/common/Button'
import { Input } from '@/components/common/FormField'
import { promoBannerApi } from '@/services/api'
import { getErrorMessage } from '@/utils/errors'
import { formatPrice } from '@/utils/format'
import { ADMIN_NAV } from '@/config/adminNav'
import type { PromoCode } from '@/types/api.types'

interface PromoForm {
  code: string
  discountType: number
  discountValue: number
  minimumOrderAmount: number
  usageLimit: number
  expiryDate: string
  isActive: boolean
}

export function AdminPromosPage() {
  const [promos, setPromos] = useState<PromoCode[] | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [form, setForm] = useState<PromoForm>({
    code: '',
    discountType: 0,
    discountValue: 0,
    minimumOrderAmount: 0,
    usageLimit: 0,
    expiryDate: '',
    isActive: true,
  })

  const load = () => {
    setLoading(true)
    setError(null)
    promoBannerApi.getPromoCodes()
      .then(setPromos)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    load()
  }, [])

  async function handleDelete(id: number) {
    if (!confirm('Are you sure you want to deactivate this promo code?')) return
    setBusyId(id)
    setError(null)
    try {
      await promoBannerApi.deactivatePromoCode(id)
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  function openForm(promo?: PromoCode) {
    if (promo) {
      setEditingId(promo.id)
      setForm({
        code: promo.code,
        discountType: promo.discountType,
        discountValue: promo.discountValue,
        minimumOrderAmount: promo.minimumOrderAmount,
        usageLimit: promo.usageLimit,
        expiryDate: promo.expiryDate.split('T')[0],
        isActive: promo.isActive,
      })
    } else {
      setEditingId(null)
      setForm({
        code: '',
        discountType: 0,
        discountValue: 0,
        minimumOrderAmount: 0,
        usageLimit: 0,
        expiryDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
        isActive: true,
      })
    }
    setShowForm(true)
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setBusyId(editingId ?? -1)
    try {
      const payload = { ...form, discountType: form.discountType as 0 | 1 }
      if (editingId) {
        await promoBannerApi.updatePromoCode(editingId, payload)
      } else {
        await promoBannerApi.createPromoCode(payload)
      }
      setShowForm(false)
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  return (
    <DashboardShell
      title="Promo Codes"
      subtitle="Manage discount codes"
      nav={ADMIN_NAV}
    >
      {error && <p className="text-sm text-red-600">{error}</p>}
      {showForm ? (
        <div className="card p-6">
          <h3 className="font-display text-lg text-royal mb-4">{editingId ? 'Edit Promo Code' : 'Add Promo Code'}</h3>
          <form onSubmit={handleSubmit} className="space-y-4">
            <Input label="Code" value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value.toUpperCase() })} required placeholder="SAVE20" />
            <div className="grid gap-4 sm:grid-cols-2">
              <select className="field" value={form.discountType} onChange={(e) => setForm({ ...form, discountType: Number(e.target.value) })} required>
                <option value={0}>Percentage</option>
                <option value={1}>Fixed Amount</option>
              </select>
              <Input label="Discount Value" type="number" value={form.discountValue} onChange={(e) => setForm({ ...form, discountValue: Number(e.target.value) })} required />
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <Input label="Minimum Order Amount" type="number" value={form.minimumOrderAmount} onChange={(e) => setForm({ ...form, minimumOrderAmount: Number(e.target.value) })} />
              <Input label="Usage Limit" type="number" value={form.usageLimit} onChange={(e) => setForm({ ...form, usageLimit: Number(e.target.value) })} />
            </div>
            <Input label="Expiry Date" type="date" value={form.expiryDate} onChange={(e) => setForm({ ...form, expiryDate: e.target.value })} required />
            <label className="flex items-center gap-2">
              <input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />
              <span className="text-sm">Active</span>
            </label>
            <div className="flex gap-2">
              <Button type="submit" loading={busyId === -1}>{editingId ? 'Update' : 'Create'}</Button>
              <Button type="button" variant="ghost" onClick={() => setShowForm(false)}>Cancel</Button>
            </div>
          </form>
        </div>
      ) : loading ? (
        <div className="card p-8 text-center">
          <Spinner label="Loading promo codes..." />
        </div>
      ) : (
        <>
          <div className="mb-4">
            <Button onClick={() => openForm()}>Add Promo Code</Button>
          </div>
          {promos && promos.length > 0 ? (
            <div className="card overflow-hidden">
              <table className="w-full">
                <thead className="bg-navy/5">
                  <tr>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Code</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Discount</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Min Order</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Usage</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Status</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {promos.map((promo) => (
                    <tr key={promo.id} className="border-t border-navy/10">
                      <td className="px-4 py-3 text-sm font-mono text-royal">{promo.code}</td>
                      <td className="px-4 py-3 text-sm text-navy">
                        {promo.discountType === 0 ? `${promo.discountValue}%` : formatPrice(promo.discountValue)}
                      </td>
                      <td className="px-4 py-3 text-sm text-navy">{formatPrice(promo.minimumOrderAmount)}</td>
                      <td className="px-4 py-3 text-sm text-navy">{promo.timesUsed} / {promo.usageLimit}</td>
                      <td className="px-4 py-3 text-sm">
                        {promo.isActive ? (
                          <span className="text-green-600">Active</span>
                        ) : (
                          <span className="text-red-600">Inactive</span>
                        )}
                      </td>
                      <td className="px-4 py-3 flex gap-2">
                        <Button variant="ghost" className="text-sm" onClick={() => openForm(promo)}>Edit</Button>
                        <Button
                          variant="ghost"
                          className="text-sm text-red-600"
                          loading={busyId === promo.id}
                          onClick={() => void handleDelete(promo.id)}
                        >
                          Deactivate
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-navy/60">No promo codes found.</p>
          )}
        </>
      )}
    </DashboardShell>
  )
}
