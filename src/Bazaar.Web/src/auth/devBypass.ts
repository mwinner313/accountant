/**
 * Dev-only "skip OTP" mode.
 *
 * Two gates must be open for bypass to be active:
 *  1. Build-time env flag `VITE_DEV_BYPASS_AUTH=true` (so it can never be on in production).
 *  2. Runtime opt-in via the button on the login screen, persisted in `localStorage`.
 *
 * While active, `/api/*` and `/connect/*` requests are rejected client-side without
 * hitting the network, and the sync engine short-circuits. All data lives in Dexie.
 */

const FLAG_KEY = 'bazaar.devBypass.v1'

export function isDevBypassEnabled(): boolean {
  if (typeof window === 'undefined') return false
  if (import.meta.env.VITE_DEV_BYPASS_AUTH !== 'true') return false
  try {
    return localStorage.getItem(FLAG_KEY) === '1'
  } catch {
    return false
  }
}

export function isDevBypassAvailable(): boolean {
  return import.meta.env.VITE_DEV_BYPASS_AUTH === 'true'
}

export function enableDevBypass(): void {
  try {
    localStorage.setItem(FLAG_KEY, '1')
  } catch {
    // ignore
  }
}

export function disableDevBypass(): void {
  try {
    localStorage.removeItem(FLAG_KEY)
  } catch {
    // ignore
  }
}
