import type { InputHTMLAttributes, SelectHTMLAttributes, TextareaHTMLAttributes } from 'react'

/**
 * Form field primitives. Each renders a `<label>` bound to its control so
 * forms stay accessible without extra wiring on every page.
 */

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string
  error?: string
}

export function Input({ label, error, id, className = '', ...rest }: InputProps) {
  const inputId = id ?? rest.name
  return (
    <div className="mb-4">
      <label htmlFor={inputId} className="field-label">
        {label}
      </label>
      <input id={inputId} className={`field ${className}`} aria-invalid={!!error} {...rest} />
      {error && <p className="mt-1 text-sm text-red-600">{error}</p>}
    </div>
  )
}

interface TextAreaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label: string
  error?: string
}

export function TextArea({ label, error, id, className = '', ...rest }: TextAreaProps) {
  const inputId = id ?? rest.name
  return (
    <div className="mb-4">
      <label htmlFor={inputId} className="field-label">
        {label}
      </label>
      <textarea id={inputId} className={`field ${className}`} aria-invalid={!!error} {...rest} />
      {error && <p className="mt-1 text-sm text-red-600">{error}</p>}
    </div>
  )
}

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string
  error?: string
  children: React.ReactNode
}

export function Select({ label, error, id, className = '', children, ...rest }: SelectProps) {
  const inputId = id ?? rest.name
  return (
    <div className="mb-4">
      <label htmlFor={inputId} className="field-label">
        {label}
      </label>
      <select id={inputId} className={`field ${className}`} {...rest}>
        {children}
      </select>
      {error && <p className="mt-1 text-sm text-red-600">{error}</p>}
    </div>
  )
}
