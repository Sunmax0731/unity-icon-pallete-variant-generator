# Manual

## Overview

Unity Icon Palette Variant Generator is a Unity Editor extension for creating color variants from icon PNGs without modifying the original source asset.

## Installation

Use one of the following methods.

### Git URL

Add the package from the Unity Package Manager using the repository URL.

### Release ZIP

Download `PaletteVariantGenerator_v1.0.0.zip` from GitHub Releases, extract it, and add the package folder to your Unity project.

## Open The Tool

```text
Tools > Icon Tools > Palette Variant Generator
```

## Basic Workflow

1. Select a PNG or Texture2D asset in `Source Image`.
2. Click `Analyze`.
3. Adjust palette extraction settings if needed.
4. Click `Auto Group`.
5. Edit group or color replacement rules.
6. Click `Preview`.
7. Add or duplicate variations as needed.
8. Click `Export` for the active variation, or `Export All` for all enabled variations.
9. Use `Save Session` to save the setup as JSON.

## Replacement Modes

- `GroupUniform`: applies one target color and blend ratio to all colors in the group.
- `PerColor`: only enabled per-color rules are applied.
- `Hybrid`: enabled per-color rules override the group fallback.

## Variation Workflow

- `Add`: creates a new variation snapshot.
- `Duplicate`: copies the active variation into an independent variation.
- `Remove`: removes the active variation when more than one variation exists.
- `Export Enabled`: controls whether a variation is included in `Export All`.
- `File Suffix`: controls the output file suffix for that variation.

## Samples

Validation samples are included under:

```text
Packages/com.sunmax0731.icon-palette-variant-generator/Samples~/SampleIcons
```

## Notes

- Source images are not overwritten.
- Output PNGs are written to the configured output folder.
- Transparent pixels at or below `Alpha Threshold` are ignored during analysis and replacement.
