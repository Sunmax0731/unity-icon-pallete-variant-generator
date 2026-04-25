# Unity Icon Palette Variant Generator

Unity Editor extension for generating palette-based color variants from icon images.

## Unity Version

- Unity 6000.4.0f1

## Current Scope

The current implementation covers the MVP editor workflow:

- Embedded UPM package under `Packages/com.sunmax0731.icon-palette-variant-generator`
- Image loading and palette extraction
- Automatic nearby-color grouping with adjustable max color distance
- Group uniform, per-color, and hybrid replacement rules
- Before / After preview with selection overlay
- Scrollable palette list
- PNG export
- Session JSON save / load
- Toolbar Help and language setting controls

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
ISSUE4_REPLACEMENT_PREVIEW_VALIDATION=PASS
ISSUE5_PNG_EXPORT_VALIDATION=PASS
ISSUE6_SESSION_JSON_VALIDATION=PASS
ISSUE10_UI_POLISH_VALIDATION=PASS
```

## Documentation

Project planning and product documentation live under `docs/`.

## Agent Docs

- `Agents.md`
- `Skill.md`
