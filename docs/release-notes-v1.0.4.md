# リリースノート - v1.0.4

`Unity Icon Palette Variant Generator` の v1.0.4 リリースです。

## 主な変更

- 公開メニューを `Tools > Palette Variant Generator > メイン画面`、`ライセンス`、`バージョン情報` に整理しました。
- Unity Editor 内のライセンス画面とバージョン情報画面を追加しました。
- MIT License 表記を README、package README、docs、release packaging に反映しました。
- `Agents.md` と `Skill.md` に共通メニュー構成と MIT License 方針を追加しました。
- README、manual、release checklist、validation checklist を v1.0.4 の配布物名に更新しました。

## 検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
powershell -ExecutionPolicy Bypass -File tools\release\build-release.ps1 -Version 1.0.4
powershell -ExecutionPolicy Bypass -File tools\release\test-release-package.ps1 -Version 1.0.4
```

Release assets:

```text
PaletteVariantGenerator_v1.0.4.zip
PaletteVariantGenerator_v1.0.4.unitypackage
```
