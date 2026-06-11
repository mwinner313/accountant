import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as repo from '@/db/repositories/productProperties'
import { flushOutbox } from '@/sync/syncEngine'
import type { CachedProductProperty } from '@/db/db'

export const useProductPropertyStore = defineStore('productProperties', () => {
  const items = ref<CachedProductProperty[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadLocal(shopId: string) {
    items.value = await repo.getAll(shopId)
  }

  async function refresh(shopId: string) {
    loading.value = true
    error.value = null
    try {
      await flushOutbox()
      await loadLocal(shopId)
    } catch (e: any) {
      error.value = e?.message ?? 'fetch-failed'
    } finally {
      loading.value = false
    }
  }

  async function create(shopId: string, name: string) {
    const created = await repo.createOptimistic(shopId, name)
    await loadLocal(shopId)
    void flushOutbox().then(() => loadLocal(shopId))
    return created
  }

  async function update(shopId: string, propertyId: string, name: string) {
    await repo.updateOptimistic(shopId, propertyId, name)
    await loadLocal(shopId)
    void flushOutbox().then(() => loadLocal(shopId))
  }

  async function remove(shopId: string, propertyId: string) {
    await repo.deleteOptimistic(shopId, propertyId)
    await loadLocal(shopId)
    void flushOutbox().then(() => loadLocal(shopId))
  }

  return { items, loading, error, loadLocal, refresh, create, update, remove }
})
