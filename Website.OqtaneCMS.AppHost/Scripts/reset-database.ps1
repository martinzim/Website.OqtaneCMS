# Website.OqtaneCMS - Database Reset Script
# Use this to completely reset PostgreSQL database and start fresh

Write-Host "⚠️  DATABASE RESET SCRIPT" -ForegroundColor Red
Write-Host "=========================" -ForegroundColor Red
Write-Host ""
Write-Host "This will:" -ForegroundColor Yellow
Write-Host "  1. Stop all Docker containers" -ForegroundColor Yellow
Write-Host "  2. Remove PostgreSQL data volume" -ForegroundColor Yellow
Write-Host "  3. Delete all Oqtane data" -ForegroundColor Yellow
Write-Host ""
Write-Host "⚠️  ALL DATA WILL BE LOST!" -ForegroundColor Red
Write-Host ""

# Ask for confirmation
$confirmation = Read-Host "Type 'RESET' to confirm"

if ($confirmation -ne "RESET") {
    Write-Host "Cancelled." -ForegroundColor Green
    exit 0
}

Write-Host ""
Write-Host "🗑️  Resetting database..." -ForegroundColor Yellow
Write-Host ""

# Stop all containers
Write-Host "Stopping Docker containers..." -ForegroundColor Cyan
try {
    docker stop $(docker ps -q) 2>$null
    Write-Host "  ✓ Containers stopped" -ForegroundColor Green
} catch {
    Write-Host "  ℹ No containers running" -ForegroundColor Gray
}

# Remove postgres volume
Write-Host "Removing PostgreSQL data volume..." -ForegroundColor Cyan
try {
    docker volume rm artportfolio-apphost-postgres-data 2>$null
    Write-Host "  ✓ Volume removed" -ForegroundColor Green
} catch {
    Write-Host "  ℹ Volume not found or in use" -ForegroundColor Gray
}

# Remove Oqtane content (if exists)
$contentPath = Join-Path $PSScriptRoot "Website.OqtaneCMS.Web\Content"
if (Test-Path $contentPath) {
    Write-Host "Removing Oqtane content folder..." -ForegroundColor Cyan
    Remove-Item -Recurse -Force $contentPath
    Write-Host "  ✓ Content removed" -ForegroundColor Green
}

# Reset appsettings to Install mode
$appsettingsPath = Join-Path $PSScriptRoot "Website.OqtaneCMS.Web\appsettings.json"
if (Test-Path $appsettingsPath) {
    Write-Host "Resetting Oqtane to Install mode..." -ForegroundColor Cyan
    
    $json = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
    if ($json.Installation) {
        $json.Installation.InstallationMode = "Install"
        $json | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
        Write-Host "  ✓ InstallationMode set to Install" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "✅ Database reset complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Run: .\start-dev.ps1" -ForegroundColor White
Write-Host "  2. Go through Oqtane Installation Wizard again" -ForegroundColor White
Write-Host ""
