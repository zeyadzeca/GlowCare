import { ArrowRight } from 'lucide-react'

/**
 * The rotating circular stamp from the reference design: a yellow disc with a
 * blue arrow, surrounded by circular text ("EXPLORE ALL PRODUCTS - ...").
 * Purely decorative - the link target is supplied by the caller.
 */
export function CircularBadge({
  text = 'EXPLORE ALL PRODUCTS',
  href = '/products',
  className = '',
}: {
  text?: string
  href?: string
  className?: string
}) {
  // Repeat the label so it comfortably wraps the whole circle.
  const ring = `${text} - ${text} - `

  return (
    <a
      href={href}
      aria-label={text}
      className={`group relative inline-flex h-36 w-36 items-center justify-center ${className}`}
    >
      {/* Rotating text ring */}
      <svg viewBox="0 0 100 100" className="absolute inset-0 h-full w-full animate-[spin_18s_linear_infinite]">
        <defs>
          <path
            id="badge-circle"
            d="M50,50 m-38,0 a38,38 0 1,1 76,0 a38,38 0 1,1 -76,0"
            fill="none"
          />
        </defs>
        <text
          fill="#3162AF"
          fontSize="8.5"
          fontWeight="700"
          letterSpacing="1.6"
          fontFamily="Oswald, sans-serif"
        >
          <textPath href="#badge-circle">{ring}</textPath>
        </text>
      </svg>

      {/* Yellow disc + blue arrow */}
      <span className="flex h-[68%] w-[68%] items-center justify-center rounded-full bg-yellow transition-transform duration-300 group-hover:scale-105">
        <svg viewBox="0 0 40 24" className="h-6 w-10 text-royal" fill="none" aria-hidden="true">
          <path
            d="M2 12c6-7 12 7 18 0s10-8 16-6"
            stroke="currentColor"
            strokeWidth="3"
            strokeLinecap="round"
          />
          <path
            d="M30 2l6 4-5 5"
            stroke="currentColor"
            strokeWidth="3"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
        <ArrowRight className="sr-only" />
      </span>
    </a>
  )
}
