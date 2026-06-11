<script setup lang="ts">
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
</script>

<template>
  <div class="auth-shell">
    <div class="auth-card">
      <div class="auth-brand">
        <div class="auth-logo">
          <i class="pi pi-shop" />
        </div>
        <h1>{{ t('app.name') }}</h1>
        <p class="muted">{{ t('app.tagline') }}</p>
      </div>
      <router-view />
    </div>
  </div>
</template>

<style scoped lang="scss">
.auth-shell {
  min-height: 100vh;
  min-height: 100dvh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background:
    radial-gradient(circle at top left, rgba(99, 102, 241, 0.18), transparent 40%),
    radial-gradient(circle at bottom right, rgba(16, 185, 129, 0.12), transparent 45%),
    var(--p-surface-50, #f8fafc);
}
.auth-card {
  width: 100%;
  max-width: 420px;
  background: var(--p-content-background);
  border: 1px solid var(--p-content-border-color);
  border-radius: 18px;
  padding: 2rem 1.5rem;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.08);
}
.auth-brand {
  text-align: center;
  margin-bottom: 1.5rem;

  .auth-logo {
    display: inline-flex;
    width: 64px;
    height: 64px;
    border-radius: 16px;
    align-items: center;
    justify-content: center;
    background: var(--p-primary-color);
    color: var(--p-primary-contrast-color);
    font-size: 1.75rem;
    margin-bottom: 0.75rem;
  }
  h1 {
    margin: 0;
    font-size: 1.5rem;
    font-weight: 800;
  }
  p {
    margin: 0.25rem 0 0;
  }
}
</style>
