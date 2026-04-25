# Skill.md - Unity Icon Palette Variant Generator Development Skill

## Goal

Develop and maintain the Unity Icon Palette Variant Generator as a non-destructive Unity Editor extension for creating icon color variants from palette analysis.

## Required Context

Before implementation, inspect:

- GitHub open Issues for priority
- `unity_icon_palette_variant_generator_docs/requirements.md`
- `unity_icon_palette_variant_generator_docs/specification.md`
- `unity_icon_palette_variant_generator_docs/architecture.md`
- `unity_icon_palette_variant_generator_docs/development_plan.md`
- Existing code under `Packages/com.sunmax0731.icon-palette-variant-generator`

## Safety Rules

- Never overwrite the source image.
- Export generated PNGs to a separate output path.
- Preserve alpha unless the user explicitly asks otherwise.
- Keep session JSON object-reference-free.
- Do not commit local QA images or generated outputs unless they are deliberate samples for #8.
- Treat untracked files under `Assets/` as user QA artifacts unless the current task says otherwise.

## Architecture Rules

- Models: serializable data under `Runtime/Models`.
- Services: image analysis, grouping, replacement, export, and session IO under `Runtime/Services`.
- Editor services: Unity asset loading and editor-only helpers under `Editor/Services`.
- UI: `PaletteVariantGeneratorWindow` should orchestrate user actions, not own core algorithms.
- Validation: keep headless smoke checks in `Editor/Validation`.

## Feature Behavior

- Palette extraction excludes pixels at or below `Alpha Threshold`.
- Quantization should reduce near-identical anti-aliased colors.
- `Max Color Distance` controls whether grouped colors remain together or split into outliers.
- Replacement mode behavior:
  - `GroupUniform`: group target applies to all entries in the group
  - `PerColor`: only enabled color-entry rules apply
  - `Hybrid`: enabled color-entry rules override the group fallback
- Replacement formula is:

```text
output = Lerp(original, target, blendRatio)
```

## Quality Gate

For code changes, run the Unity validation script with Unity `6000.4.0f1`:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

When adding service behavior, add or update focused EditMode tests under:

```text
Packages/com.sunmax0731.icon-palette-variant-generator/Tests/Editor
```

## Manual QA Checklist

Use #8 to record manual validation for:

- 64x64 transparent PNG
- 128x128 transparent PNG
- anti-aliased icon
- pixel-art icon
- transparent area remains transparent
- Analyze, Auto Group, Preview, Export
- Save Session and Load Session
- language menu and Help window
- scrollable Palette list
- `Max Color Distance`
- per-color, group uniform, and hybrid replacement modes

## Release Readiness

Do not publish `v1.0.0` until:

- #7 variation and batch export is complete
- #8 manual QA checklist is complete
- README, CHANGELOG, Manual, Terms, release notes, and BOOTH copy are prepared
- package or release ZIP is installed and validated from a clean import path
