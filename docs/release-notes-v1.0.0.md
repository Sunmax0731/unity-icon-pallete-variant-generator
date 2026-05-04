# リリースノート - v1.0.0

`Unity Icon Palette Variant Generator` の初回リリースです。

## 主な内容

- Unity EditorWindow から Source Image を選択できるようにしました。
- 画像からパレット色を抽出できるようにしました。
- 近傍色の自動グルーピングを追加しました。
- グループ単位の色置換と Preview を追加しました。
- PNG Export を追加しました。
- Session JSON 保存 / 読み込みを追加しました。

## 検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

主要 marker:

```text
ISSUE1_SCAFFOLD_VALIDATION=PASS
ISSUE2_IMAGE_PALETTE_VALIDATION=PASS
ISSUE3_COLOR_GROUPING_VALIDATION=PASS
ISSUE4_REPLACEMENT_PREVIEW_VALIDATION=PASS
ISSUE5_PNG_EXPORT_VALIDATION=PASS
ISSUE6_SESSION_JSON_VALIDATION=PASS
```
