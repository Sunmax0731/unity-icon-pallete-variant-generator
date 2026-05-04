# Unity Icon Palette Variant Generator ドキュメント

`Unity Icon Palette Variant Generator` は、Unity Editor 上で画像の色を解析し、近傍色のグルーピング、色置換、レイヤー合成、簡易描画ツールを使って色違いアイコンを生成する Editor 拡張です。

## 現行バージョン

- Package version: `1.1.0`
- Unity: `6000.4.0f1`
- 主な対象: PNG / JPEG / Texture2D
- 配布形式: UPM パッケージ ZIP / `.unitypackage`

## ドキュメント一覧

| ファイル | 内容 |
|---|---|
| `requirements.md` | ツール全体の要件定義。v1.1.0 のレイヤー、描画、JPEG 透過編集も含む。 |
| `specification.md` | 操作仕様、データ仕様、色置換、描画ツール、Export 仕様。 |
| `architecture.md` | EditorWindow、Model、Service、Validation の責務分割。 |
| `development_plan.md` | 実装フェーズ、完了状態、リリース準備項目。 |
| `manual.md` | 利用者向けマニュアル。 |
| `validation-checklist.md` | 自動 / 手動検証項目。 |
| `release-checklist.md` | v1.1.0 リリース準備手順。 |
| `release-notes-v1.1.0.md` | v1.1.0 のリリースノート。 |
| `booth-copy.md` | BOOTH 商品紹介編集ページ用 Markdown。 |
| `manual-test-layered-editing.md` | レイヤーと描画ワークフローの手動確認。 |
| `manual-test-fill-tool.md` | 塗りつぶしツールの手動確認。 |
| `manual-test-export-alpha-transparency.md` | Export 後の `Alpha Is Transparency` 確認。 |
| `paint-editing-refactor.md` | 描画処理の責務分割方針。 |
| `jpeg-source-transparency.md` | JPEG 読み込み画像の内部 RGBA 化と透過編集仕様。 |
| `color_variant_rule.schema.json` | ルール / セッション JSON の保存形式。 |
| `skills/` | Agent 向けの工程別作業ガイド。 |

## 標準検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

リリース前は次も実行します。

```powershell
powershell -ExecutionPolicy Bypass -File tools\release\build-release.ps1 -Version 1.1.0
powershell -ExecutionPolicy Bypass -File tools\release\test-release-package.ps1 -Version 1.1.0
```

## 注意

- 元画像ファイルは直接上書きしません。
- 読み込み画像への直接編集は、セッション内 RGBA バッファと Export PNG に反映します。
- JPEG は内部で PNG 相当の RGBA バッファとして扱うため、消しゴムで透明化できます。
- `Alpha Is Transparency` の自動設定は、Unity Project の `Assets/` 配下へ出力した PNG が対象です。
