# Analysis Category Presets

The Analyze section provides presets that update analysis, grouping, noise removal, and edge cleanup settings together.

## Presets

- `TransparentPng`: balanced default for transparent PNG icons. Uses RGB grouping, moderate quantization, preserves alpha, and leaves cleanup disabled.
- `WhiteBackgroundJpg`: for JPG or opaque assets on a light background. Uses Lab grouping, stronger quantization, noise removal, and `BoundaryTrim` edge cleanup.
- `LineArtIcon`: for crisp icons or line art. Uses precise quantization, fewer groups, tighter RGB distance, and keeps cleanup disabled to avoid damaging outlines.
- `Gem`: for high-saturation assets with highlights. Uses Lab grouping, low quantization, more groups, and light noise removal.
- `Plant`: for organic green/brown assets. Uses HSV grouping, moderate quantization, and noise removal tuned for small color islands.

Preset selection does not replace color rules or variations. It only changes analysis, grouping, noise, and edge cleanup parameters used by the next Analyze, Auto Group, or Preview flow.
