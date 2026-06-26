param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$appProject = Join-Path $root "src\SignalDeck.App\SignalDeck.App.csproj"
$publishDir = Join-Path $root "dist\publish"
$outputDir = Join-Path $root "dist\installer"
$iconPath = Join-Path $root "assets\SignalDeck.ico"
$licensePath = Join-Path $root "assets\THIRD-PARTY-NOTICES.txt"
$innoScript = Join-Path $root "installer\SignalDeck.iss"
$projectXml = [xml](Get-Content $appProject)
$appVersion = $projectXml.Project.PropertyGroup.Version | Select-Object -First 1
$targetFramework = $projectXml.Project.PropertyGroup.TargetFramework | Select-Object -First 1
$installerBaseName = "SignalDeckSetup-$appVersion"

if ([string]::IsNullOrWhiteSpace($appVersion)) {
    throw "Could not determine app version from SignalDeck.App.csproj."
}

if ([string]::IsNullOrWhiteSpace($targetFramework)) {
    throw "Could not determine target framework from SignalDeck.App.csproj."
}

New-Item -ItemType Directory -Force -Path $publishDir, $outputDir | Out-Null
Get-ChildItem $outputDir -Filter "SignalDeckSetup*.exe" -ErrorAction SilentlyContinue | Remove-Item -Force

dotnet publish $appProject `
    -c $Configuration `
    -f $targetFramework `
    -r win-x64 `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed."
}

Copy-Item $iconPath -Destination (Join-Path $publishDir "SignalDeck.ico") -Force
Copy-Item $licensePath -Destination (Join-Path $publishDir "THIRD-PARTY-NOTICES.txt") -Force

$isccCandidates = @(
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe",
    (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe")
)

$isccPath = $isccCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $isccPath) {
    throw "Inno Setup compiler not found. Expected ISCC.exe in a standard Inno Setup 6 install location."
}

& $isccPath `
    "/DMyAppVersion=$appVersion" `
    "/DInstallerBaseName=$installerBaseName" `
    "/DPublishDir=$publishDir" `
    "/DOutputDir=$outputDir" `
    "/DIconPath=$iconPath" `
    $innoScript

if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup compilation failed."
}
