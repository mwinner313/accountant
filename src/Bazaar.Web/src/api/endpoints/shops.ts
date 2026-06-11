import { api, type SlidingCollectionWrapper, type SlidingParams } from '../http'

export interface ShopModel {
  shopId: string
  title: string
  createdOn: string
}

export interface ShopDetailModel {
  shopId: string
  title: string
  ownerId: string
  createdOn: string
}

export interface CreateShopResult {
  shopId: string
  title: string
}

export async function listShops(p: SlidingParams = {}): Promise<SlidingCollectionWrapper<ShopModel>> {
  const { data } = await api.get('/api/shops', {
    params: { 'Pagination.Skip': p.skip ?? 0, 'Pagination.Take': p.take ?? 50 }
  })
  return normalizeSliding<ShopModel>(data, p)
}

export async function getShop(shopId: string): Promise<ShopDetailModel> {
  const { data } = await api.get(`/api/shops/${shopId}`)
  return data
}

export async function createShop(title: string): Promise<CreateShopResult> {
  const { data } = await api.post('/api/shops', { title })
  return data
}

export async function updateShop(shopId: string, title: string): Promise<void> {
  await api.put(`/api/shops/${shopId}`, { title })
}

export async function deleteShop(shopId: string): Promise<void> {
  await api.delete(`/api/shops/${shopId}`)
}

function normalizeSliding<T>(raw: any, p: SlidingParams): SlidingCollectionWrapper<T> {
  if (Array.isArray(raw)) {
    return { items: raw, totalCount: raw.length, skip: p.skip ?? 0, take: p.take ?? 50 }
  }
  return {
    items: raw.items ?? raw.Items ?? [],
    totalCount: raw.totalCount ?? raw.TotalCount,
    skip: raw.skip ?? p.skip ?? 0,
    take: raw.take ?? p.take ?? 50
  }
}
