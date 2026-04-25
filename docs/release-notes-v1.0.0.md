# Release Notes - v1.0.0

## Summary

Initial release of Unity Icon Palette Variant Generator.

This release provides a Unity Editor workflow for extracting palette colors from a source icon, grouping nearby colors, editing replacement rules, previewing the result, and exporting one or more PNG color variants.

## Included

- UPM package: `com.sunmax0731.icon-palette-variant-generator`
- Sample icons under `Samples~/SampleIcons`
- Manual: `docs/manual.md`
- Terms: `docs/terms.md`
- Changelog: `CHANGELOG.md`
- Validation checklist: `docs/validation-checklist.md`

## Validation

- Unity: `6000.4.0f1`
- Command:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

Markers:

```text
ISSUE1_SCAFFOLD_VALIDATION=PASS
ISSUE2_IMAGE_PALETTE_VALIDATION=PASS
ISSUE3_COLOR_GROUPING_VALIDATION=PASS
ISSUE4_REPLACEMENT_PREVIEW_VALIDATION=PASS
ISSUE5_PNG_EXPORT_VALIDATION=PASS
ISSUE6_SESSION_JSON_VALIDATION=PASS
ISSUE7_VARIATION_BATCH_EXPORT_VALIDATION=PASS
ISSUE8_SAMPLE_QA_VALIDATION=PASS
ISSUE10_UI_POLISH_VALIDATION=PASS
```

## Known Limitations

- UI is IMGUI-based.
- Color distance is RGB-based.
- Direct SpriteAtlas editing is not included.
- Folder-wide batch processing is not included.
