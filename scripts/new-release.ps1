param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $root "src\SignalDeck.App\SignalDeck.App.csproj"

if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Version must be in semantic version format like 0.3.0"
}

[xml]$projectXml = Get-Content $projectPath
$propertyGroup = $projectXml.Project.PropertyGroup | Select-Object -First 1
$propertyGroup.Version = $Version
$propertyGroup.FileVersion = "$Version.0"
$propertyGroup.AssemblyVersion = "$Version.0"
$projectXml.Save($projectPath)

git -C $root diff --quiet
$hasPendingChangesBeforeCommit = $LASTEXITCODE -ne 0

git -C $root add $projectPath
git -C $root commit -m "Release v$Version"

if ($LASTEXITCODE -ne 0) {
    throw "Failed to create release commit."
}

git -C $root tag "v$Version"

if ($LASTEXITCODE -ne 0) {
    throw "Failed to create git tag v$Version."
}

Write-Host ""
Write-Host "Release prepared locally."
Write-Host "Next commands:"
Write-Host "  git -C `"$root`" push Origin main"
Write-Host "  git -C `"$root`" push Origin v$Version"
Write-Host ""
Write-Host "After the tag push, GitHub Actions will build the installer and publish:"
Write-Host "  SignalDeckSetup-$Version.exe"
if ($hasPendingChangesBeforeCommit) {
    Write-Host ""
    Write-Host "Note: there were already local changes before the release commit."
}
