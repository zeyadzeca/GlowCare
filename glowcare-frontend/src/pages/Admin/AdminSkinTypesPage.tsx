import { useEffect, useState } from 'react'
import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'
import { Button } from '@/components/common/Button'
import { Input } from '@/components/common/FormField'
import { adminApi, catalogApi } from '@/services/api'
import { getErrorMessage } from '@/utils/errors'
import { ADMIN_NAV } from '@/config/adminNav'
import type { SkinType } from '@/types/api.types'

export function AdminSkinTypesPage() {
  const [items, setItems] = useState<SkinType[]>([])
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
    catalogApi.getSkinTypes()
      .then((data) => {
        if (searchTerm) {
          const term = searchTerm.toLowerCase()
          setItems(data.filter(item => item.name.toLowerCase().includes(term)))
        } else {
          setItems(data)
        }
      })
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    load()
  }, [searchTerm])

  function openForm(item?: SkinType) {
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
        await adminApi.updateSkinType(editingId, form)
      } else {
        await adminApi.createSkinType(form)
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
    if (!confirm('Delete this skin type?')) return
    setBusyId(id)
    setError(null)
    try {
      await adminApi.deleteSkinType(id)
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  return (
    <DashboardShell
      title="Skin Types"
      subtitle="Manage skin type classifications"
      nav={ADMIN_NAV}
    >
      {error && <p className="text-sm text-red-600">{error}</p>}
      {showForm ? (
        <div className="card p-6">
          <h3 className="font-display text-lg text-royal mb-4">{editingId ? 'Edit Skin Type' : 'Add Skin Type'}</h3>
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
          <div className="mb-4 flex gap-2">
            <input type="text" placeholder="Search skin types..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} className="field flex-1" />
            <Button onClick={() => openForm()}>Add Skin Type</Button>
          </div>
          {items.length > 0 ? (
            <div className="card overflow-hidden">
            <table className="w-full">
              <thead className="bg-navy/5">
                <tr>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Name</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((item) => (
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
            <p className="text-navy/60">No skin types found matching your search.</p>
          )}
        </>
      )}
    </DashboardShell>
  )
}
