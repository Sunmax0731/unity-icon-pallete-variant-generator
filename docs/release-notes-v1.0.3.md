# リリースノート - v1.0.3

`Unity Icon Palette Variant Generator` の v1.0.3 リリースです。

## 主な変更

- Preview 上のクリック色選択とドラッグパンを改善しました。
- `BrushSelect` により複数パレット色をドラッグ選択できるようにしました。
- `Visible in Export` を追加しました。
- 選択色 / 選択グループの Preview ハイライトを追加しました。
- Category Preset、Undo / Redo、Difference Preview を追加しました。
- Export 前チェックと Boundary Trim を改善しました。
- Unity 6 の非推奨 API 警告に対応しました。

## 検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
powershell -ExecutionPolicy Bypass -File tools\release\build-release.ps1 -Version 1.0.3
powershell -ExecutionPolicy Bypass -File tools\release\test-release-package.ps1 -Version 1.0.3
```

Release assets:

```text
PaletteVariantGenerator_v1.0.3.zip
PaletteVariantGenerator_v1.0.3.unitypackage
```
