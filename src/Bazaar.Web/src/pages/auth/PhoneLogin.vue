<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/auth/auth.store'
import { useShopStore } from '@/stores/shops'
import { useToast } from 'primevue/usetoast'
import { isDevBypassAvailable } from '@/auth/devBypass'

const { t } = useI18n()
const auth = useAuthStore()
const shops = useShopStore()
const router = useRouter()
const toast = useToast()

const phone = ref(auth.phone ?? '')
const loading = ref(false)
const bypassLoading = ref(false)
const errorMsg = ref<string | null>(null)

const devBypassAvailable = isDevBypassAvailable()

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
    toast.add({
      severity: 'error',
      summary: t('app.error'),
      detail: e?.response?.data ?? e?.message,
      life: 4000
    })
  } finally {
    loading.value = false
  }
}

async function devBypass() {
  bypassLoading.value = true
  try {
    const { activeShopId } = await auth.devLogin()
    await shops.loadLocal()
    shops.setActive(activeShopId)
    router.replace({ name: 'dashboard' })
  } catch (e: any) {
    toast.add({ severity: 'error', summary: t('app.error'), detail: e?.message, life: 4000 })
  } finally {
    bypassLoading.value = false
  }
}
</script>

<template>
  <form @submit.prevent="submit">
    <h2 class="welcome">{{ t('auth.welcome') }}</h2>
    <p class="muted prompt">{{ t('auth.phone_prompt') }}</p>

    <label class="field">
      <span class="label">{{ t('auth.phone_label') }}</span>
      <InputText
        v-model="phone"
        :placeholder="t('auth.phone_placeholder')"
        autocomplete="tel"
        inputmode="tel"
        dir="ltr"
        :invalid="!!errorMsg"
        fluid
      />
      <small v-if="errorMsg" class="error">{{ errorMsg }}</small>
    </label>

    <Button
      type="submit"
      :label="t('auth.phone_send_otp')"
      icon="pi pi-send"
      :loading="loading"
      class="submit"
      fluid
    />

    <div v-if="devBypassAvailable" class="dev-bypass">
      <Divider />
      <Button
        type="button"
        :label="t('auth.dev_bypass')"
        icon="pi pi-bolt"
        severity="secondary"
        outlined
        :loading="bypassLoading"
        fluid
        @click="devBypass"
      />
      <small class="muted hint">{{ t('auth.dev_bypass_hint') }}</small>
    </div>
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

  .label {
    display: block;
    font-size: 0.85rem;
    margin-bottom: 0.4rem;
    font-weight: 600;
  }
  .error {
    color: var(--p-red-500, #ef4444);
    font-size: 0.8rem;
    margin-top: 0.25rem;
    display: block;
  }
}
.submit {
  width: 100%;
}
.dev-bypass {
  margin-top: 1rem;
  .hint {
    display: block;
    text-align: center;
    margin-top: 0.5rem;
    font-size: 0.8rem;
  }
}
</style>
