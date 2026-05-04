# リリースノート - v1.1.0

## 概要

`Unity Icon Palette Variant Generator` の v1.1.0 リリースです。色替えツールに加えて、Unity Editor 内で完結する軽量なレイヤー編集、読み込み画像への直接描画、DCC 風の描画ツール、JPEG 透過編集、塗りつぶしツールを追加しました。

## 主な変更

- Unity Editor 拡張の画面を Toolbar / Settings / Preview Workspace / Inspector / Report の責務に沿って整理しました。
- 日本語モードの主要 UI 文言を日本語化しました。
- Tool Settings を折りたたみ可能にしました。
- Preview キャンバスの高さをドラッグで変更できるようにしました。
- Paint Layer / Image Layer、表示切替、ロック、並び替え、複製、削除、不透明度を含むレイヤー機能を追加しました。
- 読み込み画像へ直接編集できる `編集対象` を追加しました。元画像ファイルは上書きしません。
- ブラシ、消しゴム、ぼかし、スムース、ノイズ除去、塗りつぶしツールを追加しました。
- ブラシ / 消しゴムのドラッグ補間を追加し、素早くマウスを動かした場合の線飛びを抑えました。
- JPEG 読み込み画像を内部 RGBA バッファとして扱い、消しゴムで透明化できるようにしました。
- Export / Export All / Folder Batch Export にレイヤー合成と読み込み画像への直接編集を反映しました。
- `Assets/` 配下に出力した PNG の `Alpha Is Transparency` を自動で ON にするようにしました。

## 塗りつぶしツール

`塗りつぶし` は、クリックしたピクセルと同じ RGBA を持つ上下左右連結領域だけを対象にします。透明ピクセル同士の領域も塗りつぶし対象です。ペイントのバケツツールに近い操作で、背景や大きな単色領域をまとめて変更できます。

## JPEG 透過編集

JPEG はファイル形式上 alpha を保持できません。v1.1.0 では、読み込み時に内部で RGBA バッファへ正規化し、消しゴムで透明化した結果をセッションと PNG 出力へ保持します。元の JPEG アセットは上書きしません。

## 検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
powershell -ExecutionPolicy Bypass -File tools\release\build-release.ps1 -Version 1.1.0
powershell -ExecutionPolicy Bypass -File tools\release\test-release-package.ps1 -Version 1.1.0
```

主要 marker:

```text
ISSUE48_DIRECT_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_RGB_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_JPG_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_JPG_INTERNAL_PNG_CONVERSION_VALIDATION=PASS
ISSUE48_JPG_WINDOW_ERASER_VALIDATION=PASS
ISSUE48_EXPORT_ALPHA_TRANSPARENCY_VALIDATION=PASS
ISSUE48_FILL_TOOL_VALIDATION=PASS
ISSUE48_TOOL_POPUP_SYNC_VALIDATION=PASS
```

## 配布物

GitHub Release には以下の両方を添付します。

```text
PaletteVariantGenerator_v1.1.0.zip
PaletteVariantGenerator_v1.1.0.unitypackage
```

## 既知の制限

- SpriteAtlas の直接編集は対象外です。
- 元画像ファイルは上書きしません。読み込み画像への直接編集は Session JSON と書き出し PNG に反映されます。
- `Alpha Is Transparency` の自動設定は `Assets/` 配下に出力した PNG に限ります。
