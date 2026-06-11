import { Capacitor } from '@capacitor/core'
import { Preferences } from '@capacitor/preferences'

const KEY = 'bazaar.auth.v1'

export interface TokenSet {
  access_token: string
  refresh_token: string | null
  token_type: string
  scope: string
  /** epoch ms when the access token expires */
  expires_at: number
}

const isNative = (): boolean => {
  try {
    return Capacitor.isNativePlatform?.() ?? false
  } catch {
    return false
  }
}

export const authStorage = {
  async load(): Promise<TokenSet | null> {
    try {
      if (isNative()) {
        const { value } = await Preferences.get({ key: KEY })
        return value ? (JSON.parse(value) as TokenSet) : null
      }
      const raw = localStorage.getItem(KEY)
      return raw ? (JSON.parse(raw) as TokenSet) : null
    } catch {
      return null
    }
  },
  async save(tokens: TokenSet): Promise<void> {
    const value = JSON.stringify(tokens)
    if (isNative()) {
      await Preferences.set({ key: KEY, value })
    } else {
      localStorage.setItem(KEY, value)
    }
  },
  async clear(): Promise<void> {
    if (isNative()) {
      await Preferences.remove({ key: KEY })
    } else {
      localStorage.removeItem(KEY)
    }
  }
}
