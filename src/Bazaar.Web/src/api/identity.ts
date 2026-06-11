import axios from 'axios'
import type { TokenSet } from '@/auth/tokenStorage'

const IDENTITY_URL = import.meta.env.VITE_IDENTITY_URL || ''
const CLIENT_ID = import.meta.env.VITE_OAUTH_CLIENT_ID || 'bazaar_mobile'
const SCOPE = import.meta.env.VITE_OAUTH_SCOPE || 'openid profile phone bazaar_api offline_access'

const identityClient = axios.create({
  baseURL: IDENTITY_URL,
  timeout: 20000
})

export async function requestOtp(phoneNumber: string): Promise<void> {
  await identityClient.post(
    '/api/Otp/request',
    { phoneNumber },
    { headers: { 'Content-Type': 'application/json' } }
  )
}

export async function exchangeOtpForToken(phone: string, otpCode: string): Promise<TokenSet> {
  const body = new URLSearchParams()
  body.set('client_id', CLIENT_ID)
  body.set('grant_type', 'otp_verification')
  body.set('phone', phone)
  body.set('otp_code', otpCode)
  body.set('scope', SCOPE)
  const { data } = await identityClient.post('/connect/token', body, {
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
  })
  return normalizeTokens(data)
}

export async function refreshAccessToken(refreshToken: string): Promise<TokenSet | null> {
  try {
    const body = new URLSearchParams()
    body.set('client_id', CLIENT_ID)
    body.set('grant_type', 'refresh_token')
    body.set('refresh_token', refreshToken)
    body.set('scope', SCOPE)
    const { data } = await identityClient.post('/connect/token', body, {
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
    })
    return normalizeTokens(data)
  } catch {
    return null
  }
}

function normalizeTokens(raw: Record<string, any>): TokenSet {
  const expiresIn = Number(raw.expires_in ?? 0)
  return {
    access_token: raw.access_token,
    refresh_token: raw.refresh_token ?? null,
    token_type: raw.token_type ?? 'Bearer',
    scope: raw.scope ?? SCOPE,
    expires_at: Date.now() + expiresIn * 1000
  }
}
