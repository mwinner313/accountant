<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/auth/auth.store'
import { toast } from 'vue-sonner'
import { Send, Loader2 } from '@lucide/vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

const { t } = useI18n()
const auth = useAuthStore()
const router = useRouter()

const phone = ref(auth.phone ?? '')
const loading = ref(false)
const errorMsg = ref<string | null>(null)

function normalizePhone(input: string): string {
  return input.replace(/[\u06f0-\u06f9]/g, d => String(d.charCodeAt(0) - 0x06f0)).trim()
}

function valid(value: string): boolean {
  return /^\+?\d{10,15}$/.test(value)
}

async function submit() {
  errorMsg.value = null
  const normalized = normalizePhone(phone.value)
  if (!valid(normalized)) {
    errorMsg.value = t('auth.phone_invalid')
    return
  }
  loading.value = true
  try {
    await auth.requestOtp(normalized)
    router.push({ name: 'otp-verify' })
  } catch (e: any) {
    toast.error(e?.response?.data ?? e?.message ?? t('app.error'))
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <form @submit.prevent="submit">
    <h2 class="welcome">{{ t('auth.welcome') }}</h2>
    <p class="muted prompt">{{ t('auth.phone_prompt') }}</p>

    <div class="field">
      <Label for="phone">{{ t('auth.phone_label') }}</Label>
      <Input
        id="phone"
        v-model="phone"
        :placeholder="t('auth.phone_placeholder')"
        autocomplete="tel"
        inputmode="tel"
        dir="ltr"
        :aria-invalid="!!errorMsg"
        class="mt-1"
      />
      <small v-if="errorMsg" class="error">{{ errorMsg }}</small>
    </div>

    <Button type="submit" :disabled="loading" class="submit w-full">
      <Loader2 v-if="loading" class="size-4 animate-spin" />
      <Send v-else class="size-4" />
      {{ t('auth.phone_send_otp') }}
    </Button>
  </form>
</template>

<style scoped lang="scss">
.welcome {
  font-size: 1.25rem;
  font-weight: 700;
  margin: 0 0 0.5rem;
}
.prompt {
  margin: 0 0 1.5rem;
}
.field {
  display: block;
  margin-bottom: 1.25rem;

  .error {
    color: var(--destructive);
    font-size: 0.8rem;
    margin-top: 0.25rem;
    display: block;
  }
}
.submit {
  width: 100%;
}
</style>
