# Agents.md - Unity Icon Palette Variant Generator

## Purpose

This repository contains a Unity Editor extension that extracts palette colors from an icon image, groups nearby colors, previews replacements, and exports PNG color variants without modifying the source asset.

## Current Workflow

- Work issue-first. Check open GitHub Issues before choosing implementation work.
- Use one focused branch per implementation issue.
- Validate with Unity `6000.4.0f1` before committing or closing an issue.
- Push completed work to `master` after validation unless the user requests a PR flow.
- Leave user-created local sample images untracked unless a task explicitly asks to add samples.

## Product Shape

- Unity project root is used for validation.
- Package code lives under `Packages/com.sunmax0731.icon-palette-variant-generator`.
- Editor UI opens from:

```text
Tools > Icon Tools > Palette Variant Generator
```

## Implementation Rules

- Do not modify source image assets directly.
- Keep Editor-only code under `Editor`.
- Keep reusable models and services under `Runtime`.
- Do not store `UnityEngine.Object` references in JSON.
- Prefer serializable lists over dictionaries in saved session data.
- Destroy temporary `Texture2D` instances created for readable copies or previews.
- Keep heavy analysis and replacement logic out of `EditorWindow`; put it in services.

## UI Rules

- Follow the existing three-pane IMGUI layout:
  - top toolbar for source, Analyze, Auto Group, Preview, Export, session, Help, and language controls
  - left pane for analyze, group, export, and source settings
  - center pane for before/after preview and scrollable palette
  - right pane for group and color replacement rules
- Keep controls compact and Unity-editor-like.
- Preserve language and help affordances near the Analyze workflow.
- Selection in palette/group lists should remain visible in the preview overlay.

## Validation

Run:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

Expected markers include:

```text
ISSUE1_SCAFFOLD_VALIDATION=PASS
ISSUE2_IMAGE_PALETTE_VALIDATION=PASS
ISSUE3_COLOR_GROUPING_VALIDATION=PASS
ISSUE4_REPLACEMENT_PREVIEW_VALIDATION=PASS
ISSUE5_PNG_EXPORT_VALIDATION=PASS
ISSUE6_SESSION_JSON_VALIDATION=PASS
ISSUE10_UI_POLISH_VALIDATION=PASS
```

## Remaining Issue Order

1. `#7` Multiple icon variations and batch export
2. `#8` Manual QA, sample assets, and validation checklist
3. `#9` Release packaging, documentation, and GitHub Release `v1.0.0`

Do not start release packaging before #7 and #8 are complete.
