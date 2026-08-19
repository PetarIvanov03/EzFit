# EzFit

A personal fitness tracker for logging meals, workouts, and sleep. Entries
are logged via free text or a screenshot upload (e.g. a photo of a nutrition
label or a workout summary), and an AI service extracts structured data
(calories, macros, duration, heart rate, sleep stages, etc.) from that input
automatically instead of requiring manual form entry.

## Live demo

- Frontend: https://ez-fit.vercel.app
- Backend API: https://ezfit.onrender.com

Backend runs on Render's free tier, so it spins down after 15 minutes of
inactivity — the first request after idle can take a while to respond while
the container cold-starts.

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
2. Configure secrets (don't commit these — use `dotnet user-secrets` or a local `appsettings.Development.json`):
   - `ConnectionStrings:DefaultConnection` — a PostgreSQL connection string. **Local dev currently points at the same Neon database as production** — there's no separate local DB set up yet, so be aware writes affect the live data.
   - `Gemini:ApiKey` — a Gemini API key (required for `/api/log`; `/api/entry` and `/api/day` work without it).
   - `Gemini:Model` — optional, defaults to `gemini-3.5-flash` if unset.
3. Run it:
   ```
   dotnet run --launch-profile https
   ```
   This serves on `https://localhost:7059` (and `http://localhost:5222`), with Swagger UI at `/swagger`.

### Frontend

Requires Node.js.

1. `cd frontend`
2. `npm install`
3. Copy `.env.example` to `.env.local` and set `VITE_API_URL` to match the backend profile you're running (defaults to `https://localhost:7059/api` if unset; if the browser rejects the self-signed dev cert, either open the swagger URL once to accept it, or point at `http://localhost:5222/api` instead).
4. `npm run dev`

## Known limitations

- **No authentication.** All requests are scoped to a single hardcoded
  user (`Id = 1`), seeded directly into the `Users` table. There's no
  login, no per-user isolation, and no session handling yet.
- **Local dev shares the production database.** There's no local/dev
  Postgres instance configured — running the backend locally reads and
  writes to the same Neon database the live deployment uses. This is a
  current simplification, not the intended long-term setup.
- **Single AI provider.** Extraction goes through Gemini only, behind an
  `IAiService` abstraction. A Claude-based fallback is planned but not
  implemented.
- **Free-tier hosting.** The Render backend cold-starts after 15 minutes
  idle, so the first request in a while will be slow.
