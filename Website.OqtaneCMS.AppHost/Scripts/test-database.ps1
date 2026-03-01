# Website.OqtaneCMS - Database Connection Test
# Use this to verify PostgreSQL connection

Write-Host "🔍 Database Connection Test" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    Write-Host "  ✓ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Docker is not running" -ForegroundColor Red
    Write-Host "  Start Docker Desktop first!" -ForegroundColor Yellow
    exit 1
}

# Check for PostgreSQL container
Write-Host "Checking PostgreSQL container..." -ForegroundColor Yellow
$postgresContainer = docker ps --filter "ancestor=postgres" --format "{{.ID}}" | Select-Object -First 1

if ($postgresContainer) {
    Write-Host "  ✓ PostgreSQL container found: $postgresContainer" -ForegroundColor Green
    
    # Get container details
    $containerInfo = docker inspect $postgresContainer | ConvertFrom-Json
    $port = ($containerInfo.NetworkSettings.Ports.'5432/tcp' | Select-Object -First 1).HostPort
    
    Write-Host ""
    Write-Host "Container Details:" -ForegroundColor Cyan
    Write-Host "  ID:   $postgresContainer"
    Write-Host "  Port: $port"
    
    # Test connection
    Write-Host ""
    Write-Host "Testing connection..." -ForegroundColor Yellow
    
    $env:PGPASSWORD = "OqtaneDevPassword123!"
    $testResult = docker exec $postgresContainer psql -U oqtane -d artportfolio -c "SELECT version();" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Connection successful!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Database Version:" -ForegroundColor Cyan
        Write-Host $testResult
        
        # Check for Oqtane tables
        Write-Host ""
        Write-Host "Checking Oqtane tables..." -ForegroundColor Yellow
        $tableCount = docker exec $postgresContainer psql -U oqtane -d artportfolio -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_name LIKE 'oqt_%';" 2>&1
        
        if ($tableCount -match '\d+') {
            $count = $tableCount.Trim()
            if ([int]$count -gt 0) {
                Write-Host "  ✓ Found $count Oqtane tables" -ForegroundColor Green
                Write-Host "  Oqtane is already installed!" -ForegroundColor Green
            } else {
                Write-Host "  ℹ No Oqtane tables found" -ForegroundColor Yellow
                Write-Host "  Database is ready for fresh installation" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "  ✗ Connection failed" -ForegroundColor Red
        Write-Host $testResult
    }
    
} else {
    Write-Host "  ✗ No PostgreSQL container running" -ForegroundColor Red
    Write-Host ""
    Write-Host "Start the application first:" -ForegroundColor Yellow
    Write-Host "  .\start-dev.ps1" -ForegroundColor White
}

Write-Host ""
Write-Host "Connection String (for reference):" -ForegroundColor Cyan
Write-Host "  Host=localhost;Port=5432;Database=artportfolio;Username=oqtane;Password=OqtaneDevPassword123!" -ForegroundColor Gray
Write-Host ""
Write-Host "pgAdmin URL:" -ForegroundColor Cyan
Write-Host "  http://localhost:60751" -ForegroundColor White
Write-Host "  Login: admin@admin.com / admin" -ForegroundColor Gray
Write-Host ""
