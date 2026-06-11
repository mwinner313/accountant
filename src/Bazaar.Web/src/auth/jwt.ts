export interface JwtPayload {
  sub?: string
  phone_number?: string
  exp?: number
  iss?: string
  aud?: string | string[]
  [key: string]: unknown
}

export function decodeJwt(token: string): JwtPayload | null {
  if (!token) return null
  const parts = token.split('.')
  if (parts.length < 2) return null
  try {
    const payload = parts[1]
    const padded = payload + '='.repeat((4 - (payload.length % 4)) % 4)
    const b64 = padded.replace(/-/g, '+').replace(/_/g, '/')
    const json = atob(b64)
    const decoded = decodeURIComponent(
      Array.prototype.map.call(json, (c: string) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join('')
    )
    return JSON.parse(decoded) as JwtPayload
  } catch {
    return null
  }
}
