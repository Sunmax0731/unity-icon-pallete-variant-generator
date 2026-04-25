param(
    [string]$UnityPath = "C:\Program Files\Unity\6000.4.0f1\Editor\Unity.exe"
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$validationDir = Join-Path $repoRoot "Validation"
$logsDir = Join-Path $repoRoot "Logs"
$resultsPath = Join-Path $validationDir "scaffold-validation.txt"
$logPath = Join-Path $logsDir "editmode-tests.log"

New-Item -ItemType Directory -Force -Path $validationDir | Out-Null
New-Item -ItemType Directory -Force -Path $logsDir | Out-Null

if (-not (Test-Path -LiteralPath $UnityPath)) {
    throw "Unity executable not found: $UnityPath"
}

$arguments = @(
    "-batchmode",
    "-projectPath", $repoRoot,
    "-executeMethod", "Sunmax0731.IconPaletteVariantGenerator.Editor.Validation.PaletteVariantGeneratorValidation.RunScaffoldValidation",
    "-logFile", $logPath,
    "-quit"
)

$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru
if ($process.ExitCode -ne 0) {
    throw "Unity scaffold validation failed with exit code $($process.ExitCode). Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE1_SCAFFOLD_VALIDATION=PASS" -Quiet)) {
    throw "Unity scaffold validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE2_IMAGE_PALETTE_VALIDATION=PASS" -Quiet)) {
    throw "Unity image palette validation did not emit pass marker. Log: $logPath"
}

Set-Content -LiteralPath $resultsPath -Value @(
    "ISSUE1_SCAFFOLD_VALIDATION=PASS",
    "ISSUE2_IMAGE_PALETTE_VALIDATION=PASS"
) -Encoding UTF8
Write-Host "Unity scaffold validation completed: $resultsPath"
