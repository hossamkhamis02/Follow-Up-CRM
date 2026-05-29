param(
    [switch]$SkipDocker,
    [switch]$SkipMigrations,
    [switch]$SkipFrontend
)

$ErrorActionPreference = "Stop"
$RepoRoot = $PSScriptRoot

Write-Host "=== FollowUp CRM Development Setup ===" -ForegroundColor Cyan

# ── 1. Environment file ────────────────────────────────────────────────
if (-not (Test-Path "$RepoRoot\.env")) {
    Copy-Item "$RepoRoot\.env.example" "$RepoRoot\.env"
    Write-Host "[SETUP] Created .env from .env.example — edit passwords before continuing!" -ForegroundColor Yellow
    Write-Host "[SETUP] Required: POSTGRES_PASSWORD, Jwt__Key, PGADMIN_PASSWORD" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "[OK] .env exists" -ForegroundColor Green
}

# ── 2. Docker (PostgreSQL + pgAdmin) ────────────────────────────────────
if (-not $SkipDocker) {
    Write-Host "[SETUP] Starting PostgreSQL + pgAdmin..." -ForegroundColor Cyan
    docker-compose -f "$RepoRoot\docker-compose.yml" up -d postgres pgadmin
    Write-Host "[SETUP] Waiting for PostgreSQL healthcheck..." -ForegroundColor Cyan
    $ready = $false
    for ($i = 0; $i -lt 30; $i++) {
        $status = docker inspect followup-crm-postgres --format '{{.State.Health.Status}}' 2>$null
        if ($status -eq "healthy") {
            $ready = $true
            break
        }
        Start-Sleep -Seconds 2
    }
    if ($ready) {
        Write-Host "[OK] PostgreSQL is healthy" -ForegroundColor Green
    } else {
        Write-Host "[WARN] PostgreSQL healthcheck timeout — check docker logs" -ForegroundColor Yellow
    }
}

# ── 3. Backend ──────────────────────────────────────────────────────────
Write-Host "[SETUP] Restoring backend packages..." -ForegroundColor Cyan
dotnet restore "$RepoRoot\backend\FollowUpCrm.sln"

if (-not $SkipMigrations) {
    Write-Host "[SETUP] Applying EF Core migrations..." -ForegroundColor Cyan
    dotnet ef database update --project "$RepoRoot\backend\src\FollowUpCrm.Api"
}

Write-Host "[SETUP] Building backend..." -ForegroundColor Cyan
dotnet build "$RepoRoot\backend\FollowUpCrm.sln" --no-restore
Write-Host "[OK] Backend built" -ForegroundColor Green

# ── 4. Frontend ─────────────────────────────────────────────────────────
if (-not $SkipFrontend) {
    Write-Host "[SETUP] Installing frontend dependencies..." -ForegroundColor Cyan
    Push-Location "$RepoRoot\frontend"
    npm install
    Pop-Location
    Write-Host "[OK] Frontend dependencies installed" -ForegroundColor Green
}

# ── Summary ─────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "=== Setup Complete ===" -ForegroundColor Cyan
Write-Host "Start backend:  dotnet run --project backend/src/FollowUpCrm.Api" -ForegroundColor White
Write-Host "Start frontend: cd frontend && ng serve" -ForegroundColor White
Write-Host "pgAdmin:        http://localhost:5050" -ForegroundColor White