#!/usr/bin/env bash
set -e

echo ""
echo "╔══════════════════════════════════════════════╗"
echo "║   🇭🇺  Hongarije 2026 — container setup      ║"
echo "╚══════════════════════════════════════════════╝"
echo ""

echo "📦  Restoring .NET packages (shared with api service)..."
cd /workspace/backend/HongarijePlanner.Api
dotnet restore --verbosity quiet

echo "🔨  Building project (catching errors early)..."
dotnet build -c Debug --no-restore --verbosity quiet

echo ""
echo "╔══════════════════════════════════════════════╗"
echo "║  ✅  Setup complete!                          ║"
echo "║                                              ║"
echo "║  The API starts automatically as a Docker    ║"
echo "║  service alongside this container.           ║"
echo "║  Check its logs: docker logs -f hongarije... ║"
echo "║                                              ║"
echo "║  👉  Open http://localhost:3000              ║"
echo "║  📖  Swagger: http://localhost:5000/swagger  ║"
echo "║                                              ║"
echo "║  To debug: run '▶️ Run API locally' task     ║"
echo "║  (stop the api Docker service first)         ║"
echo "╚══════════════════════════════════════════════╝"
echo ""
