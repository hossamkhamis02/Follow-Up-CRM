param(
    [switch]$KeepVolumes
)

$ErrorActionPreference = "Stop"
$RepoRoot = $PSScriptRoot

Write-Host "=== Reset FollowUp CRM Database ===" -ForegroundColor Cyan

if ($KeepVolumes) {
    Write-Host "[RESET] Stopping containers (keeping volumes)..." -ForegroundColor Yellow
    docker-compose -f "$RepoRoot\docker-compose.yml" down
} else {
    Write-Host "[RESET] Stopping containers and removing volumes..." -ForegroundColor Red
    docker-compose -f "$RepoRoot\docker-compose.yml" down -v
}

Write-Host "[RESET] Starting PostgreSQL + pgAdmin..." -ForegroundColor Cyan
docker-compose -f "$RepoRoot\docker-compose.yml" up -d postgres pgadmin

Write-Host "[RESET] Waiting for PostgreSQL healthcheck..." -ForegroundColor Cyan
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
    Write-Host "[RESET] Applying migrations..." -ForegroundColor Cyan
    dotnet ef database update --project "$RepoRoot\backend\src\FollowUpCrm.Api"
    Write-Host "[OK] Database reset complete" -ForegroundColor Green
} else {
    Write-Host "[ERROR] PostgreSQL failed to start — check docker logs" -ForegroundColor Red
    exit 1
}