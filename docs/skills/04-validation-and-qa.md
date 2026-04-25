# 04 - Validation And QA Skill

## 目的

Unity `6000.4.0f1` での自動検証、EditMode テスト、手動 QA を管理する工程の Skill。

## 標準検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

必要 marker:

```text
ISSUE1_SCAFFOLD_VALIDATION=PASS
ISSUE2_IMAGE_PALETTE_VALIDATION=PASS
ISSUE3_COLOR_GROUPING_VALIDATION=PASS
ISSUE4_REPLACEMENT_PREVIEW_VALIDATION=PASS
ISSUE5_PNG_EXPORT_VALIDATION=PASS
ISSUE6_SESSION_JSON_VALIDATION=PASS
ISSUE7_VARIATION_BATCH_EXPORT_VALIDATION=PASS
ISSUE8_SAMPLE_QA_VALIDATION=PASS
ISSUE10_UI_POLISH_VALIDATION=PASS
ISSUE12_VARIATION_UX_VALIDATION=PASS
ISSUE13_AUTO_PREVIEW_DEBOUNCE_VALIDATION=PASS
```

## テスト方針

- 中核ロジックは `Tests/Editor` の EditMode テストで確認する。
- UI は headless validation と手動 QA の両方で確認する。
- 新しい Issue marker を追加したら README と validation script も更新する。
- 検証結果を Issue に残す場合は日本語で記載する。

## 手動 QA 観点

- 64x64 透明 PNG
- 128x128 透明 PNG
- アンチエイリアスありのアイコン
- ドット絵アイコン
- 透明部分が透明のまま出力される
- Analyze / Auto Group / Preview / Export
- Save Session / Load Session
- Language menu / Help window
- Scrollable Palette
- `Max Color Distance`
- `GroupUniform` / `PerColor` / `Hybrid`

## 生成物の扱い

- `Logs/`, `Validation/`, `Library/`, `UserSettings/` はコミットしない。
- QA 用にユーザーが置いた `Assets/` 配下の画像は、Issue でサンプルとして採用する場合だけコミットする。
- Release 用生成物は release packaging の作業範囲で扱う。
