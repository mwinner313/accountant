<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { refDebounced } from '@vueuse/core'
import { useCounterpartyStore } from '@/stores/counterparties'
import { useConfirm } from '@/composables/useConfirm'
import * as api from '@/api/endpoints/counterparties'
import type { CreateCounterpartyPayload } from '@/api/endpoints/counterparties'
import { Check, Pencil, Plus, Search, Trash2, User, X } from '@lucide/vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Badge } from '@/components/ui/badge'
import { Progress } from '@/components/ui/progress'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from '@/components/ui/dialog'

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
  } catch {
    /* keep name from list */
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
    accept: async () => {
      await cps.remove(id)
    }
  })
}
</script>

<template>
  <PageHeader :title="t('counterparties.title')">
    <template #actions>
      <Button size="sm" @click="openCreate">
        <Plus class="size-4" />
        {{ t('counterparties.new') }}
      </Button>
    </template>
  </PageHeader>

  <div class="search-row">
    <Search class="search-icon size-4" />
    <Input v-model="search" :placeholder="t('app.search')" class="search-input" />
  </div>

  <Progress v-if="cps.loading" class="h-1 animate-pulse" :model-value="undefined" />

  <EmptyState
    v-if="!cps.loading && cps.items.length === 0"
    icon="users"
    :hint="t('counterparties.empty')"
  >
    <Button class="mt-3" @click="openCreate">
      <Plus class="size-4" />
      {{ t('counterparties.new') }}
    </Button>
  </EmptyState>

  <div v-else>
    <div v-for="c in cps.items" :key="c.counterpartyId" class="list-card">
      <div class="avatar tap" @click="goDetail(c.counterpartyId)"><User class="size-5" /></div>
      <div class="list-card__body tap" @click="goDetail(c.counterpartyId)">
        <div class="list-card__title">
          {{ c.fullName }}
          <Badge v-if="c._local" variant="secondary">آفلاین</Badge>
        </div>
      </div>
      <Button variant="ghost" size="icon" @click.stop="openEdit(c.counterpartyId, c.fullName)">
        <Pencil class="size-4" />
      </Button>
      <Button variant="ghost" size="icon" @click.stop="askDelete(c.counterpartyId)">
        <Trash2 class="size-4 text-destructive" />
      </Button>
    </div>
  </div>

  <Dialog v-model:open="dialog">
    <DialogContent class="sm:max-w-[520px]">
      <DialogHeader>
        <DialogTitle>{{ mode === 'create' ? t('counterparties.new') : t('app.edit') }}</DialogTitle>
      </DialogHeader>
      <div class="dlg">
        <div class="field">
          <Label for="cp-full-name">{{ t('counterparties.full_name') }}</Label>
          <Input id="cp-full-name" v-model="fullName" class="mt-1" />
        </div>

        <h4 class="sub">{{ t('counterparties.phones') }}</h4>
        <div v-for="(p, i) in phones" :key="'p' + i" class="row-inline">
          <Input v-model="p.number" :placeholder="t('counterparties.phone_placeholder')" />
          <Button
            v-if="phones.length > 1"
            variant="ghost"
            size="icon"
            @click="removePhone(i)"
          >
            <X class="size-4 text-destructive" />
          </Button>
        </div>
        <Button type="button" variant="ghost" size="sm" @click="addPhone">
          {{ t('counterparties.add_phone') }}
        </Button>

        <h4 class="sub">{{ t('counterparties.bank_accounts') }}</h4>
        <div v-for="(b, i) in bankAccounts" :key="'b' + i" class="bank-block">
          <Input v-model="b.name" :placeholder="t('counterparties.bank_name')" class="mb-2" />
          <Input
            v-model="b.accountNumber"
            :placeholder="t('counterparties.account_number')"
            class="mb-2"
          />
          <Input v-model="b.shebaNumber" :placeholder="t('counterparties.sheba')" class="mb-2" />
          <Input v-model="b.cardNumber" :placeholder="t('counterparties.card')" class="mb-2" />
          <Button
            v-if="bankAccounts.length > 1"
            variant="ghost"
            size="sm"
            @click="removeBank(i)"
          >
            {{ t('counterparties.remove_bank') }}
          </Button>
        </div>
        <Button type="button" variant="ghost" size="sm" @click="addBank">
          {{ t('counterparties.add_bank') }}
        </Button>
      </div>
      <DialogFooter>
        <Button variant="ghost" @click="dialog = false">{{ t('app.cancel') }}</Button>
        <Button @click="save">
          <Check class="size-4" />
          {{ t('app.save') }}
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>

<style scoped lang="scss">
.search-row {
  position: relative;
  margin-bottom: 1rem;

  .search-icon {
    position: absolute;
    inset-inline-end: 0.75rem;
    top: 50%;
    transform: translateY(-50%);
    color: var(--muted-foreground);
    pointer-events: none;
  }
  .search-input {
    padding-inline-end: 2.5rem;
  }
}
.avatar {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background: color-mix(in oklch, var(--primary) 15%, transparent);
  color: var(--primary);
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
  border: 1px solid var(--border);
  margin-bottom: 0.5rem;
}
.mb-2 {
  margin-bottom: 0.35rem;
}
.mt-3 {
  margin-top: 0.75rem;
}
</style>
