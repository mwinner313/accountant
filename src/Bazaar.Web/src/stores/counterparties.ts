import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as repo from '@/db/repositories/counterparties'
import * as api from '@/api/endpoints/counterparties'
import { flushOutbox } from '@/sync/syncEngine'
import type { CachedCounterparty } from '@/db/db'
import type { CounterpartyListModel, CreateCounterpartyPayload } from '@/api/endpoints/counterparties'
import { DevBypassError } from '@/api/http'

export const useCounterpartyStore = defineStore('counterparties', () => {
  const items = ref<CachedCounterparty[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadLocal() {
    items.value = await repo.getAll()
  }

  async function refresh(search?: string | null) {
    loading.value = true
    error.value = null
    try {
      await flushOutbox()
      try {
        await repo.pullRemote(search ?? null)
      } catch (e: any) {
        if (!(e instanceof DevBypassError)) error.value = e?.message ?? 'fetch-failed'
      }
      await loadLocal()
    } finally {
      loading.value = false
    }
  }

  /** Remote search for autocomplete (falls back to local cache). */
  async function searchRemote(query: string): Promise<CounterpartyListModel[]> {
    const q = query.trim()
    try {
      const res = await api.listCounterparties({ skip: 0, take: 30, search: q || undefined })
      return res.items
    } catch (e) {
      if (e instanceof DevBypassError) {
        const all = await repo.getAll()
        if (!q) return all.map(c => ({ counterpartyId: c.counterpartyId, fullName: c.fullName }))
        const low = q.toLowerCase()
        return all
          .filter(c => c.fullName.toLowerCase().includes(low))
          .map(c => ({ counterpartyId: c.counterpartyId, fullName: c.fullName }))
      }
      throw e
    }
  }

  async function create(payload: CreateCounterpartyPayload) {
    const row = await repo.createOptimistic(payload)
    await loadLocal()
    void flushOutbox().then(() => loadLocal())
    return row
  }

  async function update(counterpartyId: string, payload: CreateCounterpartyPayload) {
    await repo.updateOptimistic(counterpartyId, payload)
    await loadLocal()
    void flushOutbox().then(() => loadLocal())
  }

  async function remove(counterpartyId: string) {
    await repo.deleteOptimistic(counterpartyId)
    await loadLocal()
    void flushOutbox().then(() => loadLocal())
  }

  return { items, loading, error, loadLocal, refresh, searchRemote, create, update, remove }
})
