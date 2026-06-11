import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as repo from '@/db/repositories/categories'
import { flushOutbox } from '@/sync/syncEngine'
import type { CachedCategory } from '@/db/db'
import { DevBypassError } from '@/api/http'

export const useCategoryStore = defineStore('categories', () => {
  const items = ref<CachedCategory[]>([])
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
      try {
        await repo.pullRemote(shopId)
      } catch (e: any) {
        if (!(e instanceof DevBypassError)) error.value = e?.message ?? 'fetch-failed'
      }
      await loadLocal(shopId)
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

  async function update(shopId: string, categoryId: string, name: string) {
    await repo.updateOptimistic(shopId, categoryId, name)
    await loadLocal(shopId)
    void flushOutbox().then(() => loadLocal(shopId))
  }

  async function remove(shopId: string, categoryId: string) {
    await repo.deleteOptimistic(shopId, categoryId)
    await loadLocal(shopId)
    void flushOutbox().then(() => loadLocal(shopId))
  }

  return { items, loading, error, loadLocal, refresh, create, update, remove }
})
