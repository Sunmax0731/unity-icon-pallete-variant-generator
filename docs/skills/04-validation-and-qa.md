# 04 - Validation And QA Skill

## 目的

Unity `6000.4.0f1` での EditMode テスト、自動 validation、手動 QA を管理するための工程ガイドです。

## 標準検証

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
ISSUE7_VARIATION_BATCH_EXPORT_VALIDATION=PASS
ISSUE8_SAMPLE_QA_VALIDATION=PASS
ISSUE10_UI_POLISH_VALIDATION=PASS
ISSUE12_VARIATION_UX_VALIDATION=PASS
ISSUE13_AUTO_PREVIEW_DEBOUNCE_VALIDATION=PASS
ISSUE17_PREVIEW_NAVIGATION_VALIDATION=PASS
ISSUE18_RULE_PRESET_VALIDATION=PASS
ISSUE19_MANUAL_GROUP_EDITING_VALIDATION=PASS
ISSUE48_DIRECT_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_RGB_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_JPG_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_JPG_INTERNAL_PNG_CONVERSION_VALIDATION=PASS
ISSUE48_JPG_WINDOW_ERASER_VALIDATION=PASS
ISSUE48_EXPORT_ALPHA_TRANSPARENCY_VALIDATION=PASS
ISSUE48_FILL_TOOL_VALIDATION=PASS
ISSUE48_TOOL_POPUP_SYNC_VALIDATION=PASS
```

## 手動 QA

- `docs/manual-test-layered-editing.md`
- `docs/manual-test-fill-tool.md`
- `docs/manual-test-export-alpha-transparency.md`

重点確認:

- Preview が白飛びせず、Paint 中にリアルタイム更新される。
- Brush の高速ドラッグで線が飛びにくい。
- Eraser が読み込み画像と描画レイヤーに効く。
- JPEG 読み込み画像の消しゴム結果が PNG Export に反映される。
- Fill が同一 RGBA の上下左右連結領域だけに効く。
- Export PNG が Preview と一致する。
- `Assets/` 配下出力時に `Alpha Is Transparency` が ON になる。

## 生成物の扱い

- `Assets/` 配下へユーザーが追加した検証画像、session JSON、preset asset は、明示がない限りコミットしない。
- `Library/`, `Logs/`, `Temp/`, `UserSettings/`, `Validation/` はコミットしない。
- ReleaseBuilds はリリース成果物として生成するが、通常の修正 commit には含めない。

## 失敗時の対応

- compile error は最優先で修正する。
- Unity が起動中で検証できない場合は、clean worktree または Editor 終了後に再実行する。
- marker 不足は validation script と docs の両方を更新する。
