param(
    [string]$Version = "1.1.0",
    [string]$UnityPath = "C:\Program Files\Unity\6000.4.0f1\Editor\Unity.exe",
    [switch]$SkipUnityPackage
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$releaseRoot = Join-Path $repoRoot "ReleaseBuilds"
$zipPath = Join-Path $releaseRoot "PaletteVariantGenerator_v$Version.zip"
$unityPackagePath = Join-Path $releaseRoot "PaletteVariantGenerator_v$Version.unitypackage"
$unityPackageLogPath = Join-Path $releaseRoot "PaletteVariantGenerator_v$Version-unitypackage.log"
$packageRoot = "Packages/com.sunmax0731.icon-palette-variant-generator"

New-Item -ItemType Directory -Force -Path $releaseRoot | Out-Null
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

if (Test-Path -LiteralPath $unityPackagePath) {
    Remove-Item -LiteralPath $unityPackagePath -Force
}

$trackedPaths = @(
    $packageRoot,
    "docs",
    "README.md",
    "LICENSE.md",
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

if (-not $SkipUnityPackage) {
    if (-not (Test-Path -LiteralPath $UnityPath)) {
        throw "Unity executable not found: $UnityPath"
    }

    if (Test-Path -LiteralPath $unityPackageLogPath) {
        Remove-Item -LiteralPath $unityPackageLogPath -Force
    }

    $arguments = @(
        "-batchmode",
        "-quit",
        "-nographics",
        "-projectPath", $repoRoot,
        "-exportPackage", $packageRoot, $unityPackagePath,
        "-logFile", $unityPackageLogPath
    )

    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru -WindowStyle Hidden
    if ($process.ExitCode -ne 0) {
        throw "UnityPackage export failed with exit code $($process.ExitCode). Log: $unityPackageLogPath"
    }

    if (-not (Test-Path -LiteralPath $unityPackagePath)) {
        throw "UnityPackage was not created: $unityPackagePath"
    }

    Write-Host "UnityPackage created: $unityPackagePath"
}
