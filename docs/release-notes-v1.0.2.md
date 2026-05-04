# リリースノート - v1.0.2

`Unity Icon Palette Variant Generator` の v1.0.2 リリースです。

## 主な変更

- メイン Window の UI Toolkit 化準備を進めました。
- ノイズ除去とエッジ外側クリーンアップを追加しました。
- 書き出し設定専用 Window を追加しました。
- JPG などの不透明画像で外側背景を処理しやすくしました。
- Export / Export All / Folder Batch Export の導線を整理しました。

## 検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
powershell -ExecutionPolicy Bypass -File tools\release\build-release.ps1 -Version 1.0.2
powershell -ExecutionPolicy Bypass -File tools\release\test-release-package.ps1 -Version 1.0.2
```

Release assets:

```text
PaletteVariantGenerator_v1.0.2.zip
PaletteVariantGenerator_v1.0.2.unitypackage
```
