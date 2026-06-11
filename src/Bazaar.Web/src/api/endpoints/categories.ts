import { api } from '../http'

export interface CategoryModel {
  categoryId: string
  name: string
}

export interface CreateCategoryResult {
  categoryId: string
  name: string
}

export async function listCategories(shopId: string): Promise<CategoryModel[]> {
  const { data } = await api.get(`/api/shops/${shopId}/Categories`)
  return Array.isArray(data) ? data : (data.items ?? [])
}

export async function createCategory(shopId: string, name: string): Promise<CreateCategoryResult> {
  const { data } = await api.post(`/api/shops/${shopId}/Categories`, { name })
  return data
}

export async function updateCategory(
  shopId: string,
  categoryId: string,
  name: string
): Promise<void> {
  await api.put(`/api/shops/${shopId}/Categories/${categoryId}`, { name })
}

export async function deleteCategory(shopId: string, categoryId: string): Promise<void> {
  await api.delete(`/api/shops/${shopId}/Categories/${categoryId}`)
}
