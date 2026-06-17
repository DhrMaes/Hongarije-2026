#!/usr/bin/env bash
set -e

echo ""
echo "╔══════════════════════════════════════════════╗"
echo "║   🇭🇺  Hongarije 2026 — container setup      ║"
echo "╚══════════════════════════════════════════════╝"
echo ""

echo "📦  Restoring .NET packages..."
cd /workspace/backend/HongarijePlanner.Api
dotnet restore --verbosity quiet

echo "🔨  Building project (catching errors early)..."
dotnet build -c Debug --no-restore --verbosity quiet

echo ""
echo "╔══════════════════════════════════════════════╗"
echo "║  ✅  Setup complete! What happens next:       ║"
echo "║                                              ║"
echo "║  1. The API starts automatically in the      ║"
echo "║     '🚀 Start API' terminal panel.           ║"
echo "║     Wait for: 'Now listening on :5000'       ║"
echo "║                                              ║"
echo "║  2. Open the app in your browser:            ║"
echo "║     👉  http://localhost:3000                ║"
echo "║     (nginx proxies /api to the backend)      ║"
echo "║                                              ║"
echo "║  3. Edit frontend files — refresh browser.   ║"
echo "║     No build step needed.                    ║"
echo "╚══════════════════════════════════════════════╝"
echo ""
