# 🇭🇺 Hongarije 2026

Collaborative trip-planning app for our Hungary holiday — activities, food ideas, packing lists, shopping, and practical info, all shared in real time.

| Tab | What it does |
|-----|-------------|
| 🏖️ Activiteiten | Suggest activities and vote on them |
| 🍽️ Eten | Food ideas grouped by category, with votes |
| 🧳 Valies | Personal + shared packing lists |
| 🛒 Boodschappen | Shared shopping list, auto-categorised |
| 📌 Info | Accommodation, vignettes, currency — admin-only edits |

---

## Getting started (dev container — recommended)

The dev container gives you a **fully pre-configured environment** in one click: .NET 10 SDK, PostgreSQL, and an nginx proxy — no local installs beyond VS Code and Docker.

### Prerequisites

You need these three things installed on your computer. Click each link for the installer:

1. **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** — runs the containers.  
   After installing, open Docker Desktop at least once so it can finish setting up.

2. **[Visual Studio Code](https://code.visualstudio.com/)** — the code editor.

3. **[Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)** — lets VS Code work inside a container.  
   Install it from within VS Code: press `Ctrl+Shift+X`, search for *Dev Containers*, click Install.

### Step-by-step

#### 1. Get the code

If you received a ZIP file, unzip it somewhere on your computer (e.g. `Documents/Hongarije-2026`).

If the repo is on GitHub, clone it:
```
git clone https://github.com/YOUR_USERNAME/Hongarije-2026.git
```

#### 2. Open the folder in VS Code

Open VS Code, then go to **File → Open Folder** and select the `Hongarije-2026` folder.

#### 3. Reopen in the dev container

VS Code will show a pop-up in the bottom-right corner:

> **"Folder contains a Dev Container configuration file. Reopen in Container?"**

Click **Reopen in Container**.

> 💡 If you miss the pop-up: press `Ctrl+Shift+P`, type `Dev Containers: Reopen in Container`, and press Enter.

#### 4. Wait for the first-time setup

The first time takes **2–4 minutes** while Docker downloads the images and builds the environment. You will see a progress notification in the bottom-right. VS Code will reload automatically when it's done.

> Subsequent opens are fast (under 10 seconds) because everything is cached.

#### 5. Wait for the API to start

Once VS Code reopens, a terminal panel labelled **🚀 Start API** appears automatically at the bottom of the screen. The backend is compiling — wait until you see:

```
info: Now listening on: http://localhost:5000
info: Application started.
```

This takes about 15–30 seconds.

#### 6. Open the app

A browser tab should open automatically at **http://localhost:3000** 🎉

If it does not open automatically, press `Ctrl+Shift+P` → type **Ports** → select **Focus on Ports View**, then click the globe icon next to port 3000.

---

## Making changes

### Frontend (HTML, CSS, JavaScript)

The frontend lives in the `frontend/` folder:

- `index.html` — page structure and modal dialogs
- `styles.css` — all styling (CSS variables, layout, components)
- `app.js` — all logic (API calls, rendering, event handling)

**Workflow:**

1. Open the file you want to edit in VS Code.
2. Make your changes and save (`Ctrl+S`).
3. Switch to your browser and press `F5` to refresh.
4. Your changes appear immediately — no build step needed.

> 💡 **Tip:** VS Code has [Prettier](https://prettier.io/) installed. Press `Shift+Alt+F` to auto-format any file.

### Backend (API / C#)

The backend lives in `backend/HongarijePlanner.Api/`:

- `Controllers/` — one file per feature (wishlist, packing, etc.)
- `Models/` — the data classes (e.g. `WishlistItem.cs`)
- `Data/AppDbContext.cs` — the database configuration
- `Program.cs` — app startup and middleware

**Workflow:**

1. Edit the files you need.
2. The running API needs to restart to pick up changes.  
   In the **🚀 Start API** terminal, press `Ctrl+C` to stop it, then press the ↑ arrow key and Enter to restart it.
3. Wait for `Now listening on: http://localhost:5000` again.
4. Refresh your browser.

> 💡 **Debugging:** Press `F5` (or go to **Run → Start Debugging**) to launch the API with a debugger attached. You can set breakpoints by clicking in the margin next to a line number.

---

## Project structure

```
.
├── frontend/
│   ├── index.html        Page structure and modals
│   ├── styles.css        All styling
│   ├── app.js            All client-side logic
│   └── Dockerfile        nginx for production
│
├── backend/
│   └── HongarijePlanner.Api/
│       ├── Controllers/  One controller per feature
│       ├── Data/         EF Core DbContext (PostgreSQL)
│       ├── Models/       Database entity classes
│       ├── Program.cs    App startup
│       └── Dockerfile    Production container
│
├── docker-compose.yml    Production deploy (Portainer)
│
├── .devcontainer/
│   ├── devcontainer.json Dev container config
│   ├── docker-compose.yml Services for dev (SDK + nginx + postgres)
│   ├── nginx.dev.conf    Dev nginx that proxies /api to the backend
│   └── setup.sh          Runs on first container start
│
└── .vscode/
    ├── tasks.json        Auto-starts the API when VS Code opens
    ├── launch.json       Debug config (F5)
    └── extensions.json   Recommended extensions
```

---

## Changing trip details

All trip-specific links and the admin name are in **`frontend/app.js`** at the top of the file:

```js
const ADMIN_NAME = 'Yana';       // the person who can delete things and manage Info
const MAPS_LINK  = '...';        // Google Maps link to the accommodation
const HOUSE_LINK = '...';        // Interhome / booking link
const WAZE_LINK  = '...';        // Waze link to the accommodation
const VIGNET_AT  = '...';        // Austrian toll vignette purchase link
const VIGNET_HU  = '...';        // Hungarian toll vignette purchase link
```

---

## Deploying (Portainer)

### Required environment variables

The app **will not start** without these set. Never commit real values to git.

| Variable | Description | Example |
|----------|-------------|---------|
| `DB_PASSWORD` | PostgreSQL password — pick something strong | `correct-horse-battery-staple` |
| `APP_PORT` | Host port the app is served on | `3000` |

### Steps

1. Push your repo to GitHub (or copy the files to your server).
2. In Portainer → **Stacks → Add stack**.
3. Choose **Repository** and point it at your GitHub repo, or use **Web editor** and paste `docker-compose.yml`.
4. Scroll down to **Environment variables** and add:
   - `DB_PASSWORD` → your chosen password
   - `APP_PORT` → e.g. `3000` (or `80` to serve on the default HTTP port)
5. Click **Deploy the stack**.

The app will be available at `http://<your-server-ip>:<APP_PORT>`.  
Database data is stored in a Docker volume and survives restarts and redeployments.

> **Reverse proxy tip:** if you run Nginx Proxy Manager or Traefik in front, set `APP_PORT` to any free internal port and let the proxy handle the public domain + HTTPS.

---

## Authentik SSO (optional)

When the app is behind Authentik forward auth, users are logged in automatically — no login screen needed. The "wissel" button still lets someone switch to another account manually.

### How it works

Authentik injects `X-authentik-username` into every proxied request. The app reads this header at `GET /api/auth/me` and logs the user in automatically.

### Setup in Authentik

1. Create a **Proxy Provider** in Authentik:
   - Mode: **Forward auth (single application)**
   - External host: your public URL (e.g. `https://hongarije.example.com`)
2. Create an **Application** linked to that provider.
3. Assign the application to the users/group who should have access.

### Setup in Nginx Proxy Manager

1. Add a new Proxy Host pointing to `http://<docker-host>:<APP_PORT>`.
2. Under **Advanced**, add the Authentik forward auth snippet:
   ```nginx
   auth_request /outpost.goauthentik.io/auth/nginx;
   error_page 401 = @goauthentik_proxy_signin;
   auth_request_set $auth_cookie $upstream_http_set_cookie;
   add_header Set-Cookie $auth_cookie;
   auth_request_set $authentik_username $upstream_http_x_authentik_username;
   proxy_set_header X-authentik-username $authentik_username;

   location /outpost.goauthentik.io {
       proxy_pass https://<your-authentik-host>/outpost.goauthentik.io;
       proxy_set_header Host <your-authentik-host>;
   }

   location @goauthentik_proxy_signin {
       internal;
       return 302 /outpost.goauthentik.io/start?rd=$scheme://$http_host$request_uri;
   }
   ```
3. Replace `<your-authentik-host>` with your Authentik domain.

> If the `X-authentik-username` header is absent (e.g. in local dev), the app falls back to the normal login screen automatically.

---

## Troubleshooting

### "Reopen in Container" pop-up did not appear
Press `Ctrl+Shift+P`, type `Dev Containers: Reopen in Container`, and press Enter.

### The container takes very long to build
This is normal the first time — Docker is downloading images (~1 GB total). Subsequent opens are fast.

### Port 3000 / 5000 is already in use
Something else on your computer is using that port. Stop the other application, then rebuild the container (`Ctrl+Shift+P` → `Dev Containers: Rebuild Container`).

### The API terminal shows a database connection error
The database might still be starting. Wait 10 seconds, then restart the API: press `Ctrl+C` in the **🚀 Start API** terminal and run `dotnet run` again. If it persists, run `Ctrl+Shift+P` → `Dev Containers: Rebuild Container`.

### I accidentally closed the "🚀 Start API" terminal
Open a new terminal in VS Code (`Ctrl+` `` ` ``), then run:
```
cd /workspace/backend/HongarijePlanner.Api
dotnet run
```

### The browser shows a blank page or errors after my changes
Press `F12` in the browser to open DevTools and check the **Console** tab for error messages.

---

## Database migrations

If you change a model in `Models/`, you need to create a migration so the database schema updates:

```bash
# In the VS Code terminal (inside the dev container):
cd /workspace/backend/HongarijePlanner.Api
dotnet ef migrations add DescribeYourChange
dotnet ef database update
```

### "Error response from daemon: accessing specified distro mount service ... ubuntu.sock"

This means Docker Desktop lost its connection to the Ubuntu WSL distro. Fix it in three steps:

1. **Shut down WSL** — open PowerShell and run:
   ```
   wsl --shutdown
   ```
2. **Restart Docker Desktop** — right-click the whale icon in the system tray → *Quit Docker Desktop*, then reopen it and wait for it to fully start (the whale icon stops animating).

3. **Re-enable WSL integration** — Docker Desktop → Settings → Resources → WSL Integration → make sure *Ubuntu* is checked → click *Apply & Restart*.

Then try *Dev Containers: Reopen in Container* again.