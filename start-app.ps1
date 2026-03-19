# AI Log Analyzer - Quick Start Script
Write-Host "?? Starting AI Log Analyzer..." -ForegroundColor Cyan
Write-Host ""

# Stop any existing instances
$existingProcess = Get-Process -Name "LogAnalyzer.API" -ErrorAction SilentlyContinue
if ($existingProcess) {
    Write-Host "??  Stopping existing process..." -ForegroundColor Yellow
    Stop-Process -Name "LogAnalyzer.API" -Force
    Start-Sleep -Seconds 1
}

# Build the project
Write-Host "?? Building project..." -ForegroundColor Cyan
dotnet build --no-restore

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Build successful!" -ForegroundColor Green
    Write-Host ""
    
    # Start the application in background
    Write-Host "?? Starting application..." -ForegroundColor Cyan
    Start-Process "dotnet" -ArgumentList "run" -WorkingDirectory $PSScriptRoot -WindowStyle Minimized
    
    # Wait for application to start
    Write-Host "? Waiting for application to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 3
    
    # Open browser
    Write-Host "?? Opening browser..." -ForegroundColor Cyan
    Start-Process "https://localhost:5001"
    
    Write-Host ""
    Write-Host "? AI Log Analyzer is running!" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? URLs:" -ForegroundColor Cyan
    Write-Host "   Main App: https://localhost:5001" -ForegroundColor White
    Write-Host "   HTTP:     http://localhost:5000" -ForegroundColor White
    Write-Host "   OpenAPI:  https://localhost:5001/openapi/v1.json" -ForegroundColor White
    Write-Host ""
    Write-Host "Press any key to stop the application..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    
    # Stop the application
    Write-Host ""
    Write-Host "??  Stopping application..." -ForegroundColor Yellow
    Stop-Process -Name "LogAnalyzer.API" -Force -ErrorAction SilentlyContinue
    Write-Host "? Application stopped." -ForegroundColor Green
} else {
    Write-Host "? Build failed! Please check the errors above." -ForegroundColor Red
}
