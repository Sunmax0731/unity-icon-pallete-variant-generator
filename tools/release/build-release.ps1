param(
    [string]$Version = "1.0.1"
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$releaseRoot = Join-Path $repoRoot "ReleaseBuilds"
$zipPath = Join-Path $releaseRoot "PaletteVariantGenerator_v$Version.zip"

New-Item -ItemType Directory -Force -Path $releaseRoot | Out-Null
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

$trackedPaths = @(
    "Packages/com.sunmax0731.icon-palette-variant-generator",
    "docs",
    "README.md",
    "CHANGELOG.md",
    "Agents.md",
    "Skill.md"
)

Push-Location $repoRoot
try {
    & git archive --format=zip --output $zipPath HEAD -- $trackedPaths
    if ($LASTEXITCODE -ne 0) {
        throw "git archive failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}

Write-Host "Release package created: $zipPath"
