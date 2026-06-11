<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useShopStore } from '@/stores/shops'
import { useFactorStore } from '@/stores/factors'
import { useProductStore } from '@/stores/products'
import { useCategoryStore } from '@/stores/categories'
import { useCounterpartyStore } from '@/stores/counterparties'
import {
  FactorType,
  type FactorTypeValue,
  type FactorItemRequest
} from '@/api/endpoints/factors'
import type { CounterpartyListModel, CreateCounterpartyPayload } from '@/api/endpoints/counterparties'
import type { CreateProductPayload } from '@/api/endpoints/products'
import { formatMoney } from '@/i18n/format'
import { useToast } from 'primevue/usetoast'
import JalaliPrimeDatePicker from '@/components/JalaliPrimeDatePicker.vue'

defineProps<{ id?: string }>()

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const toast = useToast()
const shops = useShopStore()
const factors = useFactorStore()
const products = useProductStore()
const cats = useCategoryStore()
const counterparties = useCounterpartyStore()

const id = computed(() => (route.params.id as string | undefined) ?? null)
const isNew = computed(() => !id.value)

const type = ref<FactorTypeValue>(
  Number(route.query.type ?? FactorType.Sell) as FactorTypeValue
)
const date = ref<Date>(new Date())
const notes = ref<string>('')
const items = ref<FactorItemRequest[]>([])

const selectedCp = ref<CounterpartyListModel | null>(null)
const cpSuggestions = ref<CounterpartyListModel[]>([])

const newCpDialog = ref(false)
const newFullName = ref('')
const newPhones = ref<{ number: string }[]>([{ number: '' }])
const newBanks = ref<
  { name: string; accountNumber: string; shebaNumber: string; cardNumber: string }[]
>([{ name: '', accountNumber: '', shebaNumber: '', cardNumber: '' }])

const newProductDialog = ref(false)
const newProductRowIdx = ref<number | null>(null)
const newProductForm = ref<CreateProductPayload>({
  categoryId: null,
  name: '',
  unit: '',
  picture: null,
  sellPrice: 0,
  buyPrice: 0
})

const productOptions = computed(() =>
  products.items.map(p => ({
    productId: p.productId,
    name: p.name,
    unit: p.unit,
    sellPrice: Number(p.sellPrice),
    buyPrice: Number(p.buyPrice)
  }))
)

const grandTotal = computed(() =>
  items.value.reduce((sum, it) => sum + Number(it.amount || 0) * Number(it.unitPrice || 0), 0)
)

async function onCpSearch(event: { query: string }) {
  cpSuggestions.value = await counterparties.searchRemote(event.query ?? '')
}

function resetNewCp() {
  newFullName.value = ''
  newPhones.value = [{ number: '' }]
  newBanks.value = [{ name: '', accountNumber: '', shebaNumber: '', cardNumber: '' }]
}

function openNewCp() {
  resetNewCp()
  newCpDialog.value = true
}

function addNewPhone() {
  newPhones.value.push({ number: '' })
}
function removeNewPhone(i: number) {
  if (newPhones.value.length > 1) newPhones.value.splice(i, 1)
}
function addNewBank() {
  newBanks.value.push({ name: '', accountNumber: '', shebaNumber: '', cardNumber: '' })
}
function removeNewBank(i: number) {
  if (newBanks.value.length > 1) newBanks.value.splice(i, 1)
}

function buildNewCpPayload(): CreateCounterpartyPayload {
  return {
    fullName: newFullName.value.trim(),
    phones: newPhones.value
      .map(p => ({ number: p.number.trim() }))
      .filter(p => p.number.length > 0),
    bankAccounts: newBanks.value
      .map(b => ({
        name: b.name.trim(),
        accountNumber: b.accountNumber.trim(),
        shebaNumber: b.shebaNumber.trim(),
        cardNumber: b.cardNumber.trim()
      }))
      .filter(b => b.name.length > 0)
  }
}

async function saveNewCp() {
  const payload = buildNewCpPayload()
  if (!payload.fullName) {
    toast.add({ severity: 'warn', summary: t('counterparties.full_name'), life: 2000 })
    return
  }
  const row = await counterparties.create(payload)
  selectedCp.value = { counterpartyId: row.counterpartyId, fullName: row.fullName }
  newCpDialog.value = false
}

function resetNewProduct() {
  newProductForm.value = {
    categoryId: null,
    name: '',
    unit: '',
    picture: null,
    sellPrice: 0,
    buyPrice: 0
  }
}

function openNewProduct(idx: number) {
  newProductRowIdx.value = idx
  resetNewProduct()
  if (shops.activeShopId) void cats.refresh(shops.activeShopId)
  newProductDialog.value = true
}

async function saveNewProduct() {
  const shopId = shops.activeShopId
  const rowIdx = newProductRowIdx.value
  if (!shopId || rowIdx === null) return
  const f = newProductForm.value
  if (!f.name.trim() || !f.unit.trim()) {
    toast.add({ severity: 'warn', summary: t('validation.required'), life: 2000 })
    return
  }
  const payload: CreateProductPayload = {
    categoryId: f.categoryId ?? null,
    name: f.name.trim(),
    unit: f.unit.trim(),
    picture: f.picture?.trim() || null,
    sellPrice: Number(f.sellPrice) || 0,
    buyPrice: Number(f.buyPrice) || 0
  }
  const created = await products.create(shopId, payload)
  items.value[rowIdx].productId = created.productId
  items.value[rowIdx].unitPrice =
    type.value === FactorType.Buy ? Number(created.buyPrice) : Number(created.sellPrice)
  newProductDialog.value = false
  newProductRowIdx.value = null
}

async function reload() {
  void counterparties.loadLocal()
  void counterparties.refresh(null)
  if (!shops.activeShopId) return
  void products.refresh(shops.activeShopId)
  if (id.value) {
    await factors.loadDetail(shops.activeShopId, id.value)
    const d = factors.detail
    if (d) {
      type.value = d.type
      date.value = new Date(d.date)
      notes.value = d.notes ?? ''
      items.value = d.items.map(it => ({
        productId: it.productId,
        amount: Number(it.amount),
        unitPrice: Number(it.unitPrice)
      }))
      if (d.counterpartyId && d.counterpartyFullName) {
        selectedCp.value = {
          counterpartyId: d.counterpartyId,
          fullName: d.counterpartyFullName
        }
      } else {
        selectedCp.value = null
      }
    }
  } else {
    selectedCp.value = null
    if (!items.value.length) addItem()
  }
}

onMounted(reload)
watch(() => route.params.id, reload)

function addItem() {
  items.value.push({ productId: '', amount: 1, unitPrice: 0 })
}

function removeItem(idx: number) {
  items.value.splice(idx, 1)
}

function onProductChange(idx: number, productId: string) {
  const p = productOptions.value.find(x => x.productId === productId)
  if (!p) return
  items.value[idx].productId = productId
  items.value[idx].unitPrice = type.value === FactorType.Buy ? p.buyPrice : p.sellPrice
}

async function save() {
  if (!shops.activeShopId) return
  if (!selectedCp.value?.counterpartyId) {
    toast.add({ severity: 'warn', summary: t('factors.counterparty_required'), life: 2500 })
    return
  }
  const cleaned = items.value.filter(it => it.productId && it.amount > 0)
  if (cleaned.length === 0) {
    toast.add({ severity: 'warn', summary: t('factors.no_items'), life: 2500 })
    return
  }
  const payload = {
    type: type.value,
    counterpartyId: selectedCp.value.counterpartyId,
    notes: notes.value || null,
    date: date.value.toISOString(),
    items: cleaned
  }
  if (isNew.value) {
    await factors.create(shops.activeShopId, payload)
    toast.add({ severity: 'success', summary: t('app.saved_locally'), life: 2000 })
    router.replace({ name: 'factors' })
  } else if (id.value) {
    await factors.edit(shops.activeShopId, id.value, {
      counterpartyId: selectedCp.value.counterpartyId,
      notes: notes.value || null,
      date: date.value.toISOString(),
      items: cleaned
    })
    toast.add({ severity: 'success', summary: t('app.saved_locally'), life: 2000 })
    router.replace({ name: 'factors' })
  }
}
</script>

<template>
  <PageHeader
    :title="isNew ? (type === FactorType.Buy ? t('factors.new_buy') : t('factors.new_sell')) : t('app.edit')"
  >
    <template #actions>
      <Button icon="pi pi-arrow-right" text rounded @click="router.back()" />
    </template>
  </PageHeader>

  <Message v-if="!isNew" severity="warn" :closable="false" class="warn">
    {{ t('factors.edit_warning') }}
  </Message>

  <form class="form" @submit.prevent="save">
    <div class="row">
      <label class="field">
        <span class="label">{{ t('factors.type') }}</span>
        <SelectButton
          v-model="type"
          :disabled="!isNew"
          :options="[
            { value: FactorType.Buy, label: t('factors.type_buy') },
            { value: FactorType.Sell, label: t('factors.type_sell') }
          ]"
          optionLabel="label"
          optionValue="value"
        />
      </label>
      <label class="field">
        <span class="label">{{ t('factors.date') }}</span>
        <JalaliPrimeDatePicker v-model="date" type="datetime" fluid />
      </label>
    </div>

    <div class="cp-row">
      <label class="field cp-field">
        <span class="label">{{ t('factors.counterparty') }}</span>
        <AutoComplete
          v-model="selectedCp"
          :suggestions="cpSuggestions"
          optionLabel="fullName"
          :placeholder="t('factors.counterparty_placeholder')"
          dropdown
          forceSelection
          fluid
          @complete="onCpSearch"
        />
      </label>
      <Button
        type="button"
        class="new-cp-btn"
        :label="t('factors.new_counterparty')"
        icon="pi pi-user-plus"
        severity="secondary"
        outlined
        @click="openNewCp"
      />
    </div>

    <label class="field">
      <span class="label">{{ t('factors.notes') }}</span>
      <Textarea v-model="notes" rows="2" fluid autoResize />
    </label>

    <h3 class="section-title">{{ t('factors.items') }}</h3>

    <div v-for="(it, idx) in items" :key="idx" class="item-row">
      <Select
        :modelValue="it.productId"
        :options="productOptions"
        optionLabel="name"
        optionValue="productId"
        :placeholder="t('factors.product')"
        @update:modelValue="(v: any) => onProductChange(idx, v)"
        class="item-product"
        fluid
      />
      <Button
        type="button"
        class="new-product-btn"
        icon="pi pi-box"
        rounded
        outlined
        severity="secondary"
        :aria-label="t('factors.new_product')"
        :title="t('factors.new_product')"
        @click="openNewProduct(idx)"
      />
      <InputNumber v-model="it.amount" :min="0" :minFractionDigits="0" :maxFractionDigits="4" class="item-amount" fluid />
      <InputNumber v-model="it.unitPrice" :min="0" :minFractionDigits="0" class="item-price" fluid />
      <Button
        icon="pi pi-trash"
        text
        rounded
        severity="danger"
        size="small"
        @click="removeItem(idx)"
      />
    </div>

    <Button
      type="button"
      :label="t('factors.add_item')"
      icon="pi pi-plus"
      severity="secondary"
      outlined
      fluid
      @click="addItem"
    />

    <div class="total">
      <span>{{ t('factors.grand_total') }}</span>
      <strong>{{ formatMoney(grandTotal) }}</strong>
    </div>

    <Button type="submit" :label="t('app.save')" icon="pi pi-check" fluid class="save-btn" />
  </form>

  <Dialog
    v-model:visible="newCpDialog"
    :header="t('factors.new_counterparty')"
    modal
    style="width: min(520px, 94vw)"
  >
    <div class="dlg">
      <label class="field">
        <span class="label">{{ t('counterparties.full_name') }}</span>
        <InputText v-model="newFullName" fluid />
      </label>
      <h4 class="sub">{{ t('counterparties.phones') }}</h4>
      <div v-for="(p, i) in newPhones" :key="'np' + i" class="row-inline">
        <InputText v-model="p.number" :placeholder="t('counterparties.phone_placeholder')" fluid />
        <Button
          v-if="newPhones.length > 1"
          icon="pi pi-times"
          text
          rounded
          severity="danger"
          @click="removeNewPhone(i)"
        />
      </div>
      <Button type="button" :label="t('counterparties.add_phone')" text size="small" @click="addNewPhone" />
      <h4 class="sub">{{ t('counterparties.bank_accounts') }}</h4>
      <div v-for="(b, i) in newBanks" :key="'nb' + i" class="bank-block">
        <InputText v-model="b.name" :placeholder="t('counterparties.bank_name')" fluid class="mb-2" />
        <InputText v-model="b.accountNumber" :placeholder="t('counterparties.account_number')" fluid class="mb-2" />
        <InputText v-model="b.shebaNumber" :placeholder="t('counterparties.sheba')" fluid class="mb-2" />
        <InputText v-model="b.cardNumber" :placeholder="t('counterparties.card')" fluid class="mb-2" />
        <Button
          v-if="newBanks.length > 1"
          :label="t('counterparties.remove_bank')"
          text
          size="small"
          severity="danger"
          @click="removeNewBank(i)"
        />
      </div>
      <Button type="button" :label="t('counterparties.add_bank')" text size="small" @click="addNewBank" />
    </div>
    <template #footer>
      <Button :label="t('app.cancel')" text @click="newCpDialog = false" />
      <Button :label="t('app.save')" icon="pi pi-check" @click="saveNewCp" />
    </template>
  </Dialog>

  <Dialog
    v-model:visible="newProductDialog"
    :header="t('factors.new_product_title')"
    modal
    style="width: min(520px, 94vw)"
  >
    <div class="dlg">
      <label class="field">
        <span class="label">{{ t('products.name_label') }}</span>
        <InputText v-model="newProductForm.name" fluid />
      </label>
      <div class="row">
        <label class="field">
          <span class="label">{{ t('products.unit_label') }}</span>
          <InputText
            v-model="newProductForm.unit"
            :placeholder="t('products.unit_placeholder')"
            fluid
          />
        </label>
        <label class="field">
          <span class="label">{{ t('products.category') }}</span>
          <Select
            v-model="newProductForm.categoryId"
            :options="[{ categoryId: null, name: t('products.no_category') }, ...cats.items]"
            optionLabel="name"
            optionValue="categoryId"
            fluid
          />
        </label>
      </div>
      <div class="row">
        <label class="field">
          <span class="label">{{ t('products.sell_price') }}</span>
          <InputNumber v-model="newProductForm.sellPrice" :min="0" :minFractionDigits="0" fluid />
        </label>
        <label class="field">
          <span class="label">{{ t('products.buy_price') }}</span>
          <InputNumber v-model="newProductForm.buyPrice" :min="0" :minFractionDigits="0" fluid />
        </label>
      </div>
    </div>
    <template #footer>
      <Button :label="t('app.cancel')" text @click="newProductDialog = false" />
      <Button :label="t('app.save')" icon="pi pi-check" @click="saveNewProduct" />
    </template>
  </Dialog>
</template>

<style scoped lang="scss">
.warn {
  margin-bottom: 1rem;
}
.form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
}
.cp-row {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.cp-field {
  flex: 1;
  min-width: 0;
}
.new-cp-btn {
  align-self: stretch;
}
.field {
  display: block;
  .label {
    display: block;
    font-weight: 600;
    font-size: 0.9rem;
    margin-bottom: 0.4rem;
  }
}
.section-title {
  margin: 0.5rem 0 0;
  font-size: 1rem;
  font-weight: 700;
}
.item-row {
  display: grid;
  grid-template-columns: minmax(0, 2fr) auto minmax(0, 1fr) minmax(0, 1.2fr) auto;
  gap: 0.5rem;
  align-items: center;
}
.new-product-btn {
  flex-shrink: 0;
}
.total {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.75rem 1rem;
  border-radius: 10px;
  background: var(--p-surface-100, #f1f5f9);
  font-size: 1rem;

  strong {
    color: var(--p-primary-color);
    font-size: 1.1rem;
  }
}
.save-btn {
  margin-top: 0.5rem;
}
.dlg .sub {
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
</style>
