<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { useShopStore } from '@/stores/shops'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'

const { t } = useI18n()
const shops = useShopStore()
const router = useRouter()
const toast = useToast()
const confirm = useConfirm()

const dialog = ref(false)
const dialogMode = ref<'create' | 'edit'>('create')
const editingId = ref<string | null>(null)
const titleField = ref('')

onMounted(async () => {
  await shops.loadLocal()
  void shops.refresh()
})

function openCreate() {
  dialogMode.value = 'create'
  editingId.value = null
  titleField.value = ''
  dialog.value = true
}

function openEdit(shopId: string, title: string) {
  dialogMode.value = 'edit'
  editingId.value = shopId
  titleField.value = title
  dialog.value = true
}

async function save() {
  const title = titleField.value.trim()
  if (!title) {
    toast.add({ severity: 'warn', summary: t('shops.name_required'), life: 2500 })
    return
  }
  if (dialogMode.value === 'create') {
    await shops.create(title)
  } else if (editingId.value) {
    await shops.update(editingId.value, title)
  }
  dialog.value = false
}

function pick(shopId: string) {
  shops.setActive(shopId)
  router.push({ name: 'dashboard' })
}

function askDelete(shopId: string) {
  confirm.require({
    message: t('shops.delete_confirm'),
    acceptClass: 'p-button-danger',
    accept: async () => {
      await shops.remove(shopId)
    }
  })
}
</script>

<template>
  <PageHeader :title="t('shops.title')">
    <template #actions>
      <Button :label="t('shops.new')" icon="pi pi-plus" size="small" @click="openCreate" />
    </template>
  </PageHeader>

  <ProgressBar v-if="shops.loading" mode="indeterminate" style="height: 3px" />

  <EmptyState
    v-if="!shops.loading && shops.items.length === 0"
    icon="pi-shop"
    :hint="t('shops.no_shops')"
  >
    <Button :label="t('shops.new')" icon="pi pi-plus" @click="openCreate" class="mt-3" />
  </EmptyState>

  <div v-else>
    <div v-for="s in shops.items" :key="s.shopId" class="list-card" @click="pick(s.shopId)">
      <div class="avatar"><i class="pi pi-shop" /></div>
      <div class="list-card__body">
        <h3 class="list-card__title">
          {{ s.title }}
          <Tag v-if="s._local" :value="t('app.saved_locally')" severity="info" />
        </h3>
        <div class="list-card__sub">{{ new Date(s.createdOn).toLocaleDateString('fa-IR') }}</div>
      </div>
      <Button icon="pi pi-pencil" text rounded @click.stop="openEdit(s.shopId, s.title)" />
      <Button
        icon="pi pi-trash"
        text
        rounded
        severity="danger"
        @click.stop="askDelete(s.shopId)"
      />
    </div>
  </div>

  <Dialog
    v-model:visible="dialog"
    :header="dialogMode === 'create' ? t('shops.create_title') : t('app.edit')"
    modal
    style="width: min(420px, 92vw)"
  >
    <div class="dialog-body">
      <label class="field">
        <span class="label">{{ t('shops.name_label') }}</span>
        <InputText v-model="titleField" autofocus fluid />
      </label>
    </div>
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
  font-size: 1.1rem;
}
.dialog-body {
  padding: 0.5rem 0;
}
.field {
  display: block;
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
