# Unity Icon Palette Variant Generator

Unity Editor extension for generating palette-based color variants from icon images.

## Unity Version

- Unity 6000.4.0f1

## Package

- Package: `com.sunmax0731.icon-palette-variant-generator`
- Version: `1.0.0`
- Distribution: UPM package ZIP

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
- Multiple variations and batch export
- Toolbar Help and language setting controls

## Open

In Unity:

```text
Tools > Palette Variant Generator > 開く
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
ISSUE7_VARIATION_BATCH_EXPORT_VALIDATION=PASS
ISSUE8_SAMPLE_QA_VALIDATION=PASS
ISSUE10_UI_POLISH_VALIDATION=PASS
```

## Documentation

Project planning and product documentation live under `docs/`.

- Manual: `docs/manual.md`
- Terms: `docs/terms.md`
- Release notes: `docs/release-notes-v1.0.0.md`
- BOOTH copy draft: `docs/booth-copy.md`

## Release Build

```powershell
powershell -ExecutionPolicy Bypass -File tools\release\build-release.ps1 -Version 1.0.0
```

Output:

```text
ReleaseBuilds/PaletteVariantGenerator_v1.0.0.zip
```

## Samples

Validation sample icons live under:

```text
Packages/com.sunmax0731.icon-palette-variant-generator/Samples~/SampleIcons
```

## Known Limitations

- UI is IMGUI-based for the first release.
- Color distance is RGB-based.
- Direct SpriteAtlas editing and folder-wide batch processing are out of scope for v1.0.0.

## Agent Docs

- `Agents.md`
- `Skill.md`
