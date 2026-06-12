import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { authStorage, type TokenSet } from './tokenStorage'
import { decodeJwt } from './jwt'
import { exchangeOtpForToken, requestOtp as apiRequestOtp } from '@/api/identity'

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
    tokens.value = null
    phone.value = null
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
    logout
  }
})
