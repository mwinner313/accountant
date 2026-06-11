import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { authStorage, type TokenSet } from './tokenStorage'
import { decodeJwt } from './jwt'
import { exchangeOtpForToken, requestOtp as apiRequestOtp } from '@/api/identity'
import { disableDevBypass, enableDevBypass } from './devBypass'
import { DEV_USER_ID, DEV_USER_PHONE, seedDevData } from './devSeed'

export const useAuthStore = defineStore('auth', () => {
  const tokens = ref<TokenSet | null>(null)
  const phone = ref<string | null>(null)
  const ready = ref(false)

  const isAuthenticated = computed(() => {
    if (!tokens.value) return false
    return tokens.value.expires_at > Date.now() - 5_000
  })

  const userId = computed(() => {
    if (!tokens.value?.access_token) return null
    return decodeJwt(tokens.value.access_token)?.sub ?? null
  })

  async function hydrate() {
    const loaded = await authStorage.load()
    tokens.value = loaded
    if (loaded?.access_token) {
      const claims = decodeJwt(loaded.access_token)
      phone.value = (claims?.phone_number as string) ?? null
    }
    ready.value = true
  }

  async function requestOtp(phoneNumber: string) {
    phone.value = phoneNumber
    await apiRequestOtp(phoneNumber)
  }

  async function verifyOtp(otpCode: string) {
    if (!phone.value) throw new Error('phone-not-set')
    const next = await exchangeOtpForToken(phone.value, otpCode)
    await authStorage.save(next)
    tokens.value = next
    return next
  }

  async function logout() {
    await authStorage.clear()
    disableDevBypass()
    tokens.value = null
    phone.value = null
  }

  async function devLogin(): Promise<{ activeShopId: string }> {
    const fake = makeDevJwt(DEV_USER_ID, DEV_USER_PHONE)
    const next: TokenSet = {
      access_token: fake,
      refresh_token: null,
      token_type: 'Bearer',
      scope: 'dev-bypass',
      expires_at: Date.now() + 30 * 86_400_000
    }
    await authStorage.save(next)
    tokens.value = next
    phone.value = DEV_USER_PHONE
    enableDevBypass()
    return await seedDevData()
  }

  return {
    tokens,
    phone,
    ready,
    isAuthenticated,
    userId,
    hydrate,
    requestOtp,
    verifyOtp,
    logout,
    devLogin
  }
})

function makeDevJwt(sub: string, phone: string): string {
  const b64url = (obj: unknown) =>
    btoa(JSON.stringify(obj)).replace(/=+$/, '').replace(/\+/g, '-').replace(/\//g, '_')
  const header = b64url({ alg: 'none', typ: 'JWT' })
  const payload = b64url({
    sub,
    phone_number: phone,
    exp: Math.floor(Date.now() / 1000) + 60 * 60 * 24 * 30,
    iss: 'dev-bypass',
    aud: 'bazaar_api'
  })
  return `${header}.${payload}.`
}
