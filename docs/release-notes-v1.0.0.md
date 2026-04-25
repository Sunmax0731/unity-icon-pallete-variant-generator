# リリースノート - v1.0.0

## 概要

`Unity Icon Palette Variant Generator` の初回リリースです。

このリリースでは、元アイコンからパレット色を抽出し、近傍色をグルーピングし、置換ルールを編集してプレビュー確認し、1つ以上の PNG 色違いバリエーションを書き出す Unity Editor ワークフローを提供します。

## 同梱物

- UPM package: `com.sunmax0731.icon-palette-variant-generator`
- `Samples~/SampleIcons` 配下のサンプルアイコン
- マニュアル: `docs/manual.md`
- 利用条件: `docs/terms.md`
- 変更履歴: `CHANGELOG.md`
- 検証チェックリスト: `docs/validation-checklist.md`

## 検証

- Unity: `6000.4.0f1`
- コマンド:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

marker:

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

## 既知の制限

- UI は IMGUI ベースです。
- 色距離は RGB / HSV / Lab から選択できます。
- フォルダ内 Texture2D に対する一括バリエーション出力に対応しました。
- チーム共有向けの ScriptableObject プリセットアセットを作成、更新、読み込みできるようにしました。
- SpriteAtlas の直接編集は含みません。
- フォルダ単位の一括処理は含みません。
