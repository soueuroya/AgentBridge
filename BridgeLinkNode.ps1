# BridgeLinkNode Launcher (Antigravity Real-Time Link)
$nodeScript = "BridgeLinkNode.mjs"
$bridgeDir = ".agentbridge_bridge"

if (-Not (Test-Path $bridgeDir)) { New-Item -ItemType Directory -Path $bridgeDir }

# 0. Clean up any existing listeners on port 11500
Get-NetTCPConnection -LocalPort 11500 -ErrorAction SilentlyContinue | ForEach-Object {
    Try { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue } Catch {}
}

# 1. Clean up stale response files
$responseFile = "$bridgeDir/ActiveResponse.json"
if (Test-Path $responseFile) { Remove-Item $responseFile -ErrorAction SilentlyContinue }

Write-Host "`n[Antigravity Link] Initializing Real-Time Bridge..." -ForegroundColor Cyan

# 2. Check for node requirements
if (-Not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "[Link] ERROR: Node.js version 22+ is required but not found in PATH." -ForegroundColor Red
    exit 1
}

# 3. Launch Node.js Bridge
Write-Host "[Link] Starting Node.js listener..." -ForegroundColor Gray
node "$nodeScript"

Write-Host "[Link] Bridge service stopped." -ForegroundColor Yellow
Pause
