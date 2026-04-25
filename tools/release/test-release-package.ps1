param(
    [string]$Version = "1.0.2"
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$zipPath = Join-Path $repoRoot "ReleaseBuilds/PaletteVariantGenerator_v$Version.zip"

Add-Type -AssemblyName System.IO.Compression.FileSystem

if (-not (Test-Path -LiteralPath $zipPath)) {
    throw "Release ZIP was not found: $zipPath"
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
