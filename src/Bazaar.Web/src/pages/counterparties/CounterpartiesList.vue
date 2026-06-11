<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { refDebounced } from '@vueuse/core'
import { useCounterpartyStore } from '@/stores/counterparties'
import { useConfirm } from 'primevue/useconfirm'
import * as api from '@/api/endpoints/counterparties'
import type { CreateCounterpartyPayload } from '@/api/endpoints/counterparties'
import { DevBypassError } from '@/api/http'

const { t } = useI18n()
const router = useRouter()
const cps = useCounterpartyStore()
const confirm = useConfirm()

const search = ref('')
const debouncedSearch = refDebounced(search, 350)

const dialog = ref(false)
const mode = ref<'create' | 'edit'>('create')
const editingId = ref<string | null>(null)

const fullName = ref('')
const phones = ref<{ number: string }[]>([{ number: '' }])
const bankAccounts = ref<
  { name: string; accountNumber: string; shebaNumber: string; cardNumber: string }[]
>([{ name: '', accountNumber: '', shebaNumber: '', cardNumber: '' }])

async function reload() {
  await cps.loadLocal()
  void cps.refresh(null)
}

onMounted(reload)
watch(debouncedSearch, () => {
  void cps.refresh(debouncedSearch.value.trim() || null)
})

function resetForm() {
  fullName.value = ''
  phones.value = [{ number: '' }]
  bankAccounts.value = [{ name: '', accountNumber: '', shebaNumber: '', cardNumber: '' }]
}

function openCreate() {
  mode.value = 'create'
  editingId.value = null
  resetForm()
  dialog.value = true
}

async function openEdit(id: string, name: string) {
  mode.value = 'edit'
  editingId.value = id
  fullName.value = name
  phones.value = [{ number: '' }]
  bankAccounts.value = [{ name: '', accountNumber: '', shebaNumber: '', cardNumber: '' }]
  dialog.value = true
  try {
    const d = await api.getCounterparty(id)
    fullName.value = d.fullName
    phones.value = d.phones.length ? d.phones.map(p => ({ number: p.number })) : [{ number: '' }]
    bankAccounts.value =
      d.bankAccounts.length > 0
        ? d.bankAccounts.map(b => ({
            name: b.name,
            accountNumber: b.accountNumber,
            shebaNumber: b.shebaNumber,
            cardNumber: b.cardNumber
          }))
        : [{ name: '', accountNumber: '', shebaNumber: '', cardNumber: '' }]
  } catch (e) {
    if (!(e instanceof DevBypassError)) {
      /* keep name from list */
    }
  }
}

function addPhone() {
  phones.value.push({ number: '' })
}
function removePhone(idx: number) {
  if (phones.value.length > 1) phones.value.splice(idx, 1)
}

function addBank() {
  bankAccounts.value.push({ name: '', accountNumber: '', shebaNumber: '', cardNumber: '' })
}
function removeBank(idx: number) {
  if (bankAccounts.value.length > 1) bankAccounts.value.splice(idx, 1)
}

function buildPayload(): CreateCounterpartyPayload {
  return {
    fullName: fullName.value.trim(),
    phones: phones.value.map(p => ({ number: p.number.trim() })).filter(p => p.number.length > 0),
    bankAccounts: bankAccounts.value
      .map(b => ({
        name: b.name.trim(),
        accountNumber: b.accountNumber.trim(),
        shebaNumber: b.shebaNumber.trim(),
        cardNumber: b.cardNumber.trim()
      }))
      .filter(b => b.name.length > 0)
  }
}

async function save() {
  const payload = buildPayload()
  if (!payload.fullName) return
  if (mode.value === 'create') {
    await cps.create(payload)
  } else if (editingId.value) {
    await cps.update(editingId.value, payload)
  }
  dialog.value = false
}

function goDetail(counterpartyId: string) {
  router.push({ name: 'counterparty-detail', params: { id: counterpartyId } })
}

function askDelete(id: string) {
  confirm.require({
    message: t('counterparties.delete_confirm'),
    acceptClass: 'p-button-danger',
    accept: async () => {
      await cps.remove(id)
    }
  })
}
</script>

<template>
  <PageHeader :title="t('counterparties.title')">
    <template #actions>
      <Button :label="t('counterparties.new')" icon="pi pi-plus" size="small" @click="openCreate" />
    </template>
  </PageHeader>

  <IconField class="search-row">
    <InputIcon class="pi pi-search" />
    <InputText v-model="search" :placeholder="t('app.search')" fluid />
  </IconField>

  <ProgressBar v-if="cps.loading" mode="indeterminate" style="height: 3px" />

  <EmptyState
    v-if="!cps.loading && cps.items.length === 0"
    icon="pi-users"
    :hint="t('counterparties.empty')"
  >
    <Button :label="t('counterparties.new')" icon="pi pi-plus" class="mt-3" @click="openCreate" />
  </EmptyState>

  <div v-else>
    <div v-for="c in cps.items" :key="c.counterpartyId" class="list-card">
      <div class="avatar tap" @click="goDetail(c.counterpartyId)"><i class="pi pi-user" /></div>
      <div class="list-card__body tap" @click="goDetail(c.counterpartyId)">
        <div class="list-card__title">
          {{ c.fullName }}
          <Tag v-if="c._local" value="آفلاین" severity="info" />
        </div>
      </div>
      <Button
        icon="pi pi-pencil"
        text
        rounded
        @click.stop="openEdit(c.counterpartyId, c.fullName)"
      />
      <Button
        icon="pi pi-trash"
        text
        rounded
        severity="danger"
        @click.stop="askDelete(c.counterpartyId)"
      />
    </div>
  </div>

  <Dialog
    v-model:visible="dialog"
    :header="mode === 'create' ? t('counterparties.new') : t('app.edit')"
    modal
    style="width: min(520px, 94vw)"
  >
    <div class="dlg">
      <label class="field">
        <span class="label">{{ t('counterparties.full_name') }}</span>
        <InputText v-model="fullName" fluid />
      </label>

      <h4 class="sub">{{ t('counterparties.phones') }}</h4>
      <div v-for="(p, i) in phones" :key="'p' + i" class="row-inline">
        <InputText v-model="p.number" :placeholder="t('counterparties.phone_placeholder')" fluid />
        <Button
          v-if="phones.length > 1"
          icon="pi pi-times"
          text
          rounded
          severity="danger"
          @click="removePhone(i)"
        />
      </div>
      <Button type="button" :label="t('counterparties.add_phone')" text size="small" @click="addPhone" />

      <h4 class="sub">{{ t('counterparties.bank_accounts') }}</h4>
      <div v-for="(b, i) in bankAccounts" :key="'b' + i" class="bank-block">
        <InputText v-model="b.name" :placeholder="t('counterparties.bank_name')" fluid class="mb-2" />
        <InputText v-model="b.accountNumber" :placeholder="t('counterparties.account_number')" fluid class="mb-2" />
        <InputText v-model="b.shebaNumber" :placeholder="t('counterparties.sheba')" fluid class="mb-2" />
        <InputText v-model="b.cardNumber" :placeholder="t('counterparties.card')" fluid class="mb-2" />
        <Button
          v-if="bankAccounts.length > 1"
          :label="t('counterparties.remove_bank')"
          text
          size="small"
          severity="danger"
          @click="removeBank(i)"
        />
      </div>
      <Button type="button" :label="t('counterparties.add_bank')" text size="small" @click="addBank" />
    </div>
    <template #footer>
      <Button :label="t('app.cancel')" text @click="dialog = false" />
      <Button :label="t('app.save')" icon="pi pi-check" @click="save" />
    </template>
  </Dialog>
</template>

<style scoped lang="scss">
.search-row {
  margin-bottom: 1rem;
}
.avatar {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background: var(--p-primary-100, #e0e7ff);
  color: var(--p-primary-color);
  display: flex;
  align-items: center;
  justify-content: center;
}
.tap {
  cursor: pointer;
}
.dlg {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
.field .label {
  display: block;
  font-weight: 600;
  font-size: 0.9rem;
  margin-bottom: 0.35rem;
}
.sub {
  margin: 0.5rem 0 0;
  font-size: 0.95rem;
  font-weight: 700;
}
.row-inline {
  display: flex;
  gap: 0.35rem;
  align-items: center;
}
.bank-block {
  padding: 0.75rem;
  border-radius: 10px;
  border: 1px solid var(--p-content-border-color);
  margin-bottom: 0.5rem;
}
.mb-2 {
  margin-bottom: 0.35rem;
}
.mt-3 {
  margin-top: 0.75rem;
}
</style>
