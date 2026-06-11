import { createI18n } from 'vue-i18n'
import fa from './locales/fa.json'

export type AppLocale = 'fa-IR'

export const i18n = createI18n({
  legacy: false,
  globalInjection: true,
  locale: 'fa-IR',
  fallbackLocale: 'fa-IR',
  messages: {
    'fa-IR': fa
  },
  numberFormats: {
    'fa-IR': {
      currency: { style: 'currency', currency: 'IRR', maximumFractionDigits: 0 },
      decimal: { style: 'decimal', maximumFractionDigits: 4 },
      integer: { style: 'decimal', maximumFractionDigits: 0 }
    }
  },
  datetimeFormats: {
    'fa-IR': {
      short: { year: 'numeric', month: '2-digit', day: '2-digit' },
      long: { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' }
    }
  }
})
