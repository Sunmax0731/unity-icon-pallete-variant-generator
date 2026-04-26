param(
    [string]$Version = "1.0.3"
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$zipPath = Join-Path $repoRoot "ReleaseBuilds/PaletteVariantGenerator_v$Version.zip"
$unityPackagePath = Join-Path $repoRoot "ReleaseBuilds/PaletteVariantGenerator_v$Version.unitypackage"

Add-Type -AssemblyName System.IO.Compression.FileSystem

if (-not (Test-Path -LiteralPath $zipPath)) {
    throw "Release ZIP was not found: $zipPath"
}

if (-not (Test-Path -LiteralPath $unityPackagePath)) {
    throw "Release UnityPackage was not found: $unityPackagePath"
}

$unityPackageItem = Get-Item -LiteralPath $unityPackagePath
if ($unityPackageItem.Length -le 0) {
    throw "Release UnityPackage is empty: $unityPackagePath"
}

$entries = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $names = $entries.Entries | ForEach-Object { $_.FullName }
    $required = @(
        "Packages/com.sunmax0731.icon-palette-variant-generator/package.json",
        "Packages/com.sunmax0731.icon-palette-variant-generator/Editor/Windows/PaletteVariantGeneratorWindow.cs",
        "README.md",
        "CHANGELOG.md",
        "docs/manual.md",
        "docs/release-checklist.md",
        "docs/validation-checklist.md"
    )

    foreach ($path in $required) {
        if ($names -notcontains $path) {
            throw "Release ZIP is missing required file: $path"
        }
    }

    $blockedPrefixes = @(
        "Assets/",
        "Library/",
        "Logs/",
        "Temp/",
        "ReleaseBuilds/",
        "Validation/"
    )

    foreach ($name in $names) {
        foreach ($prefix in $blockedPrefixes) {
            if ($name.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "Release ZIP contains blocked path: $name"
            }
        }
    }
}
finally {
    $entries.Dispose()
}

Write-Host "Release ZIP validation completed: $zipPath"
Write-Host "Release UnityPackage validation completed: $unityPackagePath"
