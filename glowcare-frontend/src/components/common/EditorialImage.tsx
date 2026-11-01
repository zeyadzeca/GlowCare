import { useState } from 'react'
import { Sparkles } from 'lucide-react'

/**
 * Editorial imagery frame used in the hero and story sections.
 *
 * The reference design places large photography inside SKY-BLUE organic /
 * elliptical shapes. This component renders that frame and, when the image
 * source is missing or fails to load (seed data uses placeholder URLs), falls
 * back to a designed sky-blue composition instead of a broken-image icon.
 *
 * This is pure presentation - it never fabricates product data.
 */
export function EditorialImage({
  src,
  alt,
  shape = 'organic',
  className = '',
  ratio = 'aspect-[4/3]',
}: {
  src?: string | null
  alt: string
  /** `organic` = wobbly blob (hero), `ellipse` = tall oval (portrait). */
  shape?: 'organic' | 'ellipse'
  className?: string
  ratio?: string
}) {
  const [failed, setFailed] = useState(false)
  const showImage = !!src && !failed

  return (
    <div
      className={`relative ${ratio} overflow-hidden bg-sky ${
        shape === 'ellipse' ? 'shape-ellipse' : 'shape-organic'
      } ${className}`}
    >
      {showImage ? (
        <img
          src={src}
          alt={alt}
          loading="lazy"
          onError={() => setFailed(true)}
          className="h-full w-full object-cover"
        />
      ) : (
        // Designed fallback: soft sky gradient + product silhouette.
        <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-sky-soft via-sky to-sky-deep">
          <svg
            viewBox="0 0 120 160"
            className="h-3/5 w-auto text-white/85"
            fill="none"
            aria-hidden="true"
          >
            {/* Dropper serum bottle silhouette */}
            <rect x="46" y="8" width="28" height="26" rx="6" fill="currentColor" opacity="0.9" />
            <rect x="52" y="30" width="16" height="14" fill="currentColor" opacity="0.7" />
            <path
              d="M34 48c0-6 5-10 11-10h30c6 0 11 4 11 10v92c0 7-5 12-12 12H46c-7 0-12-5-12-12V48z"
              fill="currentColor"
              opacity="0.95"
            />
            <rect x="44" y="72" width="32" height="46" rx="6" fill="#93D1F5" opacity="0.55" />
          </svg>
          <Sparkles className="absolute bottom-6 right-6 h-6 w-6 text-white/80" aria-hidden="true" />
        </div>
      )}
    </div>
  )
}
