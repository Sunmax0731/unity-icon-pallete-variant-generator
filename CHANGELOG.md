# Changelog

## 1.0.0 - 2026-04-25

Initial release.

### Added

- Palette extraction from Unity project PNG / Texture2D assets.
- Alpha threshold, minimum pixel count, quantize step, and max palette color settings.
- Nearby-color grouping with target group count and max color distance.
- Group uniform, per-color, and hybrid replacement modes.
- Before / After preview with checkerboard background and selection overlay.
- Scrollable palette list.
- Multiple variation management with Add, Duplicate, Remove, active selection, and export enable flags.
- Single PNG export and batch export for enabled variations.
- Session JSON save / load.
- Help window and language mode control.
- UPM sample icons for validation.
- Unity `6000.4.0f1` validation gate.

### Known Limitations

- UI is IMGUI-based.
- Color distance is RGB-based.
- Direct SpriteAtlas editing is not included.
- Folder-wide batch processing is not included.
