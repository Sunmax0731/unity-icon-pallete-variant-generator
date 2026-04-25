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

if (-not (Select-String -Path $logPath -Pattern "ISSUE3_COLOR_GROUPING_VALIDATION=PASS" -Quiet)) {
    throw "Unity color grouping validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE4_REPLACEMENT_PREVIEW_VALIDATION=PASS" -Quiet)) {
    throw "Unity replacement preview validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE5_PNG_EXPORT_VALIDATION=PASS" -Quiet)) {
    throw "Unity PNG export validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE6_SESSION_JSON_VALIDATION=PASS" -Quiet)) {
    throw "Unity session JSON validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE7_VARIATION_BATCH_EXPORT_VALIDATION=PASS" -Quiet)) {
    throw "Unity variation batch export validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE10_UI_POLISH_VALIDATION=PASS" -Quiet)) {
    throw "Unity UI polish validation did not emit pass marker. Log: $logPath"
}

Set-Content -LiteralPath $resultsPath -Value @(
    "ISSUE1_SCAFFOLD_VALIDATION=PASS",
    "ISSUE2_IMAGE_PALETTE_VALIDATION=PASS",
    "ISSUE3_COLOR_GROUPING_VALIDATION=PASS",
    "ISSUE4_REPLACEMENT_PREVIEW_VALIDATION=PASS",
    "ISSUE5_PNG_EXPORT_VALIDATION=PASS",
    "ISSUE6_SESSION_JSON_VALIDATION=PASS",
    "ISSUE7_VARIATION_BATCH_EXPORT_VALIDATION=PASS",
    "ISSUE10_UI_POLISH_VALIDATION=PASS"
) -Encoding UTF8
Write-Host "Unity scaffold validation completed: $resultsPath"
