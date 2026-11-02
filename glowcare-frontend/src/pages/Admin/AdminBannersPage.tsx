import { useEffect, useState } from 'react'
import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'
import { Button } from '@/components/common/Button'
import { Input } from '@/components/common/FormField'
import { promoBannerApi } from '@/services/api'
import { getErrorMessage } from '@/utils/errors'
import type { Banner, BannerSaveRequest } from '@/types/api.types'

export function AdminBannersPage() {
  const [banners, setBanners] = useState<Banner[] | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [form, setForm] = useState<BannerSaveRequest>({
    title: '',
    imageUrl: '',
    link: '',
    isActive: true,
  })

  const load = () => {
    setLoading(true)
    setError(null)
    promoBannerApi.getAllBanners()
      .then(setBanners)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    load()
  }, [])

  async function handleDelete(id: number) {
    if (!confirm('Are you sure you want to delete this banner?')) return
    setBusyId(id)
    setError(null)
    try {
      await promoBannerApi.deleteBanner(id)
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  function openForm(banner?: Banner) {
    if (banner) {
      setEditingId(banner.id)
      setForm({
        title: banner.title,
        imageUrl: banner.imageUrl,
        link: banner.link ?? '',
        isActive: banner.isActive,
      })
    } else {
      setEditingId(null)
      setForm({
        title: '',
        imageUrl: '',
        link: '',
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
      if (editingId) {
        await promoBannerApi.updateBanner(editingId, form)
      } else {
        await promoBannerApi.createBanner(form)
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
      title="Banners"
      subtitle="Manage promotional banners"
      nav={[
        { to: '/admin', label: 'Overview' },
        { to: '/admin/users', label: 'Users' },
        { to: '/admin/sellers', label: 'Seller Approvals' },
        { to: '/admin/products', label: 'Products' },
        { to: '/admin/orders', label: 'Orders' },
        { to: '/admin/promos', label: 'Promo Codes' },
        { to: '/admin/categories', label: 'Categories', end: true },
      ]}
    >
      {error && <p className="text-sm text-red-600">{error}</p>}
      {showForm ? (
        <div className="card p-6">
          <h3 className="font-display text-lg text-royal mb-4">{editingId ? 'Edit Banner' : 'Add Banner'}</h3>
          <form onSubmit={handleSubmit} className="space-y-4">
            <Input label="Title" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} required />
            <Input label="Image URL" value={form.imageUrl} onChange={(e) => setForm({ ...form, imageUrl: e.target.value })} required placeholder="https://..." />
            <Input label="Link URL" value={form.link} onChange={(e) => setForm({ ...form, link: e.target.value })} placeholder="https://..." />
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
          <Spinner label="Loading banners..." />
        </div>
      ) : (
        <>
          <div className="mb-4">
            <Button onClick={() => openForm()}>Add Banner</Button>
          </div>
          {banners && banners.length > 0 ? (
            <div className="space-y-4">
              {banners.map((banner) => (
                <div key={banner.id} className="card p-6">
                  <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                    <div className="flex gap-4">
                      {banner.imageUrl && (
                        <img src={banner.imageUrl} alt="" className="h-24 w-32 rounded-lg object-cover" />
                      )}
                      <div>
                        <h3 className="font-display text-lg text-royal">{banner.title}</h3>
                        {banner.link && (
                          <p className="text-sm text-navy/70">Link: {banner.link}</p>
                        )}
                        <p className="mt-2 text-xs font-semibold">
                          Status: <span className={banner.isActive ? 'text-green-600' : 'text-red-600'}>{banner.isActive ? 'Active' : 'Inactive'}</span>
                        </p>
                      </div>
                    </div>
                    <div className="flex gap-2">
                      <Button variant="ghost" className="text-sm" onClick={() => openForm(banner)}>Edit</Button>
                      <Button
                        variant="ghost"
                        className="text-sm text-red-600"
                        loading={busyId === banner.id}
                        onClick={() => void handleDelete(banner.id)}
                      >
                        Delete
                      </Button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-navy/60">No banners found.</p>
          )}
        </>
      )}
    </DashboardShell>
  )
}
