import { useEffect, useState } from 'react'
import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'
import { Button } from '@/components/common/Button'
import { Input } from '@/components/common/FormField'
import { adminApi, catalogApi } from '@/services/api'
import { getErrorMessage } from '@/utils/errors'
import { ADMIN_NAV } from '@/config/adminNav'
import type { Brand } from '@/types/api.types'

export function AdminBrandsPage() {
  const [items, setItems] = useState<Brand[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [form, setForm] = useState({ name: '' })
  const [busyId, setBusyId] = useState<number | null>(null)
  const [searchTerm, setSearchTerm] = useState('')

  const load = () => {
    setLoading(true)
    setError(null)
    catalogApi.getBrands()
      .then(setItems)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    load()
  }, [])

  const filteredItems = items.filter((item) => {
    if (searchTerm && !item.name.toLowerCase().includes(searchTerm.toLowerCase())) return false
    return true
  })

  function openForm(item?: Brand) {
    if (item) {
      setEditingId(item.id)
      setForm({ name: item.name })
    } else {
      setEditingId(null)
      setForm({ name: '' })
    }
    setShowForm(true)
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setBusyId(editingId ?? -1)
    try {
      if (editingId) {
        await adminApi.updateBrand(editingId, form)
      } else {
        await adminApi.createBrand(form)
      }
      setShowForm(false)
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  async function handleDelete(id: number) {
    if (!confirm('Delete this brand?')) return
    setBusyId(id)
    setError(null)
    try {
      await adminApi.deleteBrand(id)
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  return (
    <DashboardShell
      title="Brands"
      subtitle="Manage product brands"
      nav={ADMIN_NAV}
    >
      {error && <p className="text-sm text-red-600">{error}</p>}
      {showForm ? (
        <div className="card p-6">
          <h3 className="font-display text-lg text-royal mb-4">{editingId ? 'Edit Brand' : 'Add Brand'}</h3>
          <form onSubmit={handleSubmit} className="space-y-4">
            <Input label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
            <div className="flex gap-2">
              <Button type="submit" loading={busyId === -1}>{editingId ? 'Update' : 'Create'}</Button>
              <Button type="button" variant="ghost" onClick={() => setShowForm(false)}>Cancel</Button>
            </div>
          </form>
        </div>
      ) : loading ? (
        <div className="card p-8 text-center"><Spinner label="Loading..." /></div>
      ) : (
        <>
          <div className="mb-4 flex gap-4 items-center">
            <Button onClick={() => openForm()}>Add Brand</Button>
            <input type="text" placeholder="Search brands..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} className="field flex-1" />
          </div>
          {filteredItems.length > 0 ? (
            <div className="card overflow-hidden">
            <table className="w-full">
              <thead className="bg-navy/5">
                <tr>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Name</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredItems.map((item) => (
                  <tr key={item.id} className="border-t border-navy/10">
                    <td className="px-4 py-3 text-sm">{item.name}</td>
                    <td className="px-4 py-3 flex gap-2">
                      <Button variant="ghost" className="text-sm" onClick={() => openForm(item)}>Edit</Button>
                      <Button variant="ghost" className="text-sm text-red-600" loading={busyId === item.id} onClick={() => void handleDelete(item.id)}>Delete</Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          ) : (
            <p className="text-navy/60">No brands found matching your search.</p>
          )}
        </>
      )}
    </DashboardShell>
  )
}
