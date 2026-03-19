# ??? PowerShell Commands - AI Log Analyzer

## Quick Commands Reference

### ?? Running the Application

```powershell
# Start the application (Development)
dotnet run

# Start with specific environment
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run

# Start and open browser automatically
Start-Process "https://localhost:5001"
dotnet run
```

---

### ?? Build Commands

```powershell
# Clean build artifacts
dotnet clean

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Build in Release mode
dotnet build -c Release

# Full rebuild (clean + restore + build)
dotnet clean
dotnet restore
dotnet build
```

---

### ?? Publish Commands

```powershell
# Publish for production
dotnet publish -c Release -o ./publish

# Publish with specific runtime (Windows x64)
dotnet publish -c Release -r win-x64 --self-contained -o ./publish

# Publish with specific runtime (Linux x64)
dotnet publish -c Release -r linux-x64 --self-contained -o ./publish

# Run published version
cd publish
dotnet LogAnalyzer.API.dll
```

---

### ?? Testing Commands

```powershell
# Check if app is running
Test-NetConnection -ComputerName localhost -Port 5001

# Test API health endpoint
Invoke-RestMethod -Uri "https://localhost:5001/api/LogAnalyzer/health" -SkipCertificateCheck

# Test with sample log
$body = @{
    logs = "2024-03-18 [ERROR] Test error"
    logType = "dotnet"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/LogAnalyzer/analyze" `
    -Method Post `
    -Body $body `
    -ContentType "application/json" `
    -SkipCertificateCheck
```

---

### ?? Information Commands

```powershell
# Check .NET version
dotnet --version

# List installed SDKs
dotnet --list-sdks

# List installed runtimes
dotnet --list-runtimes

# Get project info
dotnet list package

# Check for outdated packages
dotnet list package --outdated
```

---

### ?? Configuration Management

```powershell
# Set environment variable (PowerShell)
$env:Gemini__ApiKey = "YOUR_API_KEY_HERE"
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Set environment variable (permanently - Windows)
[System.Environment]::SetEnvironmentVariable("Gemini__ApiKey", "YOUR_API_KEY", "User")

# Read current environment
Get-ChildItem Env: | Where-Object {$_.Name -like "*ASPNETCORE*"}
```

---

### ?? File Management

```powershell
# List all project files
Get-ChildItem -Recurse -Exclude bin,obj

# Find specific file
Get-ChildItem -Recurse -Filter "*.cs"

# Count lines of code
Get-ChildItem -Recurse -Filter "*.cs" | Get-Content | Measure-Object -Line

# Open project in VS Code
code .

# Open project in Visual Studio
start LogAnalyzer.API.csproj
```

---

### ??? Cleanup Commands

```powershell
# Remove bin and obj folders
Get-ChildItem -Path . -Include bin,obj -Recurse -Force | Remove-Item -Recurse -Force

# Clean NuGet cache
dotnet nuget locals all --clear

# Remove all build artifacts
dotnet clean
Remove-Item -Path ./bin -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ./obj -Recurse -Force -ErrorAction SilentlyContinue
```

---

### ?? Network & Port Commands

```powershell
# Check if port is in use
Get-NetTCPConnection -LocalPort 5001 -ErrorAction SilentlyContinue

# Kill process using port 5001
$processId = (Get-NetTCPConnection -LocalPort 5001).OwningProcess
Stop-Process -Id $processId -Force

# Find process by name
Get-Process -Name "LogAnalyzer.API" -ErrorAction SilentlyContinue
```

---

### ?? Monitoring Commands

```powershell
# Watch logs in real-time (if using file logging)
Get-Content -Path "logs/app.log" -Wait -Tail 50

# Monitor CPU and memory usage
while($true) {
    $process = Get-Process -Name "LogAnalyzer.API" -ErrorAction SilentlyContinue
    if($process) {
        Write-Host "CPU: $($process.CPU) | Memory: $($process.WorkingSet64 / 1MB) MB"
    }
    Start-Sleep -Seconds 2
}

# Check application status
$response = Invoke-RestMethod -Uri "https://localhost:5001/api/LogAnalyzer/health" -SkipCertificateCheck
Write-Host "Status: $($response.status) | Time: $($response.timestamp)"
```

---

### ?? Hot Reload Commands

```powershell
# Run with hot reload (auto-restart on file changes)
dotnet watch run

# Run specific file with hot reload
dotnet watch run --project LogAnalyzer.API.csproj
```

---

### ?? Docker Commands (Optional)

```powershell
# Build Docker image
docker build -t loganalyzer-api .

# Run Docker container
docker run -p 5000:5000 -p 5001:5001 loganalyzer-api

# Run with environment variable
docker run -p 5000:5000 -e Gemini__ApiKey="YOUR_KEY" loganalyzer-api

# View running containers
docker ps

# View logs
docker logs <container_id>

# Stop container
docker stop <container_id>
```

---

### ?? Git Commands (Version Control)

```powershell
# Initialize repository (if not already)
git init

# Add all files
git add .

# Commit changes
git commit -m "Initial commit - AI Log Analyzer"

# Create .gitignore (if not exists)
@"
bin/
obj/
.vs/
.vscode/
*.user
*.suo
appsettings.Development.json
"@ | Out-File -FilePath .gitignore -Encoding UTF8

# Check status
git status

# View history
git log --oneline

# Create branch
git checkout -b feature/new-feature
```

---

### ?? Troubleshooting Commands

```powershell
# Check if port is available
Test-NetConnection -ComputerName localhost -Port 5001

# Clear NuGet cache and rebuild
dotnet nuget locals all --clear
dotnet clean
dotnet restore
dotnet build

# Check for binding redirects issues
dotnet list package --include-transitive

# Verify SDK and runtime
dotnet --info

# Check for certificate issues
dotnet dev-certs https --check
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

---

### ?? Quick Start Scripts

#### Run Development
```powershell
# run-dev.ps1
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet restore
dotnet build
Start-Process "https://localhost:5001"
dotnet run
```

#### Deploy to Production
```powershell
# deploy.ps1
dotnet clean
dotnet restore
dotnet publish -c Release -o ./publish
Write-Host "Build complete. Files in ./publish folder"
```

#### Full Reset
```powershell
# reset.ps1
Write-Host "Cleaning project..."
dotnet clean
Remove-Item -Path ./bin -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ./obj -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "Clearing NuGet cache..."
dotnet nuget locals all --clear
Write-Host "Restoring packages..."
dotnet restore
Write-Host "Building project..."
dotnet build
Write-Host "Reset complete!"
```

---

### ?? Package Management

```powershell
# Add new package
dotnet add package PackageName

# Add specific version
dotnet add package PackageName --version 1.0.0

# Update package
dotnet add package PackageName --version 2.0.0

# Remove package
dotnet remove package PackageName

# List all packages
dotnet list package

# Check for security vulnerabilities
dotnet list package --vulnerable
```

---

### ?? Custom Aliases (Add to PowerShell Profile)

```powershell
# Open PowerShell profile
notepad $PROFILE

# Add these lines:
function Start-LogAnalyzer { dotnet run }
function Build-LogAnalyzer { dotnet build }
function Test-LogAnalyzer { Invoke-RestMethod -Uri "https://localhost:5001/api/LogAnalyzer/health" -SkipCertificateCheck }
function Clean-LogAnalyzer { dotnet clean; Remove-Item -Path ./bin,./obj -Recurse -Force -ErrorAction SilentlyContinue }

# Set aliases
Set-Alias -Name run -Value Start-LogAnalyzer
Set-Alias -Name build -Value Build-LogAnalyzer
Set-Alias -Name test -Value Test-LogAnalyzer
Set-Alias -Name clean -Value Clean-LogAnalyzer

# Usage:
# run    -> starts the app
# build  -> builds the project
# test   -> tests health endpoint
# clean  -> cleans the project
```

---

### ?? Pro Tips

```powershell
# Open multiple terminals for different tasks
Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$PWD'; dotnet watch run"

# Create quick backup
$date = Get-Date -Format "yyyyMMdd_HHmmss"
Compress-Archive -Path . -DestinationPath "../LogAnalyzer_Backup_$date.zip" -Force

# Find TODO comments in code
Get-ChildItem -Recurse -Filter "*.cs" | Select-String -Pattern "TODO|FIXME|HACK"

# Count errors in code
Get-ChildItem -Recurse -Filter "*.cs" | Get-Content | Select-String -Pattern "throw new" | Measure-Object

# Generate project statistics
$csFiles = Get-ChildItem -Recurse -Filter "*.cs"
$totalLines = $csFiles | Get-Content | Measure-Object -Line
Write-Host "C# Files: $($csFiles.Count)"
Write-Host "Total Lines: $($totalLines.Lines)"
```

---

### ?? One-Line Super Commands

```powershell
# Full clean rebuild and run
dotnet clean; dotnet restore; dotnet build; dotnet run

# Quick test
Invoke-RestMethod -Uri "https://localhost:5001/api/LogAnalyzer/health" -SkipCertificateCheck

# Build and open in browser
dotnet build; Start-Process "https://localhost:5001"; dotnet run

# Check everything is working
dotnet --version; dotnet restore; dotnet build; Write-Host "? All good!"
```

---

## ?? Useful Resources

- **PowerShell Documentation:** https://docs.microsoft.com/powershell/
- **.NET CLI Documentation:** https://docs.microsoft.com/dotnet/core/tools/
- **ASP.NET Core Documentation:** https://docs.microsoft.com/aspnet/core/

---

**Save this file for quick reference during development! ??**
