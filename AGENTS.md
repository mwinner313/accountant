# AGENTS.md

## Cursor Cloud specific instructions

### Overview of services

Bazaar is a shop-accounting product with a .NET 9 backend and a Vue 3 PWA frontend.

| Service | Path | Dev run command | URL |
| --- | --- | --- | --- |
| `Bazaar.Identity` | `src/Bazaar.Identity` | `ASPNETCORE_ENVIRONMENT=Development dotnet run --launch-profile http` | http://localhost:5209 (Swagger at `/swagger`, OIDC discovery at `/.well-known/openid-configuration`) |
| `Bazaar.Api` | `src/Bazaar.Api` | `ASPNETCORE_ENVIRONMENT=Development dotnet run --launch-profile http` | http://localhost:5108 (Swagger at `/swagger`) |
| `Bazaar.Web` | `src/Bazaar.Web` | `npm run dev` | http://localhost:5173 (Vite proxies `/api`, `/connect`, `/.well-known` to the backend) |

Build/test commands are the standard ones: `dotnet build Bazaar.sln` for the backend; `npm run build` (vue-tsc type-check + vite) for the web. See `src/Bazaar.Web/README.md` for the full list of npm scripts.

### Database (SQL Server)

- Both backend services require SQL Server. It runs as a Docker container named `bazaar-sql` (image `mcr.microsoft.com/mssql/server:2022-latest`, port 1433, `sa` / `Your_password123`, started with `--restart unless-stopped`).
- The Docker daemon is NOT managed by systemd in this VM. If `docker ps` fails, start it with: `sudo dockerd > /tmp/dockerd.log 2>&1 &` (then `sudo docker start bazaar-sql` if the container is not already up).
- The default `appsettings.json` connection strings use `Server=.;Trusted_Connection=True;` (Windows auth) which does NOT work on Linux. Dev overrides live in gitignored `appsettings.Development.json` files in `src/Bazaar.Api` and `src/Bazaar.Identity` pointing at `Server=localhost,1433;User Id=sa;Password=Your_password123;TrustServerCertificate=True;`. If those files are missing, recreate them (databases are `BazaarDb` and `BazaarIdentityDb`). They are only loaded because the launch profiles set `ASPNETCORE_ENVIRONMENT=Development`.

### Important caveats

- There are NO EF Core migrations in the repo. `Bazaar.Identity` calls `Database.Migrate()` on startup, which only creates an empty `BazaarIdentityDb` (no app tables). `Bazaar.Api` does not auto-create its schema. So backend write endpoints that touch app tables will fail until schema is created (e.g. via `dotnet ef migrations add` or the `src/Bazaar.Data/Scripts/*.sql` scripts). The services themselves start, connect to SQL Server, and serve Swagger fine.
- `Bazaar.Api`'s default `Identity:Url` is `https://localhost:5001`, which is wrong; the dev override sets it to `http://localhost:5209` (where Identity actually runs) so JWT validation can fetch JWKS.
- The frontend is offline-first (Dexie/IndexedDB) and ships a dev auth bypass. Set `VITE_DEV_BYPASS_AUTH=true` in a gitignored `src/Bazaar.Web/.env`, then click the "dev bypass" button on the login screen to enter the app with seeded data and exercise full CRUD entirely client-side (no backend/DB schema needed). This is the fastest way to demo/test the product UI.
- ESLint is broken: `package.json` pins ESLint v10 (flat-config only) but the repo ships a legacy `.eslintrc.cjs`, so `npm run lint` fails with "couldn't find an eslint.config file". This is a repo/dependency mismatch, not an environment problem. Use `npm run type-check` / `npm run build` for static checking instead.
