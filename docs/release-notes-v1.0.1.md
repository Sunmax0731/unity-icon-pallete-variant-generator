# リリースノート - v1.0.1

`Unity Icon Palette Variant Generator` の v1.0.1 リリースです。

## 主な変更

- RGB / HSV / Lab の色距離モードを追加しました。
- `Max Color Distance` によるグループ調整を追加しました。
- Folder Batch Export を追加しました。
- ScriptableObject プリセットを追加しました。
- ドッキング時の横幅とスクロール表示を改善しました。
- UI Toolkit 移行に向けた構成整理を行いました。

## 検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

主要 marker:

```text
ISSUE7_VARIATION_BATCH_EXPORT_VALIDATION=PASS
ISSUE10_UI_POLISH_VALIDATION=PASS
ISSUE12_VARIATION_UX_VALIDATION=PASS
ISSUE18_RULE_PRESET_VALIDATION=PASS
```
