import { api, type SlidingCollectionWrapper, type SlidingParams } from '../http'

export const FactorType = {
  Buy: 1,
  Sell: 2
} as const

export type FactorTypeValue = (typeof FactorType)[keyof typeof FactorType]

export interface FactorModel {
  factorId: string
  counterpartyId?: string | null
  counterpartyFullName?: string | null
  type: FactorTypeValue
  notes?: string | null
  date: string
  isReversed: boolean
  itemCount: number
  createdOn: string
}

export interface FactorItemDetailModel {
  productId: string
  productName: string
  amount: number
  unitPrice: number
  total: number
}

export interface FactorDetailModel {
  factorId: string
  shopId: string
  type: FactorTypeValue
  counterpartyId?: string | null
  counterpartyFullName?: string | null
  notes?: string | null
  date: string
  isReversed: boolean
  createdOn: string
  items: FactorItemDetailModel[]
}

export interface FactorItemRequest {
  productId: string
  amount: number
  unitPrice: number
}

export interface CreateFactorPayload {
  type: FactorTypeValue
  counterpartyId: string
  notes?: string | null
  date: string
  items: FactorItemRequest[]
}

export interface EditFactorPayload {
  counterpartyId: string
  notes?: string | null
  date: string
  items: FactorItemRequest[]
}

export interface CreateFactorResult {
  factorId: string
}

export interface ListFactorsParams extends SlidingParams {
  type?: FactorTypeValue | null
}

export async function listFactors(
  shopId: string,
  p: ListFactorsParams = {}
): Promise<SlidingCollectionWrapper<FactorModel>> {
  const params: Record<string, any> = {
    'Pagination.Skip': p.skip ?? 0,
    'Pagination.Take': p.take ?? 30
  }
  if (p.type) params['Type'] = p.type
  const { data } = await api.get(`/api/shops/${shopId}/Factors`, { params })
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

export async function getFactor(shopId: string, factorId: string): Promise<FactorDetailModel> {
  const { data } = await api.get(`/api/shops/${shopId}/Factors/${factorId}`)
  return data
}

export async function createFactor(
  shopId: string,
  payload: CreateFactorPayload
): Promise<CreateFactorResult> {
  const { data } = await api.post(`/api/shops/${shopId}/Factors`, payload)
  return data
}

export async function editFactor(
  shopId: string,
  factorId: string,
  payload: EditFactorPayload
): Promise<void> {
  await api.put(`/api/shops/${shopId}/Factors/${factorId}`, payload)
}
