<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useShopStore } from '@/stores/shops'
import { useCategoryStore } from '@/stores/categories'
import { useConfirm } from 'primevue/useconfirm'

const { t } = useI18n()
const shops = useShopStore()
const cats = useCategoryStore()
const confirm = useConfirm()

const dialog = ref(false)
const mode = ref<'create' | 'edit'>('create')
const editingId = ref<string | null>(null)
const name = ref('')

async function reload() {
  if (!shops.activeShopId) return
  await cats.loadLocal(shops.activeShopId)
  void cats.refresh(shops.activeShopId)
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
    await cats.create(shops.activeShopId, v)
  } else if (editingId.value) {
    await cats.update(shops.activeShopId, editingId.value, v)
  }
  dialog.value = false
}

function askDelete(id: string) {
  confirm.require({
    message: t('categories.delete_confirm'),
    acceptClass: 'p-button-danger',
    accept: async () => {
      if (shops.activeShopId) await cats.remove(shops.activeShopId, id)
    }
  })
}
</script>

<template>
  <PageHeader :title="t('categories.title')">
    <template #actions>
      <Button :label="t('categories.new')" icon="pi pi-plus" size="small" @click="openCreate" />
    </template>
  </PageHeader>

  <ProgressBar v-if="cats.loading" mode="indeterminate" style="height: 3px" />

  <EmptyState
    v-if="!cats.loading && cats.items.length === 0"
    icon="pi-tags"
    :hint="t('categories.no_categories')"
  >
    <Button :label="t('categories.new')" icon="pi pi-plus" @click="openCreate" class="mt-3" />
  </EmptyState>

  <div v-else>
    <div v-for="c in cats.items" :key="c.categoryId" class="list-card">
      <div class="avatar"><i class="pi pi-tag" /></div>
      <div class="list-card__body">
        <div class="list-card__title">
          {{ c.name }}
          <Tag v-if="c._local" value="آفلاین" severity="info" />
        </div>
      </div>
      <Button icon="pi pi-pencil" text rounded @click="openEdit(c.categoryId, c.name)" />
      <Button
        icon="pi pi-trash"
        text
        rounded
        severity="danger"
        @click="askDelete(c.categoryId)"
      />
    </div>
  </div>

  <Dialog v-model:visible="dialog" :header="t('categories.new')" modal style="width: min(420px, 92vw)">
    <label class="field">
      <span class="label">{{ t('categories.name_label') }}</span>
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
