# Bazaar.Web

Offline-first **Vue 3 + PrimeVue** PWA for the Bazaar accounting backend. Persian (Farsi) UI in **RTL**, written in TypeScript.

**Distribution targets:**

- **Web / iOS** — installable PWA. iOS users add to home screen from Safari; no App Store / Capacitor build is shipped for iOS.
- **Android** — Capacitor-wrapped native app (and/or TWA via Bubblewrap as a lighter alternative).
- **Desktop** — deferred. Wrap `dist/` with Tauri or Electron later without any source changes.

## Stack

- Vue 3 + TypeScript + Vite
- PrimeVue 4 (Aura theme) + PrimeIcons
- Pinia + Vue Router
- vue-i18n (Persian, RTL)
- vite-plugin-pwa (Workbox) — install + background sync
- Dexie 4 (IndexedDB) — offline-first cache + outbox queue
- Axios — auth header + transparent 401/refresh interceptor
- Capacitor 6 — **Android-only** native wrapper (iOS is intentionally PWA-only)

## Prerequisites

- Node 20+ (tested on 22)
- The backend running locally:
  - `Bazaar.Api` on `http://localhost:5108` (Swagger at `/swagger`)
  - `Bazaar.Identity` on `http://localhost:5209`

## Getting started

```bash
cd src/Bazaar.Web
cp .env.example .env   # already exists; tweak if needed
npm install
npm run dev
```

The Vite dev server runs on `http://localhost:5173` and proxies:

| Path           | Target                          |
| -------------- | ------------------------------- |
| `/api/*`       | `VITE_API_URL` (Bazaar.Api)     |
| `/swagger/*`   | `VITE_API_URL`                  |
| `/connect/*`   | `VITE_IDENTITY_URL` (Identity)  |
| `/.well-known` | `VITE_IDENTITY_URL`             |

So CORS is **not** required during dev. For non-dev origins (production, TWA, Capacitor), you must enable CORS on the backend — see [Backend changes required](#backend-changes-required).

## NPM scripts

| Script             | What it does                                                       |
| ------------------ | ------------------------------------------------------------------ |
| `npm run dev`      | Vite dev server with HMR on 5173                                   |
| `npm run build`    | Type-check + production build into `dist/`                         |
| `npm run preview`  | Preview the production build locally                               |
| `npm run lint`     | ESLint over `src/`                                                 |
| `npm run format`   | Prettier write                                                     |
| `npm run gen:api`  | Regenerate `src/types/api.d.ts` from `/swagger/v1/swagger.json`    |
| `npm run gen:icons`| Re-render PWA icons from `public/icons/icon.svg`                   |
| `npm run cap:sync` | Copy the built `dist/` into the Android Capacitor project          |
| `npm run cap:android` | Run the Capacitor Android app on a connected device / emulator  |

## OTP login flow

1. `POST /api/Otp/request` with `{ phoneNumber }` triggers an OTP. In development the backend **logs the code** instead of sending SMS — watch the `Bazaar.Identity` console. Set `VITE_DEV_OTP_HINT=true` to display a hint on the verify screen.
2. `POST /connect/token` with `client_id=bazaar_mobile`, `grant_type=otp_verification`, `phone`, `otp_code`, `scope=openid profile phone bazaar_api offline_access` exchanges the OTP for an access token (24 h) + refresh token. Tokens are stored:
   - **Browser:** `localStorage` under `bazaar.auth.v1`.
   - **Capacitor (iOS/Android):** `@capacitor/preferences` (encrypted on iOS Keychain / Android Keystore where the platform exposes it).
3. The Axios interceptor adds `Authorization: Bearer …` to every API call. On `401`, it transparently calls `/connect/token` with the refresh token and retries the request once.

## Offline-first architecture

- All reads go through `src/db/repositories/*.ts`, which return cached IndexedDB rows immediately and fire a background `pullRemote()` to refresh.
- All writes are **optimistic**: they update IndexedDB synchronously, push a typed entry into the `outbox` table (`src/db/db.ts → outbox`), and return.
- `src/sync/syncEngine.ts` drains the outbox:
  - On app start (`startBackgroundSync()` in `App.vue`).
  - On every `window.online` event.
  - Every 30 s while the app is open.
- The Workbox service worker (`vite.config.ts → VitePWA.workbox`) layers a second line of defense:
  - **GET `/api/*`** → `StaleWhileRevalidate`, 7-day TTL.
  - **POST/PUT/DELETE** → registered in a `BackgroundSync` queue with 24 h retention so the OS retries after the browser is closed.
- Conflict policy: server response overrides Dexie row on success. On 4xx the entry is marked `failed` and stays visible in the outbox table for manual resolution. On 5xx it stays `pending`.

The "Pending sync" pill in the app header shows the live outbox count.

## iOS — PWA only

iOS users install the app **from Safari** (Share → Add to Home Screen). The manifest is configured for this path:

- `display: standalone`, `theme_color`, `background_color`, and `apple-touch-icon-180x180.png`.
- `<meta name="apple-mobile-web-app-capable" content="yes">` in [`index.html`](index.html).
- The Workbox service worker keeps the app fully offline-capable on iOS Safari.

No Xcode, no Capacitor iOS project, no App Store review — the PWA is the supported iOS deliverable.

> If you ever decide to ship to the App Store, you'd just run `npx cap add ios` (macOS + Xcode 16+ required). The token storage layer already handles iOS Capacitor transparently via `@capacitor/preferences`.

## Android — Capacitor (recommended) or TWA

### Option A: Capacitor (default)

`capacitor.config.ts` is ready (`appId: com.bazaar.app`, `webDir: dist`). The `android/` folder is not committed; add it on demand:

```bash
npm run build
npx cap add android
npm run cap:sync        # copy dist/ into the android/ project
npm run cap:android     # build + run on a connected device / emulator
```

For Play Store releases:

- Create a keystore: `keytool -genkey -v -keystore release.keystore -alias bazaar -keyalg RSA -keysize 2048 -validity 10000`
- Configure release signing in `android/app/build.gradle`.
- `cd android && ./gradlew bundleRelease` produces an `.aab` for the Play Console.

### Option B: TWA via Bubblewrap (no native code)

If you'd rather avoid maintaining an Android project, [`@bubblewrap/cli`](https://github.com/GoogleChromeLabs/bubblewrap) wraps the deployed PWA as a Trusted Web Activity — the build output already meets the [TWA Quality Criteria](https://developer.chrome.com/docs/android/trusted-web-activity/quality-criteria/) (valid manifest, installable, `display: standalone`, HTTPS-served).

```bash
npm install -g @bubblewrap/cli
bubblewrap init --manifest=https://your-domain.com/manifest.webmanifest
bubblewrap build       # produces an .aab for Play Store
```

TWA pros: no Java code to maintain. Cons: requires you to host the PWA on HTTPS first and to ship an `assetlinks.json` from that origin to validate ownership.

## Desktop wrapper (later)

The PWA is wrapper-agnostic. To ship as a desktop app later, point Tauri or Electron at the `dist/` folder — no source code changes needed.

- **Tauri 2** (recommended, ~10 MB binary):
  ```bash
  npm install -D @tauri-apps/cli
  npx tauri init   # set distDir = "../dist", devUrl = "http://localhost:5173"
  npx tauri build
  ```
- **Electron** (heavier, ~150 MB, more ecosystem):
  ```bash
  npm install -D electron electron-builder
  # add a main.js that loads dist/index.html
  ```

## Folder map

```
src/
├── api/            Axios client + per-entity endpoint wrappers
├── auth/           Token storage, JWT decode, Pinia auth store
├── db/             Dexie schema + repositories + outbox
├── sync/           Outbox drainer + online listener
├── stores/         Pinia stores (one per entity)
├── pages/          Route components (auth, shops, products, factors, settings)
├── layouts/        AppLayout (with bottom nav) + AuthLayout
├── components/     EmptyState, PageHeader, …
├── composables/    useOnline, usePendingSync
├── i18n/           vue-i18n + Persian locale + Jalali date helpers
├── router/         routes + auth/shop guards
├── styles/         app.scss + primevue.scss (RTL overrides)
├── types/          Auto-generated OpenAPI types
├── App.vue
└── main.ts         PrimeVue + Pinia + Router + i18n wiring
```

## Backend changes required

Two issues in the existing `.NET` backend that prevent the frontend from running outside the Vite dev proxy:

### 1. CORS

Neither `Bazaar.Api` nor `Bazaar.Identity` configure CORS. Add to **both** `Program.cs` files (replace `*` with your allowed origins in production):

```csharp
builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(p => p
        .WithOrigins("http://localhost:5173", "capacitor://localhost", "https://localhost")
        .AllowAnyHeader()
        .AllowAnyMethod());
});
// later:
app.UseCors();
```

### 2. `Identity:Url` mismatch

`src/Bazaar.Api/appsettings.json` currently sets:

```json
"Identity": { "Url": "https://localhost:5001" }
```

but `Bazaar.Identity` actually runs on `https://localhost:7012` (or `http://localhost:5209`). Either change `Identity:Url` to the real Identity origin, or change Identity's launch profile. Without this, `Bazaar.Api` cannot fetch the JWKS to validate tokens.

### 3. Optional: list endpoint for product properties

The Settings page benefits from a `GET /api/shops/{shopId}/product-properties` endpoint to render the master list. Today, property names are only returned embedded inside `ProductDetailModel.Properties`. The frontend gracefully falls back to local cache, but a dedicated endpoint would close the gap.

## License

Internal project — not yet published.
