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
import { toast } from 'vue-sonner'
import JalaliDatePicker from '@/components/JalaliDatePicker.vue'
import {
  ArrowRight,
  Box,
  Check,
  Plus,
  Trash2,
  UserPlus,
  X
} from '@lucide/vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle-group'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from '@/components/ui/dialog'

defineProps<{ id?: string }>()

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
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
const typeStr = computed({
  get: () => String(type.value),
  set: (v: string) => {
    type.value = Number(v) as FactorTypeValue
  }
})

const date = ref<Date>(new Date())
const notes = ref<string>('')
const items = ref<FactorItemRequest[]>([])

const selectedCp = ref<CounterpartyListModel | null>(null)
const cpSearchText = ref('')
const cpSuggestions = ref<CounterpartyListModel[]>([])
const showCpDropdown = ref(false)

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

const newProductCategorySelect = computed({
  get: () =>
    newProductForm.value.categoryId == null ? 'none' : String(newProductForm.value.categoryId),
  set: (v: string) => {
    newProductForm.value.categoryId = v === 'none' ? null : v
  }
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

async function onCpSearch(query: string) {
  cpSuggestions.value = await counterparties.searchRemote(query ?? '')
  showCpDropdown.value = cpSuggestions.value.length > 0
}

function onCpInput(value: string | number) {
  const q = String(value)
  cpSearchText.value = q
  selectedCp.value = null
  void onCpSearch(q)
}

function selectCp(cp: CounterpartyListModel) {
  selectedCp.value = cp
  cpSearchText.value = cp.fullName
  showCpDropdown.value = false
}

function hideCpDropdown() {
  window.setTimeout(() => {
    showCpDropdown.value = false
  }, 150)
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
    toast.warning(t('counterparties.full_name'))
    return
  }
  const row = await counterparties.create(payload)
  selectCp({ counterpartyId: row.counterpartyId, fullName: row.fullName })
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
    toast.warning(t('validation.required'))
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
        selectCp({
          counterpartyId: d.counterpartyId,
          fullName: d.counterpartyFullName
        })
      } else {
        selectedCp.value = null
        cpSearchText.value = ''
      }
    }
  } else {
    selectedCp.value = null
    cpSearchText.value = ''
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
    toast.warning(t('factors.counterparty_required'))
    return
  }
  const cleaned = items.value.filter(it => it.productId && it.amount > 0)
  if (cleaned.length === 0) {
    toast.warning(t('factors.no_items'))
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
    toast.success(t('app.saved_locally'))
    router.replace({ name: 'factors' })
  } else if (id.value) {
    await factors.edit(shops.activeShopId, id.value, {
      counterpartyId: selectedCp.value.counterpartyId,
      notes: notes.value || null,
      date: date.value.toISOString(),
      items: cleaned
    })
    toast.success(t('app.saved_locally'))
    router.replace({ name: 'factors' })
  }
}
</script>

<template>
  <PageHeader
    :title="isNew ? (type === FactorType.Buy ? t('factors.new_buy') : t('factors.new_sell')) : t('app.edit')"
  >
    <template #actions>
      <Button variant="ghost" size="icon" @click="router.back()">
        <ArrowRight class="size-4" />
      </Button>
    </template>
  </PageHeader>

  <Alert v-if="!isNew" variant="default" class="warn">
    <AlertDescription>{{ t('factors.edit_warning') }}</AlertDescription>
  </Alert>

  <form class="form" @submit.prevent="save">
    <div class="row">
      <div class="field">
        <Label>{{ t('factors.type') }}</Label>
        <ToggleGroup
          v-model="typeStr"
          type="single"
          variant="outline"
          :disabled="!isNew"
          class="mt-1 w-full"
        >
          <ToggleGroupItem :value="String(FactorType.Buy)" class="flex-1">{{
            t('factors.type_buy')
          }}</ToggleGroupItem>
          <ToggleGroupItem :value="String(FactorType.Sell)" class="flex-1">{{
            t('factors.type_sell')
          }}</ToggleGroupItem>
        </ToggleGroup>
      </div>
      <div class="field">
        <Label>{{ t('factors.date') }}</Label>
        <JalaliDatePicker v-model="date" type="datetime" fluid class="mt-1" />
      </div>
    </div>

    <div class="cp-row">
      <div class="field cp-field">
        <Label for="cp-search">{{ t('factors.counterparty') }}</Label>
        <div class="cp-autocomplete mt-1">
          <Input
            id="cp-search"
            :model-value="cpSearchText"
            :placeholder="t('factors.counterparty_placeholder')"
            autocomplete="off"
            @update:model-value="onCpInput"
            @focus="showCpDropdown = cpSuggestions.length > 0"
            @blur="hideCpDropdown"
          />
          <div v-if="showCpDropdown" class="cp-dropdown">
            <button
              v-for="cp in cpSuggestions"
              :key="cp.counterpartyId"
              type="button"
              class="cp-suggestion"
              @mousedown.prevent="selectCp(cp)"
            >
              {{ cp.fullName }}
            </button>
          </div>
        </div>
      </div>
      <Button type="button" class="new-cp-btn" variant="outline" @click="openNewCp">
        <UserPlus class="size-4" />
        {{ t('factors.new_counterparty') }}
      </Button>
    </div>

    <div class="field">
      <Label for="factor-notes">{{ t('factors.notes') }}</Label>
      <Textarea id="factor-notes" v-model="notes" rows="2" class="mt-1" />
    </div>

    <h3 class="section-title">{{ t('factors.items') }}</h3>

    <div v-for="(it, idx) in items" :key="idx" class="item-row">
      <Select
        :model-value="it.productId || undefined"
        @update:model-value="v => v != null && onProductChange(idx, String(v))"
      >
        <SelectTrigger class="item-product w-full">
          <SelectValue :placeholder="t('factors.product')" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem
            v-for="p in productOptions"
            :key="p.productId"
            :value="p.productId"
          >
            {{ p.name }}
          </SelectItem>
        </SelectContent>
      </Select>
      <Button
        type="button"
        class="new-product-btn"
        variant="outline"
        size="icon"
        :aria-label="t('factors.new_product')"
        :title="t('factors.new_product')"
        @click="openNewProduct(idx)"
      >
        <Box class="size-4" />
      </Button>
      <Input
        type="number"
        min="0"
        step="any"
        class="item-amount"
        :model-value="String(it.amount)"
        @update:model-value="v => (it.amount = Number(v) || 0)"
      />
      <Input
        type="number"
        min="0"
        class="item-price"
        :model-value="String(it.unitPrice)"
        @update:model-value="v => (it.unitPrice = Number(v) || 0)"
      />
      <Button variant="ghost" size="icon" @click="removeItem(idx)">
        <Trash2 class="size-4 text-destructive" />
      </Button>
    </div>

    <Button type="button" variant="outline" class="w-full" @click="addItem">
      <Plus class="size-4" />
      {{ t('factors.add_item') }}
    </Button>

    <div class="total">
      <span>{{ t('factors.grand_total') }}</span>
      <strong>{{ formatMoney(grandTotal) }}</strong>
    </div>

    <Button type="submit" class="save-btn w-full">
      <Check class="size-4" />
      {{ t('app.save') }}
    </Button>
  </form>

  <Dialog v-model:open="newCpDialog">
    <DialogContent class="sm:max-w-[520px]">
      <DialogHeader>
        <DialogTitle>{{ t('factors.new_counterparty') }}</DialogTitle>
      </DialogHeader>
      <div class="dlg">
        <div class="field">
          <Label for="new-cp-name">{{ t('counterparties.full_name') }}</Label>
          <Input id="new-cp-name" v-model="newFullName" class="mt-1" />
        </div>
        <h4 class="sub">{{ t('counterparties.phones') }}</h4>
        <div v-for="(p, i) in newPhones" :key="'np' + i" class="row-inline">
          <Input v-model="p.number" :placeholder="t('counterparties.phone_placeholder')" />
          <Button
            v-if="newPhones.length > 1"
            variant="ghost"
            size="icon"
            @click="removeNewPhone(i)"
          >
            <X class="size-4 text-destructive" />
          </Button>
        </div>
        <Button type="button" variant="ghost" size="sm" @click="addNewPhone">
          {{ t('counterparties.add_phone') }}
        </Button>
        <h4 class="sub">{{ t('counterparties.bank_accounts') }}</h4>
        <div v-for="(b, i) in newBanks" :key="'nb' + i" class="bank-block">
          <Input v-model="b.name" :placeholder="t('counterparties.bank_name')" class="mb-2" />
          <Input
            v-model="b.accountNumber"
            :placeholder="t('counterparties.account_number')"
            class="mb-2"
          />
          <Input v-model="b.shebaNumber" :placeholder="t('counterparties.sheba')" class="mb-2" />
          <Input v-model="b.cardNumber" :placeholder="t('counterparties.card')" class="mb-2" />
          <Button
            v-if="newBanks.length > 1"
            variant="ghost"
            size="sm"
            @click="removeNewBank(i)"
          >
            {{ t('counterparties.remove_bank') }}
          </Button>
        </div>
        <Button type="button" variant="ghost" size="sm" @click="addNewBank">
          {{ t('counterparties.add_bank') }}
        </Button>
      </div>
      <DialogFooter>
        <Button variant="ghost" @click="newCpDialog = false">{{ t('app.cancel') }}</Button>
        <Button @click="saveNewCp">
          <Check class="size-4" />
          {{ t('app.save') }}
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>

  <Dialog v-model:open="newProductDialog">
    <DialogContent class="sm:max-w-[520px]">
      <DialogHeader>
        <DialogTitle>{{ t('factors.new_product_title') }}</DialogTitle>
      </DialogHeader>
      <div class="dlg">
        <div class="field">
          <Label for="new-prod-name">{{ t('products.name_label') }}</Label>
          <Input id="new-prod-name" v-model="newProductForm.name" class="mt-1" />
        </div>
        <div class="row">
          <div class="field">
            <Label for="new-prod-unit">{{ t('products.unit_label') }}</Label>
            <Input
              id="new-prod-unit"
              v-model="newProductForm.unit"
              :placeholder="t('products.unit_placeholder')"
              class="mt-1"
            />
          </div>
          <div class="field">
            <Label>{{ t('products.category') }}</Label>
            <Select v-model="newProductCategorySelect" class="mt-1">
              <SelectTrigger class="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="none">{{ t('products.no_category') }}</SelectItem>
                <SelectItem
                  v-for="c in cats.items"
                  :key="c.categoryId"
                  :value="String(c.categoryId)"
                >
                  {{ c.name }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
        <div class="row">
          <div class="field">
            <Label for="new-sell">{{ t('products.sell_price') }}</Label>
            <Input
              id="new-sell"
              type="number"
              min="0"
              :model-value="String(newProductForm.sellPrice)"
              class="mt-1"
              @update:model-value="v => (newProductForm.sellPrice = Number(v) || 0)"
            />
          </div>
          <div class="field">
            <Label for="new-buy">{{ t('products.buy_price') }}</Label>
            <Input
              id="new-buy"
              type="number"
              min="0"
              :model-value="String(newProductForm.buyPrice)"
              class="mt-1"
              @update:model-value="v => (newProductForm.buyPrice = Number(v) || 0)"
            />
          </div>
        </div>
      </div>
      <DialogFooter>
        <Button variant="ghost" @click="newProductDialog = false">{{ t('app.cancel') }}</Button>
        <Button @click="saveNewProduct">
          <Check class="size-4" />
          {{ t('app.save') }}
        </Button>
      </DialogFooter>
    </DialogContent>
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
.cp-autocomplete {
  position: relative;
}
.cp-dropdown {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  z-index: 50;
  margin-top: 0.25rem;
  background: var(--popover);
  border: 1px solid var(--border);
  border-radius: 0.375rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  max-height: 200px;
  overflow-y: auto;
}
.cp-suggestion {
  display: block;
  width: 100%;
  text-align: start;
  padding: 0.5rem 0.75rem;
  background: transparent;
  border: none;
  font: inherit;
  cursor: pointer;

  &:hover {
    background: var(--accent);
  }
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
  background: var(--muted);
  font-size: 1rem;

  strong {
    color: var(--primary);
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
  border: 1px solid var(--border);
  margin-bottom: 0.5rem;
}
.mb-2 {
  margin-bottom: 0.35rem;
}
</style>
