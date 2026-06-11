import axios, { AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios'
import { authStorage, type TokenSet } from '@/auth/tokenStorage'
import { isDevBypassEnabled } from '@/auth/devBypass'
import { refreshAccessToken } from './identity'

const API_URL = import.meta.env.VITE_API_URL || ''

export const api: AxiosInstance = axios.create({
  baseURL: API_URL,
  timeout: 20000,
  headers: { 'Content-Type': 'application/json' }
})

export class DevBypassError extends Error {
  constructor() {
    super('dev-bypass: api call short-circuited')
    this.name = 'DevBypassError'
  }
}

let refreshPromise: Promise<TokenSet | null> | null = null

api.interceptors.request.use(async (config: InternalAxiosRequestConfig) => {
  if (isDevBypassEnabled()) {
    // No backend in dev-bypass mode — fail fast, silently. The repository layer
    // catches the rejection and keeps showing the seeded Dexie data.
    throw new DevBypassError()
  }
  const tokens = await authStorage.load()
  if (tokens?.access_token) {
    config.headers = config.headers ?? {}
    ;(config.headers as Record<string, string>)['Authorization'] = `Bearer ${tokens.access_token}`
  }
  return config
})

api.interceptors.response.use(
  r => r,
  async (error: AxiosError) => {
    const original = error.config as InternalAxiosRequestConfig & { _retry?: boolean }
    if (
      error.response?.status === 401 &&
      original &&
      !original._retry &&
      !original.url?.includes('/connect/token')
    ) {
      original._retry = true
      try {
        if (!refreshPromise) {
          refreshPromise = (async () => {
            const current = await authStorage.load()
            if (!current?.refresh_token) return null
            const next = await refreshAccessToken(current.refresh_token)
            if (next) await authStorage.save(next)
            return next
          })().finally(() => {
            refreshPromise = null
          })
        }
        const next = await refreshPromise
        if (next?.access_token) {
          original.headers = original.headers ?? {}
          ;(original.headers as Record<string, string>)['Authorization'] =
            `Bearer ${next.access_token}`
          return api.request(original)
        }
      } catch {
        // fall through to reject
      }
      await authStorage.clear()
    }
    return Promise.reject(error)
  }
)

export interface SlidingCollectionWrapper<T> {
  items: T[]
  totalCount?: number
  skip: number
  take: number
}

export interface SlidingParams {
  skip?: number
  take?: number
}
