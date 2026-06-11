import dayjs from 'dayjs'
import jalaliday from 'jalaliday'

dayjs.extend(jalaliday)

const FA_LOCALE = 'fa-IR'

export function toFaDigits(value: number | string | null | undefined): string {
  if (value === null || value === undefined || value === '') return ''
  return String(value).replace(/[0-9]/g, d => String.fromCharCode(0x06f0 + Number(d)))
}

export function toEnDigits(value: string | null | undefined): string {
  if (!value) return ''
  return value
    .replace(/[\u06f0-\u06f9]/g, d => String(d.charCodeAt(0) - 0x06f0))
    .replace(/[\u0660-\u0669]/g, d => String(d.charCodeAt(0) - 0x0660))
}

const moneyFmt = new Intl.NumberFormat(FA_LOCALE, { maximumFractionDigits: 0 })
const decimalFmt = new Intl.NumberFormat(FA_LOCALE, { maximumFractionDigits: 4 })

export function formatMoney(value: number | string | null | undefined): string {
  if (value === null || value === undefined || value === '') return ''
  const n = typeof value === 'string' ? Number(value) : value
  if (Number.isNaN(n)) return ''
  return moneyFmt.format(n)
}

export function formatDecimal(value: number | string | null | undefined): string {
  if (value === null || value === undefined || value === '') return ''
  const n = typeof value === 'string' ? Number(value) : value
  if (Number.isNaN(n)) return ''
  return decimalFmt.format(n)
}

export function toJalali(date: string | Date | null | undefined, withTime = false): string {
  if (!date) return ''
  // jalaliday extends dayjs with a "jalali" calendar.
  // Cast required because the type defs for jalaliday don't expose the calendar option.
  const d = (dayjs as unknown as { (input?: dayjs.ConfigType, opts?: { jalali?: boolean }): dayjs.Dayjs })(
    date as dayjs.ConfigType,
    { jalali: false }
  )
  const fmt = withTime ? 'YYYY/MM/DD HH:mm' : 'YYYY/MM/DD'
  return toFaDigits((d as any).calendar('jalali').locale('fa').format(fmt))
}

export function fromJalaliInput(faInput: string): Date | null {
  if (!faInput) return null
  const cleaned = toEnDigits(faInput).replace(/[^0-9/\-:\s]/g, '').trim()
  if (!cleaned) return null
  const parsed = (dayjs as any)(cleaned, { jalali: true })
  if (!parsed.isValid()) return null
  return parsed.calendar('gregory').toDate()
}
