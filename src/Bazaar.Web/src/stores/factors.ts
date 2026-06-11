import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as repo from '@/db/repositories/factors'
import { flushOutbox } from '@/sync/syncEngine'
import type { CachedFactor } from '@/db/db'
import type {
  CreateFactorPayload,
  EditFactorPayload,
  FactorDetailModel,
  FactorTypeValue
} from '@/api/endpoints/factors'
import { DevBypassError } from '@/api/http'

export const useFactorStore = defineStore('factors', () => {
  const items = ref<CachedFactor[]>([])
  const detail = ref<FactorDetailModel | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadLocal(shopId: string, type?: FactorTypeValue | null) {
    items.value = await repo.getAll(shopId, type ?? null)
  }

  async function refresh(shopId: string, type?: FactorTypeValue | null) {
    loading.value = true
    error.value = null
    try {
      await flushOutbox()
      try {
        await repo.pullRemote(shopId, type ?? null)
      } catch (e: any) {
        if (!(e instanceof DevBypassError)) error.value = e?.message ?? 'fetch-failed'
      }
      await loadLocal(shopId, type ?? null)
    } finally {
      loading.value = false
    }
  }

  async function loadDetail(shopId: string, factorId: string) {
    try {
      await repo.pullDetail(shopId, factorId)
      const f = items.value.find(i => i.factorId === factorId)
      detail.value = f?._detail ?? null
    } catch {
      // offline-ok
    }
  }

  async function create(shopId: string, payload: CreateFactorPayload) {
    const created = await repo.createOptimistic(shopId, payload)
    await loadLocal(shopId)
    void flushOutbox().then(() => loadLocal(shopId))
    return created
  }

  async function edit(shopId: string, factorId: string, payload: EditFactorPayload) {
    await repo.editOptimistic(shopId, factorId, payload)
    await loadLocal(shopId)
    void flushOutbox().then(() => loadLocal(shopId))
  }

  return { items, detail, loading, error, loadLocal, refresh, loadDetail, create, edit }
})
