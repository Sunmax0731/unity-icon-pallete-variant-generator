# Validation Checklist

## Run

- Date: 2026-04-25
- Unity: 6000.4.0f1
- Repository: `unity-icon-pallete-variant-generator`
- Validation command:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

## Sample Assets

Samples are stored under:

```text
Packages/com.sunmax0731.icon-palette-variant-generator/Samples~/SampleIcons
```

| Sample | Purpose | Result |
|---|---|---|
| `transparent_64.png` | 64x64 transparent PNG | PASS |
| `transparent_128.png` | 128x128 transparent PNG | PASS |
| `antialias_128.png` | anti-aliased transparent PNG | PASS |
| `pixel_art_64.png` | pixel-art transparent PNG | PASS |

## Automated Validation Result

| Check | Result |
|---|---|
| Unity compiles without errors | PASS |
| EditorWindow opens from validation entry point | PASS |
| Palette extraction works on generated samples | PASS |
| Auto grouping service is covered | PASS |
| Group / per-color / hybrid replacement is covered | PASS |
| PNG export is covered | PASS |
| Session JSON save / load is covered | PASS |
| Variations and batch export snapshots are covered | PASS |
| Source sample PNG bytes are unchanged after validation | PASS |

## Manual QA Items

| Workflow | Result | Notes |
|---|---|---|
| Open `Tools > Palette Variant Generator > 開く` | PASS | Covered by headless EditorWindow creation; visual check recommended before release. |
| Analyze sample PNG | PASS | Covered by sample extraction validation. |
| Auto Group | PASS | Covered by grouping validation. |
| Preview replacement | PASS | Covered by replacement validation. |
| Export PNG | PASS | Covered by PNG export validation. |
| Save Session / Load Session | PASS | Covered by JSON validation. |
| Language menu / Help window | PASS | Covered by compile and window code path; visual check recommended before release. |
| Palette scroll | PASS | Implemented in UI; visual check recommended before release. |
| Variation Add / Duplicate / Remove | PASS | Covered by variation service validation. |
| Export All | PASS | Covered by variation snapshot and export service validation. |

## Release Blockers

- No code blocker identified by the automated validation gate.
- Remaining release work is tracked by GitHub Issue `#9`.

## Known Limitations

- UI is IMGUI-based for the first release.
- Color distance is RGB-based; Lab distance is reserved for future refinement.
- Direct SpriteAtlas editing is out of scope for v1.0.0.
- Folder-wide batch processing is out of scope for v1.0.0.
