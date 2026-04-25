param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$releaseRoot = Join-Path $repoRoot "ReleaseBuilds"
$stagingRoot = Join-Path $releaseRoot "PaletteVariantGenerator_v$Version"
$zipPath = Join-Path $releaseRoot "PaletteVariantGenerator_v$Version.zip"

if (Test-Path -LiteralPath $stagingRoot) {
    Remove-Item -LiteralPath $stagingRoot -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $stagingRoot | Out-Null
New-Item -ItemType Directory -Force -Path $releaseRoot | Out-Null

$stagingPackages = Join-Path $stagingRoot "Packages"
New-Item -ItemType Directory -Force -Path $stagingPackages | Out-Null
Copy-Item `
    -LiteralPath (Join-Path $repoRoot "Packages/com.sunmax0731.icon-palette-variant-generator") `
    -Destination $stagingPackages `
    -Recurse
Copy-Item -LiteralPath (Join-Path $repoRoot "docs") -Destination (Join-Path $stagingRoot "docs") -Recurse
Copy-Item -LiteralPath (Join-Path $repoRoot "README.md") -Destination $stagingRoot
Copy-Item -LiteralPath (Join-Path $repoRoot "CHANGELOG.md") -Destination $stagingRoot
Copy-Item -LiteralPath (Join-Path $repoRoot "Agents.md") -Destination $stagingRoot
Copy-Item -LiteralPath (Join-Path $repoRoot "Skill.md") -Destination $stagingRoot

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -Path (Join-Path $stagingRoot "*") -DestinationPath $zipPath -Force
Write-Host "Release package created: $zipPath"
