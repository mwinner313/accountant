import { db, type CachedProduct } from '../db'
import * as outbox from '../outbox'
import * as api from '@/api/endpoints/products'

function localId(): string {
  return 'local-' + (crypto.randomUUID ? crypto.randomUUID() : Date.now().toString(36))
}

export async function getAll(
  shopId: string,
  filter: { categoryId?: string | null; search?: string } = {}
): Promise<CachedProduct[]> {
  let coll = db.products.where('shopId').equals(shopId).filter(p => !p._pendingDelete)
  if (filter.categoryId) {
    const cat = filter.categoryId
    coll = coll.filter(p => p.categoryId === cat)
  }
  if (filter.search) {
    const q = filter.search.toLowerCase()
    coll = coll.filter(p => (p.name ?? '').toLowerCase().includes(q))
  }
  return await coll.toArray()
}

export async function getById(productId: string): Promise<CachedProduct | undefined> {
  return await db.products.get(productId)
}

export async function pullRemote(
  shopId: string,
  filter: { categoryId?: string | null; search?: string } = {}
): Promise<void> {
  const res = await api.listProducts(shopId, { skip: 0, take: 200, ...filter })
  const now = Date.now()
  await db.transaction('rw', db.products, async () => {
    const items = res.items.map(p => ({
      ...p,
      shopId,
      _local: false,
      _pendingDelete: false,
      _updatedAt: now
    })) as CachedProduct[]
    await db.products.bulkPut(items)
  })
}

export async function pullDetail(shopId: string, productId: string): Promise<void> {
  const detail = await api.getProduct(shopId, productId)
  await db.products.update(productId, { _detail: detail, _updatedAt: Date.now() })
}

export async function createOptimistic(
  shopId: string,
  payload: api.CreateProductPayload
): Promise<CachedProduct> {
  const id = localId()
  const row: CachedProduct = {
    productId: id,
    name: payload.name,
    unit: payload.unit,
    picture: payload.picture ?? null,
    sellPrice: payload.sellPrice,
    buyPrice: payload.buyPrice,
    inventoryAmount: 0,
    categoryId: payload.categoryId ?? null,
    shopId,
    _local: true,
    _updatedAt: Date.now()
  }
  await db.products.put(row)
  await outbox.enqueue('product', 'create', { shopId, payload }, { shopId, localId: id })
  return row
}

export async function updateOptimistic(
  shopId: string,
  productId: string,
  payload: api.CreateProductPayload
): Promise<void> {
  await db.products.update(productId, {
    name: payload.name,
    unit: payload.unit,
    picture: payload.picture ?? null,
    sellPrice: payload.sellPrice,
    buyPrice: payload.buyPrice,
    categoryId: payload.categoryId ?? null,
    _updatedAt: Date.now()
  })
  await outbox.enqueue('product', 'update', { shopId, productId, payload }, { shopId })
}

export async function deleteOptimistic(shopId: string, productId: string): Promise<void> {
  await db.products.update(productId, { _pendingDelete: true, _updatedAt: Date.now() })
  await outbox.enqueue('product', 'delete', { shopId, productId }, { shopId })
}

export async function setPropertyOptimistic(
  shopId: string,
  productId: string,
  productPropertyId: string,
  value: string
): Promise<void> {
  await outbox.enqueue(
    'productPropertyValue',
    'update',
    { shopId, productId, productPropertyId, value },
    { shopId }
  )
}

export async function applyServerCreated(
  shopId: string,
  localId: string,
  real: api.CreateProductResult,
  original: api.CreateProductPayload
): Promise<void> {
  await db.transaction('rw', db.products, async () => {
    await db.products.delete(localId)
    await db.products.put({
      productId: real.productId,
      name: real.name,
      unit: original.unit,
      picture: original.picture ?? null,
      sellPrice: original.sellPrice,
      buyPrice: original.buyPrice,
      inventoryAmount: 0,
      categoryId: original.categoryId ?? null,
      shopId,
      _local: false,
      _updatedAt: Date.now()
    })
  })
}

export async function applyServerDeleted(productId: string): Promise<void> {
  await db.products.delete(productId)
}
