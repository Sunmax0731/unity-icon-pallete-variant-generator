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

if (-not (Select-String -Path $logPath -Pattern "ISSUE8_SAMPLE_QA_VALIDATION=PASS" -Quiet)) {
    throw "Unity sample QA validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE10_UI_POLISH_VALIDATION=PASS" -Quiet)) {
    throw "Unity UI polish validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE12_VARIATION_UX_VALIDATION=PASS" -Quiet)) {
    throw "Unity variation UX validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE13_AUTO_PREVIEW_DEBOUNCE_VALIDATION=PASS" -Quiet)) {
    throw "Unity auto preview debounce validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE17_PREVIEW_NAVIGATION_VALIDATION=PASS" -Quiet)) {
    throw "Unity preview navigation validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE18_RULE_PRESET_VALIDATION=PASS" -Quiet)) {
    throw "Unity rule preset validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE19_MANUAL_GROUP_EDITING_VALIDATION=PASS" -Quiet)) {
    throw "Unity manual group editing validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE20_COLOR_DISTANCE_MODE_VALIDATION=PASS" -Quiet)) {
    throw "Unity color distance mode validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE21_FOLDER_BATCH_EXPORT_VALIDATION=PASS" -Quiet)) {
    throw "Unity folder batch export validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE22_SCRIPTABLE_OBJECT_PRESET_VALIDATION=PASS" -Quiet)) {
    throw "Unity ScriptableObject preset validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE23_DOCKED_LAYOUT_VALIDATION=PASS" -Quiet)) {
    throw "Unity docked layout validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE23_UI_TOOLKIT_PREVIEW_VALIDATION=PASS" -Quiet)) {
    throw "Unity UI Toolkit preview validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE23_UI_TOOLKIT_INTERACTION_VALIDATION=PASS" -Quiet)) {
    throw "Unity UI Toolkit interaction validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE23_MAIN_WINDOW_UI_TOOLKIT_HOST_VALIDATION=PASS" -Quiet)) {
    throw "Unity main window UI Toolkit host validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE25_PREVIEW_MENU_HIDDEN_VALIDATION=PASS" -Quiet)) {
    throw "Unity UI Toolkit preview menu hidden validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE25_UI_TOOLKIT_PRODUCTION_VALIDATION=PASS" -Quiet)) {
    throw "Unity production UI Toolkit validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE26_NOISE_REMOVAL_VALIDATION=PASS" -Quiet)) {
    throw "Unity noise removal validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE27_EDGE_OUTSIDE_CLEANUP_VALIDATION=PASS" -Quiet)) {
    throw "Unity edge outside cleanup validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE28_EXPORT_UI_DISCLOSURE_VALIDATION=PASS" -Quiet)) {
    throw "Unity export UI disclosure validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE29_OPAQUE_EDGE_OUTSIDE_CLEANUP_VALIDATION=PASS" -Quiet)) {
    throw "Unity opaque edge outside cleanup validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE30_EXPORT_SETTINGS_WINDOW_VALIDATION=PASS" -Quiet)) {
    throw "Unity export settings window validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE31_PREVIEW_COLOR_PICK_VALIDATION=PASS" -Quiet)) {
    throw "Unity preview color pick validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE32_SELECTION_HIGHLIGHT_VALIDATION=PASS" -Quiet)) {
    throw "Unity selection highlight validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE33_UI_TOOLKIT_PREVIEW_ZOOM_VALIDATION=PASS" -Quiet)) {
    throw "Unity UI Toolkit preview zoom validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE34_UI_TOOLKIT_PREVIEW_DRAG_PAN_VALIDATION=PASS" -Quiet)) {
    throw "Unity UI Toolkit preview drag pan validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE35_PREVIEW_DISPLAY_CACHE_VALIDATION=PASS" -Quiet)) {
    throw "Unity preview display cache validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE36_DELAYED_PREVIEW_REFRESH_VALIDATION=PASS" -Quiet)) {
    throw "Unity delayed preview refresh validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE37_SELECTED_COLOR_INFO_VALIDATION=PASS" -Quiet)) {
    throw "Unity selected color info validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE38_PALETTE_RULE_STATUS_VALIDATION=PASS" -Quiet)) {
    throw "Unity palette rule status validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE39_EFFECT_HIGHLIGHT_VALIDATION=PASS" -Quiet)) {
    throw "Unity effect highlight validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE40_ANALYSIS_PRESET_VALIDATION=PASS" -Quiet)) {
    throw "Unity analysis preset validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE41_COLLAPSIBLE_SETTINGS_VALIDATION=PASS" -Quiet)) {
    throw "Unity collapsible settings validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE42_PREVIEW_MINI_TOOLBAR_VALIDATION=PASS" -Quiet)) {
    throw "Unity preview mini toolbar validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE43_UNDO_REDO_VALIDATION=PASS" -Quiet)) {
    throw "Unity undo redo validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE44_DIFFERENCE_PREVIEW_VALIDATION=PASS" -Quiet)) {
    throw "Unity difference preview validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE45_EXPORT_PRECHECK_VALIDATION=PASS" -Quiet)) {
    throw "Unity export precheck validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE46_BOUNDARY_TRIM_VALIDATION=PASS" -Quiet)) {
    throw "Unity boundary trim validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE47_PREVIEW_BRUSH_SELECTION_VALIDATION=PASS" -Quiet)) {
    throw "Unity preview brush selection validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "FOLLOWUP_PREVIEW_VISIBILITY_HELP_VALIDATION=PASS" -Quiet)) {
    throw "Unity preview visibility/help follow-up validation did not emit pass marker. Log: $logPath"
}

if (-not (Select-String -Path $logPath -Pattern "ISSUE24_RELEASE_AUTOMATION_VALIDATION=PASS" -Quiet)) {
    throw "Unity release automation validation did not emit pass marker. Log: $logPath"
}

Set-Content -LiteralPath $resultsPath -Value @(
    "ISSUE1_SCAFFOLD_VALIDATION=PASS",
    "ISSUE2_IMAGE_PALETTE_VALIDATION=PASS",
    "ISSUE3_COLOR_GROUPING_VALIDATION=PASS",
    "ISSUE4_REPLACEMENT_PREVIEW_VALIDATION=PASS",
    "ISSUE5_PNG_EXPORT_VALIDATION=PASS",
    "ISSUE6_SESSION_JSON_VALIDATION=PASS",
    "ISSUE7_VARIATION_BATCH_EXPORT_VALIDATION=PASS",
    "ISSUE8_SAMPLE_QA_VALIDATION=PASS",
    "ISSUE10_UI_POLISH_VALIDATION=PASS",
    "ISSUE12_VARIATION_UX_VALIDATION=PASS",
    "ISSUE13_AUTO_PREVIEW_DEBOUNCE_VALIDATION=PASS",
    "ISSUE17_PREVIEW_NAVIGATION_VALIDATION=PASS",
    "ISSUE18_RULE_PRESET_VALIDATION=PASS",
    "ISSUE19_MANUAL_GROUP_EDITING_VALIDATION=PASS",
    "ISSUE20_COLOR_DISTANCE_MODE_VALIDATION=PASS",
    "ISSUE21_FOLDER_BATCH_EXPORT_VALIDATION=PASS",
    "ISSUE22_SCRIPTABLE_OBJECT_PRESET_VALIDATION=PASS",
    "ISSUE23_DOCKED_LAYOUT_VALIDATION=PASS",
    "ISSUE23_UI_TOOLKIT_PREVIEW_VALIDATION=PASS",
    "ISSUE23_UI_TOOLKIT_INTERACTION_VALIDATION=PASS",
    "ISSUE23_MAIN_WINDOW_UI_TOOLKIT_HOST_VALIDATION=PASS",
    "ISSUE25_PREVIEW_MENU_HIDDEN_VALIDATION=PASS",
    "ISSUE25_UI_TOOLKIT_PRODUCTION_VALIDATION=PASS",
    "ISSUE26_NOISE_REMOVAL_VALIDATION=PASS",
    "ISSUE27_EDGE_OUTSIDE_CLEANUP_VALIDATION=PASS",
    "ISSUE28_EXPORT_UI_DISCLOSURE_VALIDATION=PASS",
    "ISSUE29_OPAQUE_EDGE_OUTSIDE_CLEANUP_VALIDATION=PASS",
    "ISSUE30_EXPORT_SETTINGS_WINDOW_VALIDATION=PASS",
    "ISSUE31_PREVIEW_COLOR_PICK_VALIDATION=PASS",
    "ISSUE32_SELECTION_HIGHLIGHT_VALIDATION=PASS",
    "ISSUE33_UI_TOOLKIT_PREVIEW_ZOOM_VALIDATION=PASS",
    "ISSUE34_UI_TOOLKIT_PREVIEW_DRAG_PAN_VALIDATION=PASS",
    "ISSUE35_PREVIEW_DISPLAY_CACHE_VALIDATION=PASS",
    "ISSUE36_DELAYED_PREVIEW_REFRESH_VALIDATION=PASS",
    "ISSUE37_SELECTED_COLOR_INFO_VALIDATION=PASS",
    "ISSUE38_PALETTE_RULE_STATUS_VALIDATION=PASS",
    "ISSUE39_EFFECT_HIGHLIGHT_VALIDATION=PASS",
    "ISSUE40_ANALYSIS_PRESET_VALIDATION=PASS",
    "ISSUE41_COLLAPSIBLE_SETTINGS_VALIDATION=PASS",
    "ISSUE42_PREVIEW_MINI_TOOLBAR_VALIDATION=PASS",
    "ISSUE43_UNDO_REDO_VALIDATION=PASS",
    "ISSUE44_DIFFERENCE_PREVIEW_VALIDATION=PASS",
    "ISSUE45_EXPORT_PRECHECK_VALIDATION=PASS",
    "ISSUE46_BOUNDARY_TRIM_VALIDATION=PASS",
    "ISSUE47_PREVIEW_BRUSH_SELECTION_VALIDATION=PASS",
    "FOLLOWUP_PREVIEW_VISIBILITY_HELP_VALIDATION=PASS",
    "ISSUE24_RELEASE_AUTOMATION_VALIDATION=PASS"
) -Encoding UTF8
Write-Host "Unity scaffold validation completed: $resultsPath"
