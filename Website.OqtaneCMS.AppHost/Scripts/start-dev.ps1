# Website.OqtaneCMS - Oqtane Development Quick Start
# PowerShell setup script

Write-Host "🎨 Website.OqtaneCMS - Oqtane CMS Setup" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Check prerequisites
Write-Host "✓ Checking prerequisites..." -ForegroundColor Yellow

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "  ✓ .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "  ✗ .NET SDK not found. Install .NET 10 SDK" -ForegroundColor Red
    exit 1
}

# Check Docker
try {
    docker --version | Out-Null
    Write-Host "  ✓ Docker installed" -ForegroundColor Green
    
    # Check if Docker is running
    docker ps | Out-Null
    Write-Host "  ✓ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Docker not running. Start Docker Desktop first!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "📦 Configuration:" -ForegroundColor Yellow
Write-Host "  Database: PostgreSQL (Docker)"
Write-Host "  Username: oqtane"
Write-Host "  Password: OqtaneDevPassword123!"
Write-Host "  pgAdmin:  http://localhost:60751"
Write-Host ""

# Ask user if they want to continue
Write-Host "Press any key to start Aspire AppHost (Ctrl+C to cancel)..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Write-Host ""
Write-Host "🚀 Starting Aspire AppHost..." -ForegroundColor Cyan
Write-Host ""

# Run Aspire AppHost
try {
    Push-Location $PSScriptRoot
    dotnet run --project Website.OqtaneCMS.AppHost
} catch {
    Write-Host ""
    Write-Host "❌ Failed to start AppHost" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}
