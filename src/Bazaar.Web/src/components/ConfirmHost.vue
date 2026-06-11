<script setup lang="ts">
import { useConfirm } from '@/composables/useConfirm'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle
} from '@/components/ui/alert-dialog'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const { open, options, accept, cancel } = useConfirm()
</script>

<template>
  <AlertDialog :open="open" @update:open="v => !v && cancel()">
    <AlertDialogContent>
      <AlertDialogHeader>
        <AlertDialogTitle>{{ t('app.confirm') }}</AlertDialogTitle>
        <AlertDialogDescription>{{ options?.message }}</AlertDialogDescription>
      </AlertDialogHeader>
      <AlertDialogFooter>
        <AlertDialogCancel @click="cancel">{{ t('app.cancel') }}</AlertDialogCancel>
        <AlertDialogAction @click="accept">{{ t('app.confirm') }}</AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>
</template>
