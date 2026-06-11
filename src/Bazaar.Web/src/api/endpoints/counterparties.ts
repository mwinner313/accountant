import { api, type SlidingCollectionWrapper, type SlidingParams } from '../http'

export interface CounterpartyPhoneDto {
  number: string
}

export interface CounterpartyBankAccountDto {
  name: string
  accountNumber: string
  shebaNumber: string
  cardNumber: string
}

export interface CounterpartyListModel {
  counterpartyId: string
  fullName: string
}

export interface CounterpartyDetailModel {
  counterpartyId: string
  fullName: string
  phones: CounterpartyPhoneDto[]
  bankAccounts: CounterpartyBankAccountDto[]
}

export interface CreateCounterpartyPayload {
  fullName: string
  phones: CounterpartyPhoneDto[]
  bankAccounts: CounterpartyBankAccountDto[]
}

export interface CreateCounterpartyResult {
  counterpartyId: string
}

export interface ListCounterpartiesParams extends SlidingParams {
  search?: string | null
}

export async function listCounterparties(
  p: ListCounterpartiesParams = {}
): Promise<SlidingCollectionWrapper<CounterpartyListModel>> {
  const params: Record<string, any> = {
    'Pagination.Skip': p.skip ?? 0,
    'Pagination.Take': p.take ?? 30
  }
  if (p.search) params['Search'] = p.search
  const { data } = await api.get('/api/Counterparties', { params })
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

export async function getCounterparty(counterpartyId: string): Promise<CounterpartyDetailModel> {
  const { data } = await api.get(`/api/Counterparties/${counterpartyId}`)
  return data
}

export async function createCounterparty(
  payload: CreateCounterpartyPayload
): Promise<CreateCounterpartyResult> {
  const { data } = await api.post('/api/Counterparties', payload)
  return data
}

export async function updateCounterparty(
  counterpartyId: string,
  payload: CreateCounterpartyPayload
): Promise<void> {
  await api.put(`/api/Counterparties/${counterpartyId}`, {
    fullName: payload.fullName,
    phones: payload.phones,
    bankAccounts: payload.bankAccounts
  })
}

export async function deleteCounterparty(counterpartyId: string): Promise<void> {
  await api.delete(`/api/Counterparties/${counterpartyId}`)
}
