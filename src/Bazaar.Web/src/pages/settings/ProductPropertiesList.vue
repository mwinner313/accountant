<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useShopStore } from '@/stores/shops'
import { useProductPropertyStore } from '@/stores/productProperties'
import { useConfirm } from '@/composables/useConfirm'
import { Check, Pencil, Plus, SlidersHorizontal, Trash2 } from '@lucide/vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Badge } from '@/components/ui/badge'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from '@/components/ui/dialog'

const { t } = useI18n()
const shops = useShopStore()
const props = useProductPropertyStore()
const confirm = useConfirm()

const dialog = ref(false)
const mode = ref<'create' | 'edit'>('create')
const editingId = ref<string | null>(null)
const name = ref('')

async function reload() {
  if (!shops.activeShopId) return
  await props.refresh(shops.activeShopId)
}
onMounted(reload)
watch(() => shops.activeShopId, reload)

function openCreate() {
  mode.value = 'create'
  editingId.value = null
  name.value = ''
  dialog.value = true
}

function openEdit(id: string, n: string) {
  mode.value = 'edit'
  editingId.value = id
  name.value = n
  dialog.value = true
}

async function save() {
  const v = name.value.trim()
  if (!v || !shops.activeShopId) return
  if (mode.value === 'create') {
    await props.create(shops.activeShopId, v)
  } else if (editingId.value) {
    await props.update(shops.activeShopId, editingId.value, v)
  }
  dialog.value = false
}

function askDelete(id: string) {
  confirm.require({
    message: t('properties.delete_confirm'),
    accept: async () => {
      if (shops.activeShopId) await props.remove(shops.activeShopId, id)
    }
  })
}
</script>

<template>
  <PageHeader :title="t('properties.title')" :subtitle="t('properties.hint')">
    <template #actions>
      <Button size="sm" @click="openCreate">
        <Plus class="size-4" />
        {{ t('properties.new') }}
      </Button>
    </template>
  </PageHeader>

  <EmptyState
    v-if="props.items.length === 0"
    icon="sliders"
    :hint="t('properties.no_properties')"
  >
    <Button class="mt-3" @click="openCreate">
      <Plus class="size-4" />
      {{ t('properties.new') }}
    </Button>
  </EmptyState>

  <div v-else>
    <div v-for="p in props.items" :key="p.productPropertyId" class="list-card">
      <div class="avatar"><SlidersHorizontal class="size-5" /></div>
      <div class="list-card__body">
        <div class="list-card__title">
          {{ p.name }}
          <Badge v-if="p._local" variant="secondary">آفلاین</Badge>
        </div>
      </div>
      <Button variant="ghost" size="icon" @click="openEdit(p.productPropertyId, p.name)">
        <Pencil class="size-4" />
      </Button>
      <Button variant="ghost" size="icon" @click="askDelete(p.productPropertyId)">
        <Trash2 class="size-4 text-destructive" />
      </Button>
    </div>
  </div>

  <Dialog v-model:open="dialog">
    <DialogContent class="sm:max-w-[420px]">
      <DialogHeader>
        <DialogTitle>{{ t('properties.new') }}</DialogTitle>
      </DialogHeader>
      <Label for="prop-name">{{ t('properties.name_label') }}</Label>
      <Input id="prop-name" v-model="name" class="mt-1" autofocus />
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
.mt-3 {
  margin-top: 0.75rem;
}
</style>
