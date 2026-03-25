#Requires -Version 5.1
[CmdletBinding()]
param(
    [switch]$SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$SolutionRoot = Split-Path -Parent $PSScriptRoot

# 1. Read version
$versionJson = Get-Content "$SolutionRoot/version.json" -Raw | ConvertFrom-Json
$version = "$($versionJson.major).$($versionJson.minor).$($versionJson.patch)"
$assemblyVersion = "$version.0"
Write-Host "Building version $version ..." -ForegroundColor Cyan

# 2. Run tests
if (-not $SkipTests) {
    Write-Host "`nRunning tests..." -ForegroundColor Cyan
    dotnet test "$SolutionRoot/VirtualScreenManager.sln" --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "Tests failed." }
}

# 3. Publish
$publishDir = "$SolutionRoot/publish/win-x64"
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }

Write-Host "`nPublishing self-contained win-x64..." -ForegroundColor Cyan
dotnet publish "$SolutionRoot/src/VirtualScreenManager.UI/VirtualScreenManager.UI.csproj" `
    --configuration Release `
    --runtime win-x64 `
    --self-contained `
    --output $publishDir `
    /p:Version=$version `
    /p:AssemblyVersion=$assemblyVersion `
    /p:FileVersion=$assemblyVersion

if ($LASTEXITCODE -ne 0) { throw "Publish failed." }

# 4. Build installer
$iscc = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $iscc)) { throw "Inno Setup 6 not found at $iscc. Install from https://jrsoftware.org/isinfo.php" }

$issFile = "$SolutionRoot/installer/VirtualScreenManager.iss"
$artifactsDir = "$SolutionRoot/artifacts"
if (-not (Test-Path $artifactsDir)) { New-Item -ItemType Directory -Path $artifactsDir | Out-Null }

Write-Host "`nBuilding installer..." -ForegroundColor Cyan
& $iscc /DAppVersion=$version `
        /DPublishDir=$publishDir `
        /DOutputDir=$artifactsDir `
        $issFile

if ($LASTEXITCODE -ne 0) { throw "Inno Setup compilation failed." }

Write-Host "`nInstaller created: $artifactsDir/VirtualScreenManager-$version-Setup.exe" -ForegroundColor Green
