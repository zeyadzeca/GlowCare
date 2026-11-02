import { useEffect, useState } from 'react'
import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'
import { Button } from '@/components/common/Button'
import { Input } from '@/components/common/FormField'
import { adminApi } from '@/services/api'
import { getErrorMessage } from '@/utils/errors'
import { ADMIN_NAV } from '@/config/adminNav'
import type { UserManagement } from '@/types/api.types'

interface UserForm {
  fullName: string
  email: string
  isLockedOut: boolean
}

export function AdminUsersPage() {
  const [users, setUsers] = useState<UserManagement[] | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [form, setForm] = useState<UserForm>({
    fullName: '',
    email: '',
    isLockedOut: false,
  })

  useEffect(() => {
    let cancelled = false
    adminApi.getAdminUsers(searchTerm || undefined)
      .then((data) => {
        if (!cancelled) setUsers(data)
      })
      .catch(() => {
        if (!cancelled) setUsers(null)
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [searchTerm])

  function openForm(user?: UserManagement) {
    if (user) {
      setEditingId(user.id as any)
      setForm({
        fullName: user.fullName,
        email: user.email,
        isLockedOut: user.isLockedOut,
      })
    } else {
      setEditingId(null)
      setForm({
        fullName: '',
        email: '',
        isLockedOut: false,
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
        await adminApi.updateAdminUser(editingId, form)
      }
      setShowForm(false)
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  async function load() {
    setLoading(true)
    setError(null)
    adminApi.getAdminUsers(searchTerm || undefined)
      .then(setUsers)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }

  return (
    <DashboardShell
      title="Users"
      subtitle="Manage user accounts"
      nav={ADMIN_NAV}
    >
      {error && <p className="text-sm text-red-600">{error}</p>}
      {showForm ? (
        <div className="card p-6">
          <h3 className="font-display text-lg text-royal mb-4">Edit User</h3>
          <form onSubmit={handleSubmit} className="space-y-4">
            <Input label="Full Name" value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} required />
            <Input label="Email" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
            <label className="flex items-center gap-2">
              <input type="checkbox" checked={form.isLockedOut} onChange={(e) => setForm({ ...form, isLockedOut: e.target.checked })} />
              <span className="text-sm">Lock Account</span>
            </label>
            <div className="flex gap-2">
              <Button type="submit" loading={busyId === -1}>Update</Button>
              <Button type="button" variant="ghost" onClick={() => setShowForm(false)}>Cancel</Button>
            </div>
          </form>
        </div>
      ) : loading ? (
        <div className="card p-8 text-center">
          <Spinner label="Loading users..." />
        </div>
      ) : users ? (
        <>
          <div className="mb-4">
            <input type="text" placeholder="Search by name or email..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} className="field w-full" />
            {searchTerm && (
              <button type="button" onClick={() => setSearchTerm('')} className="text-sm text-royal hover:underline mt-1">Clear search</button>
            )}
          </div>
          {users && users.length > 0 ? (
            <div className="card overflow-hidden">
              <table className="w-full">
            <thead className="bg-navy/5">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Name</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Email</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Roles</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Status</th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id} className="border-t border-navy/10">
                  <td className="px-4 py-3 text-sm text-navy">{user.fullName}</td>
                  <td className="px-4 py-3 text-sm text-navy">{user.email}</td>
                  <td className="px-4 py-3 text-sm text-navy">{user.roles.join(', ')}</td>
                  <td className="px-4 py-3 text-sm">
                    {user.isLockedOut ? (
                      <span className="text-red-600">Locked</span>
                    ) : (
                      <span className="text-green-600">Active</span>
                    )}
                  </td>
                  <td className="px-4 py-3">
                    <Button variant="ghost" className="text-sm" onClick={() => openForm(user)}>Edit</Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
          ) : (
            <p className="text-navy/60">No users found matching your search.</p>
          )}
        </>
      ) : (
        <p className="text-navy/60">Failed to load users.</p>
      )}
    </DashboardShell>
  )
}
