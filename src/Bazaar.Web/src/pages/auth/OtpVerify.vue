<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/auth/auth.store'
import { toast } from 'vue-sonner'
import { Check, Loader2, RefreshCw } from '@lucide/vue'
import { Button } from '@/components/ui/button'
import { InputOTP, InputOTPGroup, InputOTPSlot } from '@/components/ui/input-otp'

const { t } = useI18n()
const auth = useAuthStore()
const router = useRouter()

const code = ref('')
const loading = ref(false)
const resending = ref(false)
const cooldown = ref(60)
let timer: number | null = null

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
    toast.success(t('auth.logged_in'))
    router.replace({ path: '/' })
  } catch (e: any) {
    toast.error(e?.response?.data?.error_description ?? e?.message ?? t('auth.otp_invalid'))
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
    toast.error(e?.message ?? t('app.error'))
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
      <InputOTP v-model="code" :maxlength="4">
        <InputOTPGroup>
          <InputOTPSlot :index="0" />
          <InputOTPSlot :index="1" />
          <InputOTPSlot :index="2" />
          <InputOTPSlot :index="3" />
        </InputOTPGroup>
      </InputOTP>
    </div>

    <p class="muted otp-hint">کد تأیید: 1111</p>

    <Button type="submit" :disabled="loading || code.length < 4" class="submit w-full">
      <Loader2 v-if="loading" class="size-4 animate-spin" />
      <Check v-else class="size-4" />
      {{ t('auth.otp_submit') }}
    </Button>

    <div class="resend">
      <Button
        v-if="cooldown <= 0"
        type="button"
        variant="ghost"
        :disabled="resending"
        @click="resend"
      >
        <Loader2 v-if="resending" class="size-4 animate-spin" />
        <RefreshCw v-else class="size-4" />
        {{ t('auth.otp_resend') }}
      </Button>
      <span v-else class="muted">{{ t('auth.otp_resend_in', { seconds: cooldown }) }}</span>
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
  margin-bottom: 0.75rem;
}
.otp-hint {
  text-align: center;
  font-size: 0.85rem;
  margin: 0 0 1.25rem;
}
.submit {
  width: 100%;
}
.resend {
  text-align: center;
  margin-top: 1rem;
  font-size: 0.9rem;
}
</style>
