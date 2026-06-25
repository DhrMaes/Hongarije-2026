# Copilot Instructions

## Project overview

Collaborative trip-planning app for a group holiday to Hungary. Dutch UI. The app has five tabs: **Activiteiten** (activities/wishlist), **Eten** (food ideas), **Valies** (packing), **Boodschappen** (shopping), **Info** (admin-only).

```
frontend/          Vanilla HTML/CSS/JS — no framework, no build step
backend/           ASP.NET Core 10 Web API
  HongarijePlanner.Api/
    Controllers/   One controller per feature area
    Data/          EF Core DbContext (Npgsql, snake_case naming)
    Models/        Entity classes
docker-compose.yml PostgreSQL + backend + frontend (nginx) — Portainer-ready
.devcontainer/     VS Code dev container (SDK + PostgreSQL auto-started)
```

## Architecture

### Frontend
- `frontend/index.html` — HTML structure and modal templates only (no inline CSS or JS)
- `frontend/styles.css` — all styling; CSS custom properties defined in `:root`
- `frontend/app.js` — all client logic; fetches the API on every user action (no local cache)

`me` (global string) holds the current user's name. `isAdmin()` checks `me === ADMIN_NAME`.

Every render function is `async` and re-fetches from the API: `renderWishlist()`, `renderItinerary()`, `renderPacking()`, `renderShopping()`, `renderInfo()`. `renderAll()` runs them in parallel via `Promise.all`.

HTML escaping: always use `esc(value)` when interpolating user strings into innerHTML templates.

### Backend
ASP.NET Core 10, controller-based. EF Core with Npgsql. `EnsureCreated()` bootstraps the schema on first startup (no migrations needed for fresh deploys).

**Connection strings**
- Docker / production: `appsettings.json` — `Host=db;Port=5432;...`
- Local dev: `appsettings.Development.json` — `Host=localhost;Port=5432;...`
- Dev container: injected as env var `ConnectionStrings__DefaultConnection`

**snake_case naming**: all table and column names are snake_case via `UseSnakeCaseNamingConvention()` (requires the `EFCore.NamingConventions` package).

**HTTPS redirection** is skipped when `DOTNET_RUNNING_IN_CONTAINER=true` (handled by a reverse proxy instead).

### Database schema (key relationships)
- `WishlistVote` and `ItineraryVote` have composite PKs `(item_id, user_name)` — configured via Fluent API in `AppDbContext`.
- Votes are toggled server-side: if the same direction already exists, delete it (un-vote); otherwise upsert.

## Key conventions

### Admin system
`ADMIN_NAME = 'Yana'` is hardcoded in `frontend/app.js`. Admin-only: delete any item, add Info cards. The backend does **not** enforce this — admin checks are client-side only.

### API base URL
`const API = window.location.port === '5500' ? 'http://localhost:5000/api' : '/api'` in `app.js`. In dev container / local dev, the browser and API are on different ports so set this to `http://localhost:5000/api` for local runs without a reverse proxy.

### IDs
Generated server-side: `Guid.NewGuid().ToString("N")[..8]` (8-char hex string).

### Voting endpoints
`POST /api/wishlist/{id}/vote` and `POST /api/itinerary/{id}/vote` — body: `{ "user": "string", "direction": "up"|"down" }`.

### Packing endpoints
- Personal items: `/api/packing?user={name}`, toggle: `PATCH /api/packing/{id}/toggle`
- Shared items: `/api/packing/shared`, toggle: `PATCH /api/packing/shared/{id}/toggle` with body `{ "packedBy": "string" }`

### Special Info cards
`InfoItem.Special` field drives custom widgets rendered client-side:
- `"maps"` -> Google Maps + Waze buttons
- `"vignets"` -> Austria / Hungary toll-vignette links
- `"currency"` -> EUR <-> HUF calculator (rate ~410, hardcoded in `app.js`)

### Constants to update when trip details change (all in `frontend/app.js`)
```js
const ADMIN_NAME  = 'Yana';
const MAPS_LINK   = '...';
const HOUSE_LINK  = '...';
const WAZE_LINK   = '...';
const VIGNET_AT   = '...';
const VIGNET_HU   = '...';
```

## Running locally

### Dev container (recommended)
Open in VS Code → **Reopen in Container**. On first open, `setup.sh` restores and builds the project (~2-4 min). After that, the **🚀 Start API** VS Code task starts automatically on every open (via `tasks.json` `runOn: folderOpen` + `task.allowAutomaticTasks: on`). The nginx `proxy` service (port 3000) proxies `/api/*` to the backend, so `const API = '/api'` works as-is. Browser auto-opens to `http://localhost:3000`.

### Docker Compose
```bash
docker compose up --build
# frontend -> http://localhost:3000
# API      -> http://localhost:5000
# Swagger  -> http://localhost:5000/swagger
```

### Backend only
```bash
cd backend/HongarijePlanner.Api
dotnet run
```

## EF Core migrations
```bash
cd backend/HongarijePlanner.Api
dotnet ef migrations add <MigrationName>
dotnet ef database update
```


