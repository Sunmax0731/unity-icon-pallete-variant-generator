# Skill.md - Unity Icon Palette Variant Generator

このファイルは、工程別 Skill の入口です。作業内容に応じて `docs/skills/` 配下の該当ファイルと、最新仕様ドキュメントを確認してください。

## Skill Index

1. `docs/skills/01-issue-and-planning.md`
   - GitHub Issue 確認、優先順位、要件、設計、仕様確認
2. `docs/skills/02-core-implementation.md`
   - Model / Service / JSON / PNG 出力などの中核実装
3. `docs/skills/03-editor-ui-workflow.md`
   - EditorWindow、IMGUI / UI Toolkit、Help、言語設定、Preview overlay
4. `docs/skills/04-validation-and-qa.md`
   - Unity `6000.4.0f1` 検証、EditMode テスト、手動 QA
5. `docs/skills/05-release-packaging.md`
   - README / CHANGELOG / Manual / Terms / BOOTH copy / GitHub Release

## Always Apply

- GitHub Issue を確認してから実装に入る。
- Issue、Issue コメント、PR、リリース説明、ユーザー向けドキュメントは日本語を基本にする。
- 元画像アセットを直接変更しない。
- 出力画像は別ファイルとして保存する。
- 読み込み画像への直接編集は、セッション内 RGBA バッファと書き出し PNG に反映する。
- JPEG は内部で PNG 相当の RGBA バッファとして扱い、消しゴムの alpha 0 を保持する。
- `Assets/` 配下へ出力した PNG は `Alpha Is Transparency` を ON にする。
- JSON に `UnityEngine.Object` 参照を直接保存しない。
- `Assets/...` 相対パスと OS 絶対パスを混同しない。
- ユーザーが検証用に追加した未追跡アセットは、明示されない限りコミットしない。
- Unity の検証は `6000.4.0f1` を基準にする。

## Current Required Order

現在の主要作業単位は GitHub Issue `#48` の UI ガイドライン準拠、レイヤー対応、描画ツール、読み込み画像直接編集、JPEG 透過編集、塗りつぶし、Export alpha 設定です。作業時は以下を優先して確認します。

1. `docs/requirements-layered-editing.md`
2. `docs/specification-layered-editing.md`
3. `docs/architecture-layered-editing.md`
4. `docs/paint-editing-refactor.md`
5. `docs/jpeg-source-transparency.md`
6. `docs/manual-test-layered-editing.md`
7. `docs/manual-test-fill-tool.md`
8. `docs/manual-test-export-alpha-transparency.md`

## Architecture Notes

- `PaletteVariantGeneratorWindow` は UI と操作仲介を担当する。
- `RasterPaintService` は Brush / Eraser / Fill / Blur / Smooth / NoiseRemoval のピクセル処理を担当する。
- `PaintStrokeSessionService` はドラッグ中の編集バッファ、ブラシ補間、commit を担当する。
- `LayerCompositingService` は Preview / Export 共通の合成経路を担当する。
- `TextureAssetLoader` は JPEG を含む読み込み画像を編集可能な RGBA バッファへ正規化する。
- `ExportedTextureImportSettingsService` は出力後の Unity TextureImporter 設定を担当する。

## Validation

標準検証コマンド:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

Issue #48 で必ず確認する marker:

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

## Release Artifact Policy

- GitHub Release には ZIP と `.unitypackage` の両方を添付する。
- `tools\release\build-release.ps1 -Version <version>` は `ReleaseBuilds/PaletteVariantGenerator_v<version>.zip` と `ReleaseBuilds/PaletteVariantGenerator_v<version>.unitypackage` を生成する。
- `tools\release\test-release-package.ps1 -Version <version>` で ZIP と `.unitypackage` の両方を検証してから Release を公開する。
- Release asset の追加・差し替え後は `gh release view <tag> --json assets` で添付状態を確認する。

## BOOTH Copy

- `D:\Claude\UnityEditor-Dev\booth-product-page-autofill` の Markdown 仕様に合わせ、`docs/booth-copy.md` を BOOTH 編集ページ用の入力原稿として維持する。
- 自動入力対象は `## 商品名`、`## 概要`、その他入力対象 `##` / `###` セクション。
- `## タグ案`、`## 商品画像構成案`、`## BOOTH入力設定` は作業用情報として扱い、BOOTH 商品紹介文へは自動入力されない。
