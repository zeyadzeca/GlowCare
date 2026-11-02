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
  const [phoneNumbers, setPhoneNumbers] = useState<{ id: number; phoneNumber: string; createdAt: string }[]>([])
  const [seller, setSeller] = useState<SellerProfile | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  const [addressForm, setAddressForm] = useState({ city: '', street: '', buildingNumber: '' })
  const [editingId, setEditingId] = useState<number | null>(null)
  const [phoneForm, setPhoneForm] = useState({ phoneNumber: '' })
  const [editingPhoneId, setEditingPhoneId] = useState<number | null>(null)
  const [sellerForm, setSellerForm] = useState({ storeName: '', description: '', logo: '' })
  const [busy, setBusy] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const [profile, list, phones, sellerProfile] = await Promise.all([
        userApi.getProfile(),
        userApi.getAddresses(),
        userApi.getPhoneNumbers(),
        userApi.getSellerProfile(),
      ])
      setFullName(profile.fullName)
      setPhoneNumber(profile.phoneNumber ?? '')
      setAddresses(list)
      setPhoneNumbers(phones)
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

  async function savePhoneNumber(event: FormEvent) {
    event.preventDefault()
    setBusy(true)
    setNotice(null)
    try {
      if (editingPhoneId) {
        await userApi.updatePhoneNumber(editingPhoneId, phoneForm)
      } else {
        await userApi.addPhoneNumber(phoneForm)
      }
      setPhoneForm({ phoneNumber: '' })
      setEditingPhoneId(null)
      setPhoneNumbers(await userApi.getPhoneNumbers())
    } catch (err) {
      setNotice(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  async function removePhoneNumber(id: number) {
    setBusy(true)
    try {
      await userApi.deletePhoneNumber(id)
      setPhoneNumbers(await userApi.getPhoneNumbers())
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

  async function updateSellerProfile(event: FormEvent) {
    event.preventDefault()
    setBusy(true)
    setNotice(null)
    try {
      if (seller) {
        setSeller(await userApi.updateSellerProfile(sellerForm))
        setNotice('Store profile updated.')
      }
    } catch (err) {
      setNotice(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  async function changePassword(event: FormEvent) {
    event.preventDefault()
    setBusy(true)
    setNotice(null)
    try {
      const form = event.target as HTMLFormElement
      const currentPassword = (form.elements.namedItem('currentPassword') as HTMLInputElement).value
      const newPassword = (form.elements.namedItem('newPassword') as HTMLInputElement).value
      await userApi.changePassword({ currentPassword, newPassword })
      setNotice('Password changed successfully.')
      form.reset()
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

        {!hasRole('Admin') && (
          <section className="card p-6">
            <h2 className="font-display text-xl text-royal">{hasRole('Seller') ? 'Locations' : 'Addresses'}</h2>
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
        )}

        <section className="card p-6">
          <h2 className="font-display text-xl text-royal">Phone Numbers</h2>
          <ul className="mt-4 space-y-3">
            {phoneNumbers.map((phone) => (
              <li key={phone.id} className="flex items-start justify-between gap-3 rounded-xl bg-off-white p-3">
                <p className="text-sm">{phone.phoneNumber}</p>
                <div className="flex gap-2">
                  <button
                    type="button"
                    className="text-sm font-semibold text-royal"
                    onClick={() => {
                      setEditingPhoneId(phone.id)
                      setPhoneForm({ phoneNumber: phone.phoneNumber })
                    }}
                  >
                    Edit
                  </button>
                  <button type="button" className="text-sm text-navy/60" onClick={() => void removePhoneNumber(phone.id)}>
                    Delete
                  </button>
                </div>
              </li>
            ))}
          </ul>
          <form onSubmit={savePhoneNumber} className="mt-4">
            <p className="text-sm font-semibold text-royal">{editingPhoneId ? 'Edit phone number' : 'Add phone number'}</p>
            <Input label="Phone Number" required value={phoneForm.phoneNumber} onChange={(e) => setPhoneForm({ phoneNumber: e.target.value })} />
            <Button type="submit" loading={busy}>{editingPhoneId ? 'Update phone number' : 'Add phone number'}</Button>
          </form>
        </section>
      </div>

      {!hasRole('Admin') && (
        <section className="card mt-6 p-6">
          <h2 className="font-display text-xl text-royal">{hasRole('Seller') ? 'GlowCare Seller' : 'Sell on GlowCare'}</h2>
          {seller ? (
            <div className="mt-4">
              <form onSubmit={updateSellerProfile} className="space-y-4 max-w-xl">
                <Input label="Store name" required value={sellerForm.storeName || seller.storeName} onChange={(e) => setSellerForm({ ...sellerForm, storeName: e.target.value })} />
                <TextArea label="Description" required rows={4} value={sellerForm.description || seller.description} onChange={(e) => setSellerForm({ ...sellerForm, description: e.target.value })} />
                <Input label="Logo URL" value={sellerForm.logo || seller.logo} onChange={(e) => setSellerForm({ ...sellerForm, logo: e.target.value })} />
                <div className="flex items-center gap-4">
                  <Button type="submit" loading={busy}>Update store profile</Button>
                  <ApprovalBadge status={seller.approvalStatus} />
                </div>
              </form>
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
      )}

      {!hasRole('Admin') && (
        <section className="card mt-6 p-6">
          <h2 className="font-display text-xl text-royal">Security</h2>
          <form onSubmit={changePassword} className="mt-4 max-w-xl space-y-4">
            <Input label="Current password" type="password" name="currentPassword" required />
            <Input label="New password" type="password" name="newPassword" required minLength={6} />
            <Button type="submit" loading={busy}>Change password</Button>
          </form>
        </section>
      )}
    </div>
  )
}
