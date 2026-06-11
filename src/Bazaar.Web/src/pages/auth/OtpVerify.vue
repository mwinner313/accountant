<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/auth/auth.store'
import { useToast } from 'primevue/usetoast'

const { t } = useI18n()
const auth = useAuthStore()
const router = useRouter()
const toast = useToast()

const code = ref('')
const loading = ref(false)
const resending = ref(false)
const cooldown = ref(60)
let timer: number | null = null

const devHint = import.meta.env.VITE_DEV_OTP_HINT === 'true'

function startCooldown() {
  cooldown.value = 60
  if (timer !== null) window.clearInterval(timer)
  timer = window.setInterval(() => {
    cooldown.value--
    if (cooldown.value <= 0 && timer !== null) {
      window.clearInterval(timer)
      timer = null
    }
  }, 1000)
}

onMounted(() => {
  if (!auth.phone) {
    router.replace({ name: 'phone-login' })
    return
  }
  startCooldown()
})
onUnmounted(() => {
  if (timer !== null) window.clearInterval(timer)
})

async function submit() {
  if (!code.value || code.value.length < 4) return
  loading.value = true
  try {
    await auth.verifyOtp(code.value)
    toast.add({ severity: 'success', summary: t('auth.logged_in'), life: 2000 })
    router.replace({ path: '/' })
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: t('auth.otp_invalid'),
      detail: e?.response?.data?.error_description ?? e?.message,
      life: 4000
    })
  } finally {
    loading.value = false
  }
}

async function resend() {
  if (cooldown.value > 0 || !auth.phone) return
  resending.value = true
  try {
    await auth.requestOtp(auth.phone)
    startCooldown()
  } catch (e: any) {
    toast.add({ severity: 'error', summary: t('app.error'), detail: e?.message, life: 4000 })
  } finally {
    resending.value = false
  }
}
</script>

<template>
  <form @submit.prevent="submit">
    <h2 class="welcome">{{ t('auth.otp_prompt') }}</h2>
    <p class="muted prompt" dir="ltr">{{ auth.phone }}</p>

    <div class="otp-wrap" dir="ltr">
      <InputOtp v-model="code" :length="4" integer-only />
    </div>

    <Button
      type="submit"
      :label="t('auth.otp_submit')"
      icon="pi pi-check"
      :loading="loading"
      :disabled="code.length < 4"
      class="submit"
      fluid
    />

    <div class="resend">
      <Button
        v-if="cooldown <= 0"
        :label="t('auth.otp_resend')"
        icon="pi pi-refresh"
        severity="secondary"
        text
        :loading="resending"
        @click="resend"
      />
      <span v-else class="muted">{{ t('auth.otp_resend_in', { seconds: cooldown }) }}</span>
    </div>

    <div v-if="devHint" class="dev-hint">
      <i class="pi pi-info-circle" /> {{ t('auth.otp_dev_hint') }}
    </div>
  </form>
</template>

<style scoped lang="scss">
.welcome {
  font-size: 1.15rem;
  font-weight: 700;
  margin: 0 0 0.25rem;
}
.prompt {
  margin: 0 0 1.5rem;
  font-weight: 600;
}
.otp-wrap {
  display: flex;
  justify-content: center;
  margin-bottom: 1.5rem;
}
.submit {
  width: 100%;
}
.resend {
  text-align: center;
  margin-top: 1rem;
  font-size: 0.9rem;
}
.dev-hint {
  margin-top: 1rem;
  padding: 0.75rem;
  border-radius: 8px;
  background: var(--p-blue-50, #eff6ff);
  color: var(--p-blue-700, #1d4ed8);
  font-size: 0.85rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
</style>
