import { api, type SlidingCollectionWrapper, type SlidingParams } from '../http'

export interface ProductModel {
  productId: string
  name: string
  unit: string
  picture?: string | null
  sellPrice: number
  buyPrice: number
  inventoryAmount: number
  categoryId?: string | null
}

export interface ProductPropertyValueModel {
  propertyId: string
  propertyName: string
  value: string
}

export interface ProductDetailModel extends ProductModel {
  shopId: string
  createdOn: string
  properties: ProductPropertyValueModel[]
}

export interface CreateProductPayload {
  categoryId?: string | null
  name: string
  unit: string
  picture?: string | null
  sellPrice: number
  buyPrice: number
}

export interface CreateProductResult {
  productId: string
  name: string
}

export interface ListProductsParams extends SlidingParams {
  categoryId?: string | null
  search?: string
}

export async function listProducts(
  shopId: string,
  p: ListProductsParams = {}
): Promise<SlidingCollectionWrapper<ProductModel>> {
  const params: Record<string, any> = {
    'Pagination.Skip': p.skip ?? 0,
    'Pagination.Take': p.take ?? 30
  }
  if (p.categoryId) params['CategoryId'] = p.categoryId
  if (p.search) params['Search'] = p.search
  const { data } = await api.get(`/api/shops/${shopId}/Products`, { params })
  if (Array.isArray(data)) {
    return { items: data, totalCount: data.length, skip: p.skip ?? 0, take: p.take ?? 30 }
  }
  return {
    items: data.items ?? [],
    totalCount: data.totalCount,
    skip: data.skip ?? p.skip ?? 0,
    take: data.take ?? p.take ?? 30
  }
}

export async function getProduct(shopId: string, productId: string): Promise<ProductDetailModel> {
  const { data } = await api.get(`/api/shops/${shopId}/Products/${productId}`)
  return data
}

export async function createProduct(
  shopId: string,
  payload: CreateProductPayload
): Promise<CreateProductResult> {
  const { data } = await api.post(`/api/shops/${shopId}/Products`, payload)
  return data
}

export async function updateProduct(
  shopId: string,
  productId: string,
  payload: CreateProductPayload
): Promise<void> {
  await api.put(`/api/shops/${shopId}/Products/${productId}`, payload)
}

export async function deleteProduct(shopId: string, productId: string): Promise<void> {
  await api.delete(`/api/shops/${shopId}/Products/${productId}`)
}

export async function setProductPropertyValue(
  shopId: string,
  productId: string,
  productPropertyId: string,
  value: string
): Promise<void> {
  await api.post(`/api/shops/${shopId}/Products/${productId}/properties`, {
    productPropertyId,
    value
  })
}
