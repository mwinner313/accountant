import { api } from '../http'

export interface ProductPropertyModel {
  productPropertyId: string
  name: string
}

export interface CreateProductPropertyResult {
  productPropertyId: string
  name: string
}

export async function createProductProperty(
  shopId: string,
  name: string
): Promise<CreateProductPropertyResult> {
  const { data } = await api.post(`/api/shops/${shopId}/product-properties`, { name })
  return data
}

export async function updateProductProperty(
  shopId: string,
  propertyId: string,
  name: string
): Promise<void> {
  await api.put(`/api/shops/${shopId}/product-properties/${propertyId}`, { name })
}

export async function deleteProductProperty(
  shopId: string,
  propertyId: string
): Promise<void> {
  await api.delete(`/api/shops/${shopId}/product-properties/${propertyId}`)
}
