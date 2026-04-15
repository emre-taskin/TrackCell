$ErrorActionPreference = "Stop"
$env:PGPASSWORD = '1864m90p04'

Write-Host "Connecting to trackcell database to list tables..." -ForegroundColor Cyan
psql -h localhost -U postgres -d trackcell -c "\dt"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to connect to the database. Ensure PostgreSQL is running and psql is in your PATH." -ForegroundColor Red
} else {
    Write-Host "Done!" -ForegroundColor Green
}
