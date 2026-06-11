<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useShopStore } from '@/stores/shops'
import { useProductPropertyStore } from '@/stores/productProperties'
import { useConfirm } from 'primevue/useconfirm'

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
    acceptClass: 'p-button-danger',
    accept: async () => {
      if (shops.activeShopId) await props.remove(shops.activeShopId, id)
    }
  })
}
</script>

<template>
  <PageHeader :title="t('properties.title')" :subtitle="t('properties.hint')">
    <template #actions>
      <Button :label="t('properties.new')" icon="pi pi-plus" size="small" @click="openCreate" />
    </template>
  </PageHeader>

  <EmptyState
    v-if="props.items.length === 0"
    icon="pi-sliders-h"
    :hint="t('properties.no_properties')"
  >
    <Button :label="t('properties.new')" icon="pi pi-plus" @click="openCreate" class="mt-3" />
  </EmptyState>

  <div v-else>
    <div v-for="p in props.items" :key="p.productPropertyId" class="list-card">
      <div class="avatar"><i class="pi pi-sliders-h" /></div>
      <div class="list-card__body">
        <div class="list-card__title">
          {{ p.name }}
          <Tag v-if="p._local" value="آفلاین" severity="info" />
        </div>
      </div>
      <Button icon="pi pi-pencil" text rounded @click="openEdit(p.productPropertyId, p.name)" />
      <Button
        icon="pi pi-trash"
        text
        rounded
        severity="danger"
        @click="askDelete(p.productPropertyId)"
      />
    </div>
  </div>

  <Dialog v-model:visible="dialog" :header="t('properties.new')" modal style="width: min(420px, 92vw)">
    <label class="field">
      <span class="label">{{ t('properties.name_label') }}</span>
      <InputText v-model="name" autofocus fluid />
    </label>
    <template #footer>
      <Button :label="t('app.cancel')" text @click="dialog = false" />
      <Button :label="t('app.save')" icon="pi pi-check" @click="save" />
    </template>
  </Dialog>
</template>

<style scoped lang="scss">
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
.field {
  display: block;
  margin-top: 0.5rem;
  .label {
    display: block;
    margin-bottom: 0.4rem;
    font-weight: 600;
  }
}
.mt-3 {
  margin-top: 0.75rem;
}
</style>
