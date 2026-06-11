<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { useShopStore } from '@/stores/shops'
import { toast } from 'vue-sonner'
import { useConfirm } from '@/composables/useConfirm'
import { Check, Pencil, Plus, Store, Trash2 } from '@lucide/vue'
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
const shops = useShopStore()
const router = useRouter()
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
    toast.warning(t('shops.name_required'))
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
    accept: async () => {
      await shops.remove(shopId)
    }
  })
}
</script>

<template>
  <PageHeader :title="t('shops.title')">
    <template #actions>
      <Button size="sm" @click="openCreate">
        <Plus class="size-4" />
        {{ t('shops.new') }}
      </Button>
    </template>
  </PageHeader>

  <Progress v-if="shops.loading" class="h-1 animate-pulse" :model-value="undefined" />

  <EmptyState
    v-if="!shops.loading && shops.items.length === 0"
    icon="store"
    :hint="t('shops.no_shops')"
  >
    <Button class="mt-3" @click="openCreate">
      <Plus class="size-4" />
      {{ t('shops.new') }}
    </Button>
  </EmptyState>

  <div v-else>
    <div v-for="s in shops.items" :key="s.shopId" class="list-card" @click="pick(s.shopId)">
      <div class="avatar"><Store class="size-5" /></div>
      <div class="list-card__body">
        <h3 class="list-card__title">
          {{ s.title }}
          <Badge v-if="s._local" variant="secondary">{{ t('app.saved_locally') }}</Badge>
        </h3>
        <div class="list-card__sub">{{ new Date(s.createdOn).toLocaleDateString('fa-IR') }}</div>
      </div>
      <Button variant="ghost" size="icon" @click.stop="openEdit(s.shopId, s.title)">
        <Pencil class="size-4" />
      </Button>
      <Button variant="ghost" size="icon" @click.stop="askDelete(s.shopId)">
        <Trash2 class="size-4 text-destructive" />
      </Button>
    </div>
  </div>

  <Dialog v-model:open="dialog">
    <DialogContent class="sm:max-w-[420px]">
      <DialogHeader>
        <DialogTitle>{{
          dialogMode === 'create' ? t('shops.create_title') : t('app.edit')
        }}</DialogTitle>
      </DialogHeader>
      <div class="dialog-body">
        <Label for="shop-title">{{ t('shops.name_label') }}</Label>
        <Input id="shop-title" v-model="titleField" class="mt-1" autofocus />
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
.dialog-body {
  padding: 0.5rem 0;
}
.mt-3 {
  margin-top: 0.75rem;
}
</style>
