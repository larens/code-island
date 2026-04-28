# Code Island Windows Build Script
# Build, test, and package the Windows application

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [ValidateSet("x64", "ARM64")]
    [string]$Platform = "x64",

    [switch]$SkipTests,
    [switch]$Package
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$SolutionPath = Join-Path $ProjectRoot "CodeIsland.Windows.sln"
$ProjectPath = Join-Path $ProjectRoot "src\CodeIsland.Windows\CodeIsland.Windows.csproj"
$TestProjectPath = Join-Path $ProjectRoot "tests\CodeIsland.Windows.Tests\CodeIsland.Windows.Tests.csproj"
$PublishDir = Join-Path $ProjectRoot "publish"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Code Island Windows Build" -ForegroundColor Cyan
Write-Host "  Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "  Platform: $Platform" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Restore packages
Write-Host "`n[1/4] Restoring packages..." -ForegroundColor Yellow
dotnet restore $SolutionPath
if ($LASTEXITCODE -ne 0) { throw "Package restore failed" }

# Step 2: Build
Write-Host "`n[2/4] Building..." -ForegroundColor Yellow
dotnet build $SolutionPath -c $Configuration -p:Platform=$Platform --no-restore
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

# Step 3: Test
if (-not $SkipTests) {
    Write-Host "`n[3/4] Running tests..." -ForegroundColor Yellow
    dotnet test $TestProjectPath -c $Configuration --no-build
    if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
} else {
    Write-Host "`n[3/4] Skipping tests..." -ForegroundColor Yellow
}

# Step 4: Publish
Write-Host "`n[4/4] Publishing..." -ForegroundColor Yellow
$runtimeId = if ($Platform -eq "ARM64") { "win-arm64" } else { "win-x64" }
$publishPath = Join-Path $PublishDir "$runtimeId"

dotnet publish $ProjectPath `
    -c $Configuration `
    -r $runtimeId `
    --self-contained `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishPath

if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  Build complete!" -ForegroundColor Green
Write-Host "  Output: $publishPath" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
