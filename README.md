# EzFit

A personal fitness tracker for logging meals, workouts, and sleep. Entries
are logged via free text or a screenshot upload (e.g. a photo of a nutrition
label or a workout summary), and an AI service extracts structured data
(calories, macros, duration, heart rate, sleep stages, etc.) from that input
automatically instead of requiring manual form entry.

## Tech stack

**Backend**
- .NET 8 / ASP.NET Core Web API (C#)
- Entity Framework Core 8.0.29 + Npgsql.EntityFrameworkCore.PostgreSQL 8.0.11 (PostgreSQL)
- SixLabors.ImageSharp 3.1.12 for image processing (tiling/re-encoding uploads to WebP)
- Swashbuckle.AspNetCore 6.6.2 (Swagger/OpenAPI, dev only)
- Gemini API called directly over `HttpClient` (no SDK) behind an `IAiService` abstraction

**Frontend**
- React 19 + TypeScript, built with Vite 8
- React Router 7 for routing
- TanStack Query 5 for server state
- Tailwind CSS 4 + Radix UI primitives (shadcn-style components), `class-variance-authority`, `tailwind-merge`
- Axios for API calls
- oxlint for linting

**Infra**
- Backend: Docker container on Render (free tier)
- Database: PostgreSQL on Neon (free tier)
- Frontend: Vercel

## Architecture

**Backend** follows a layered structure: `Controllers` (thin, per-resource:
`Day`, `Entry`, `Log`) call into `Services` (business logic — day
summaries, entry creation, image processing, AI extraction), which use
`Repositories` for data access, which sit on top of an EF Core
`AppDbContext` (`Data/AppDbContext.cs`) talking to PostgreSQL. AI
extraction is isolated behind `IAiService`, currently implemented by
`GeminiAiService`, which sends free text and/or uploaded images to Gemini
using function-calling ("tools" for `record_meal`, `record_activity`,
`record_sleep`, `reject_entry`) and maps the structured tool-call
response into entries.

**Frontend** is a small Vite/React SPA: `pages/` holds the three routed
views (day list, day detail, add entry), `components/` holds shared UI
(day cards, entry cards, app shell, shadcn-style primitives under `ui/`),
and `api/` wraps the backend endpoints with Axios + TanStack Query.

## API endpoints

- `GET /api/day?date=YYYY-MM-DD` — summary for a single day
- `GET /api/day/list?count=7` — summaries for the N most recent days (max 30)
- `POST /api/entry?date=YYYY-MM-DD` — create an entry manually
- `POST /api/log?date=YYYY-MM-DD` — form-data (`message`, `images[]`); runs AI extraction and creates the resulting entries

## Local setup

### Backend

Requires .NET 8 SDK.

1. `cd backend/EzFit/EzFit`
2. Configure secrets with `dotnet user-secrets` (don't put these in `appsettings.json` or `appsettings.Development.json`):
   ```
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<postgres connection string>"
   dotnet user-secrets set "Gemini:ApiKey" "<gemini api key>"
   dotnet user-secrets set "Security:ApiKey" "<shared api key>"
   ```
   - `ConnectionStrings:DefaultConnection` — a PostgreSQL connection string. **Local dev currently points at the same Neon database as production** — there's no separate local DB set up yet, so be aware writes affect the live data.
   - `Gemini:ApiKey` — a Gemini API key (required for `/api/log`; `/api/entry` and `/api/day` work without it).
   - `Gemini:Model` — optional, defaults to `gemini-3.5-flash` if unset.
   - `Security:ApiKey` — optional. When set, every `/api` request must send a matching
     `X-Api-Key` header (see `ApiKeyMiddleware`). This is a stopgap against casual
     scripted abuse, not real security — the key ships in the public frontend bundle.
     Leave it unset locally and the check no-ops (a startup warning is logged).
3. Run it:
   ```
   dotnet run --launch-profile https
   ```
   This serves on `https://localhost:7059` (and `http://localhost:5222`), with Swagger UI at `/swagger`.

Non-secret upload limits live in `appsettings.json` under `Uploads` (no
user-secrets needed, override per-environment via env vars if desired):
- `MaxFileSizeBytes` — per-file cap enforced in `LogController` (default 10 MB)
- `MaxFileCount` — max images per `/api/log` request (default 5)
- `MaxPixels` / `MaxDimension` — header-only checks in `ImageService` that
  reject oversized images before they're decoded

Also non-secret, under `RateLimiting` (fixed-window limits, partitioned by client IP):
- `Log:PermitLimit` / `Log:WindowSeconds` — applies to `POST /api/log` (default 10/60s)
- `Api:PermitLimit` / `Api:WindowSeconds` — applies to the rest of `/api` (default 60/60s)

And `CurrentUser:Id` — the fixed user id used everywhere until real auth lands
(read by `ICurrentUserProvider`; default `1`).

### Frontend

Requires Node.js.

1. `cd frontend`
2. `npm install`
3. Copy `.env.example` to `.env.local` and set `VITE_API_URL` to match the backend profile you're running (defaults to `https://localhost:7059/api` if unset; if the browser rejects the self-signed dev cert, either open the swagger URL once to accept it, or point at `http://localhost:5222/api` instead). If the backend has `Security:ApiKey` configured, also set `VITE_API_KEY` to the same value.
4. `npm run dev`

## Known limitations

- **No authentication yet.** Auth is a planned, later stage of this
  project — not yet implemented. In the meantime the API is gated by rate
  limiting, upload limits, and an optional shared `X-Api-Key` header — none
  of which are a substitute for real per-user auth.
- **Local dev shares the production database.** There's no local/dev
  Postgres instance configured — running the backend locally reads and
  writes to the same Neon database the live deployment uses. This is a
  current simplification, not the intended long-term setup.
- **Single AI provider.** Extraction goes through Gemini only, behind an
  `IAiService` abstraction. A Claude-based fallback is planned but not
  implemented.
- **Free-tier hosting.** The Render backend cold-starts after 15 minutes
  idle, so the first request in a while will be slow.
- **Uploaded images are not durably stored.** Render's filesystem is
  ephemeral — files written to `App_Data/uploads` are lost on every
  redeploy and cold start/restart. `Entry.ImagePath` values in the
  database can end up pointing at files that no longer exist. Moving
  uploads to external object storage is a future step.
