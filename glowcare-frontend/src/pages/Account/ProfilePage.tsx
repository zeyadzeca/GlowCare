import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { userApi } from '@/services/api'
import { useAuth } from '@/context/AuthContext'
import type { Address, SellerProfile } from '@/types/api.types'
import { getErrorMessage } from '@/utils/errors'
import { Button } from '@/components/common/Button'
import { Input, TextArea } from '@/components/common/FormField'
import { ErrorState, Spinner } from '@/components/common/States'
import { ApprovalBadge } from '@/components/common/StatusBadge'

export function ProfilePage() {
  const { user, refreshProfile, hasRole } = useAuth()
  const [fullName, setFullName] = useState(user?.fullName ?? '')
  const [phoneNumber, setPhoneNumber] = useState(user?.phoneNumber ?? '')
  const [addresses, setAddresses] = useState<Address[]>([])
  const [seller, setSeller] = useState<SellerProfile | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  const [addressForm, setAddressForm] = useState({ city: '', street: '', buildingNumber: '' })
  const [editingId, setEditingId] = useState<number | null>(null)
  const [sellerForm, setSellerForm] = useState({ storeName: '', description: '', logo: '' })
  const [busy, setBusy] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const [profile, list, sellerProfile] = await Promise.all([
        userApi.getProfile(),
        userApi.getAddresses(),
        userApi.getSellerProfile(),
      ])
      setFullName(profile.fullName)
      setPhoneNumber(profile.phoneNumber ?? '')
      setAddresses(list)
      setSeller(sellerProfile)
      await refreshProfile()
    } catch (err) {
      setError(getErrorMessage(err, 'Could not load your account.'))
    } finally {
      setLoading(false)
    }
  }, [refreshProfile])

  useEffect(() => {
    void load()
  }, [load])

  async function saveProfile(event: FormEvent) {
    event.preventDefault()
    setBusy(true)
    setNotice(null)
    try {
      await userApi.updateProfile({ fullName, phoneNumber: phoneNumber || null })
      await refreshProfile()
      setNotice('Profile updated.')
    } catch (err) {
      setNotice(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  async function saveAddress(event: FormEvent) {
    event.preventDefault()
    setBusy(true)
    setNotice(null)
    try {
      if (editingId) {
        await userApi.updateAddress(editingId, addressForm)
      } else {
        await userApi.addAddress(addressForm)
      }
      setAddressForm({ city: '', street: '', buildingNumber: '' })
      setEditingId(null)
      setAddresses(await userApi.getAddresses())
    } catch (err) {
      setNotice(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  async function removeAddress(id: number) {
    setBusy(true)
    try {
      await userApi.deleteAddress(id)
      setAddresses(await userApi.getAddresses())
    } catch (err) {
      setNotice(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  async function applySeller(event: FormEvent) {
    event.preventDefault()
    setBusy(true)
    setNotice(null)
    try {
      setSeller(await userApi.applyForSeller(sellerForm))
      setNotice('Application submitted. An admin will review it.')
    } catch (err) {
      setNotice(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  if (loading) return <div className="shell"><Spinner label="Loading account..." /></div>
  if (error && addresses.length === 0 && !user) {
    return (
      <div className="shell py-12">
        <ErrorState message={error} onRetry={() => void load()} />
      </div>
    )
  }

  return (
    <div className="shell py-10">
      <h1 className="heading-display uppercase">My account</h1>
      <p className="mt-2 text-navy/60">{user?.email}</p>
      {notice && <p className="mt-4 text-sm text-royal">{notice}</p>}

      <div className="mt-8 grid gap-6 lg:grid-cols-2">
        <section className="card p-6">
          <h2 className="font-display text-xl text-royal">Profile</h2>
          <form onSubmit={saveProfile} className="mt-4">
            <Input label="Full name" required value={fullName} onChange={(e) => setFullName(e.target.value)} />
            <Input label="Phone" value={phoneNumber} onChange={(e) => setPhoneNumber(e.target.value)} />
            <Button type="submit" loading={busy}>Save profile</Button>
          </form>
        </section>

        <section className="card p-6">
          <h2 className="font-display text-xl text-royal">Addresses</h2>
          <ul className="mt-4 space-y-3">
            {addresses.map((address) => (
              <li key={address.id} className="flex items-start justify-between gap-3 rounded-xl bg-off-white p-3">
                <p className="text-sm">
                  {address.street}, {address.buildingNumber}
                  <span className="block text-navy/60">{address.city}</span>
                </p>
                <div className="flex gap-2">
                  <button
                    type="button"
                    className="text-sm font-semibold text-royal"
                    onClick={() => {
                      setEditingId(address.id)
                      setAddressForm({
                        city: address.city,
                        street: address.street,
                        buildingNumber: address.buildingNumber,
                      })
                    }}
                  >
                    Edit
                  </button>
                  <button type="button" className="text-sm text-navy/60" onClick={() => void removeAddress(address.id)}>
                    Delete
                  </button>
                </div>
              </li>
            ))}
          </ul>
          <form onSubmit={saveAddress} className="mt-4">
            <p className="text-sm font-semibold text-royal">{editingId ? 'Edit address' : 'Add address'}</p>
            <Input label="City" required value={addressForm.city} onChange={(e) => setAddressForm({ ...addressForm, city: e.target.value })} />
            <Input label="Street" required value={addressForm.street} onChange={(e) => setAddressForm({ ...addressForm, street: e.target.value })} />
            <Input label="Building" required value={addressForm.buildingNumber} onChange={(e) => setAddressForm({ ...addressForm, buildingNumber: e.target.value })} />
            <Button type="submit" loading={busy}>{editingId ? 'Update address' : 'Add address'}</Button>
          </form>
        </section>
      </div>

      <section className="card mt-6 p-6">
        <h2 className="font-display text-xl text-royal">Sell on GlowCare</h2>
        {seller ? (
          <div className="mt-4">
            <p className="font-semibold">{seller.storeName}</p>
            <p className="mt-1 text-navy/70">{seller.description}</p>
            <div className="mt-3"><ApprovalBadge status={seller.approvalStatus} /></div>
            {hasRole('Seller') && (
              <Link to="/seller" className="btn-primary mt-4 inline-flex">Open seller dashboard</Link>
            )}
          </div>
        ) : (
          <form onSubmit={applySeller} className="mt-4 max-w-xl">
            <p className="mb-4 text-sm text-navy/70">
              Submit a store profile. An admin must approve it before you can publish products.
            </p>
            <Input label="Store name" required value={sellerForm.storeName} onChange={(e) => setSellerForm({ ...sellerForm, storeName: e.target.value })} />
            <TextArea label="Description" required rows={4} value={sellerForm.description} onChange={(e) => setSellerForm({ ...sellerForm, description: e.target.value })} />
            <Input label="Logo URL (optional)" value={sellerForm.logo} onChange={(e) => setSellerForm({ ...sellerForm, logo: e.target.value })} />
            <Button type="submit" loading={busy}>Apply to become a seller</Button>
          </form>
        )}
      </section>
    </div>
  )
}
