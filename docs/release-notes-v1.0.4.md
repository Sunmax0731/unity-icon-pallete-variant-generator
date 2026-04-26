# リリースノート - v1.0.4

## 概要

`Unity Icon Palette Variant Generator` の v1.0.4 リリースです。
このリリースでは、Unity Editor の公開メニュー構成、MIT License 表記、README / docs / release packaging の整合性を更新しました。

## 主な変更

- Unity Editor の公開メニューを `Tools > Palette Variant Generator > メイン画面`, `ライセンス`, `バージョン情報` に統一しました。
- Unity Editor 内のライセンス画面とバージョン情報画面に、MIT License、package id、version、repository / release URL を表示するよう整理しました。
- `LICENSE.md` を package に追加し、root README と package README に MIT License を明記しました。
- `Agents.md` と `Skill.md` に、今後の Unity Editor 拡張で共通適用するメニュー構成と MIT License 方針を追記しました。
- README、manual、release checklist、validation checklist を v1.0.4 の配布物名に更新しました。

## 検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
powershell -ExecutionPolicy Bypass -File tools\release\build-release.ps1 -Version 1.0.4
powershell -ExecutionPolicy Bypass -File tools\release\test-release-package.ps1 -Version 1.0.4
```

Release には `PaletteVariantGenerator_v1.0.4.zip` と `PaletteVariantGenerator_v1.0.4.unitypackage` を添付します。