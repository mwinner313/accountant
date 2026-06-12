<script setup lang="ts">
import { useConfirm } from '@/composables/useConfirm'
import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle
} from '@/components/ui/alert-dialog'
import { Button } from '@/components/ui/button'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const { open, options, accept, cancel } = useConfirm()

function onOpenChange(value: boolean) {
  // Escape key dismisses the dialog without clicking our buttons.
  if (!value && open.value && options.value) cancel()
}
</script>

<template>
  <AlertDialog :open="open" @update:open="onOpenChange">
    <AlertDialogContent>
      <AlertDialogHeader>
        <AlertDialogTitle>{{ t('app.confirm') }}</AlertDialogTitle>
        <AlertDialogDescription>{{ options?.message }}</AlertDialogDescription>
      </AlertDialogHeader>
      <AlertDialogFooter>
        <Button type="button" variant="outline" @click="cancel">{{ t('app.cancel') }}</Button>
        <Button type="button" @click="accept">{{ t('app.confirm') }}</Button>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>
</template>
