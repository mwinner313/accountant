import { db, type CachedCategory } from '../db'
import * as outbox from '../outbox'
import * as api from '@/api/endpoints/categories'

function localId(): string {
  return 'local-' + (crypto.randomUUID ? crypto.randomUUID() : Date.now().toString(36))
}

export async function getAll(shopId: string): Promise<CachedCategory[]> {
  return await db.categories.where('shopId').equals(shopId).filter(c => !c._pendingDelete).toArray()
}

export async function pullRemote(shopId: string): Promise<void> {
  const items = await api.listCategories(shopId)
  const now = Date.now()
  await db.transaction('rw', db.categories, async () => {
    const localOnly = await db.categories
      .where('shopId')
      .equals(shopId)
      .filter(c => !!c._local)
      .toArray()
    await db.categories.where('shopId').equals(shopId).delete()
    if (localOnly.length) await db.categories.bulkPut(localOnly)
    await db.categories.bulkPut(
      items.map(c => ({ ...c, shopId, _local: false, _pendingDelete: false, _updatedAt: now }))
    )
  })
}

export async function createOptimistic(shopId: string, name: string): Promise<CachedCategory> {
  const id = localId()
  const row: CachedCategory = {
    categoryId: id,
    name,
    shopId,
    _local: true,
    _updatedAt: Date.now()
  }
  await db.categories.put(row)
  await outbox.enqueue('category', 'create', { shopId, name }, { shopId, localId: id })
  return row
}

export async function updateOptimistic(
  shopId: string,
  categoryId: string,
  name: string
): Promise<void> {
  await db.categories.update(categoryId, { name, _updatedAt: Date.now() })
  await outbox.enqueue('category', 'update', { shopId, categoryId, name }, { shopId })
}

export async function deleteOptimistic(shopId: string, categoryId: string): Promise<void> {
  await db.categories.update(categoryId, { _pendingDelete: true, _updatedAt: Date.now() })
  await outbox.enqueue('category', 'delete', { shopId, categoryId }, { shopId })
}

export async function applyServerCreated(
  shopId: string,
  localId: string,
  real: api.CreateCategoryResult
): Promise<void> {
  await db.transaction('rw', db.categories, async () => {
    await db.categories.delete(localId)
    await db.categories.put({
      categoryId: real.categoryId,
      name: real.name,
      shopId,
      _local: false,
      _updatedAt: Date.now()
    })
  })
}

export async function applyServerDeleted(categoryId: string): Promise<void> {
  await db.categories.delete(categoryId)
}
