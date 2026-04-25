# Unity Icon Palette Variant Generator

Unity Editor extension for generating palette-based color variants from icon images.

## Unity Version

- Unity 6000.4.0f1

## Current Scope

The current implementation is the Phase 0 scaffold:

- Embedded UPM package under `Packages/com.sunmax0731.icon-palette-variant-generator`
- Runtime model and enum baseline
- Editor-only assembly
- Empty EditorWindow opened from `Tools > Icon Tools > Palette Variant Generator`

## Open

In Unity:

```text
Tools > Icon Tools > Palette Variant Generator
```

## Validation

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

Expected marker:

```text
ISSUE1_SCAFFOLD_VALIDATION=PASS
ISSUE2_IMAGE_PALETTE_VALIDATION=PASS
ISSUE3_COLOR_GROUPING_VALIDATION=PASS
```

## Planning Docs

The planning baseline is under `unity_icon_palette_variant_generator_docs/`.
