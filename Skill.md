# Skill.md - Unity Icon Palette Variant Generator

このファイルは工程別 Skill の入口です。作業内容に応じて `docs/skills/` 配下の該当ファイルを参照してください。

## Skill Index

1. `docs/skills/01-issue-and-planning.md`
   - GitHub Issue 確認、優先順位、要件、設計、仕様確認
2. `docs/skills/02-core-implementation.md`
   - Model / Service / JSON / PNG 出力などの中核実装
3. `docs/skills/03-editor-ui-workflow.md`
   - EditorWindow、IMGUI レイアウト、Help、言語設定、プレビュー overlay
4. `docs/skills/04-validation-and-qa.md`
   - Unity `6000.4.0f1` 検証、EditMode テスト、手動 QA
5. `docs/skills/05-release-packaging.md`
   - README / CHANGELOG / Manual / Terms / BOOTH copy / GitHub Release

## Always Apply

- GitHub Issue を確認してから実装に入る。
- Issue、Issue コメント、PR、リリース説明、ユーザー向けドキュメントは日本語で記載する。
- 元画像アセットを直接変更しない。
- 出力画像は別ファイルとして保存する。
- 透明ピクセルを意図せず塗らない。
- JSON に `UnityEngine.Object` 参照を保存しない。
- `Assets/...` 相対パスと OS 絶対パスを混同しない。
- ユーザーが検証用に追加した未追跡アセットは、明示されない限りコミットしない。
- Unity の検証は `6000.4.0f1` を基準にする。

## Current Required Order

現在は GitHub Issue の open 状態を確認し、優先度が高く実行可能な Issue から着手する。

標準検証コマンド:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

## Release Artifact Policy

- GitHub Release には ZIP と `.unitypackage` 単体の両方を必ず添付する。
- `tools\release\build-release.ps1 -Version <version>` は `ReleaseBuilds/PaletteVariantGenerator_v<version>.zip` と `ReleaseBuilds/PaletteVariantGenerator_v<version>.unitypackage` を生成する。
- `tools\release\test-release-package.ps1 -Version <version>` で ZIP と `.unitypackage` の両方を検証してから Release を公開する。
- Release asset の追加・差し替え後は `gh release view <tag> --json assets` または `gh release list` で添付状態を確認する。

## Shared Unity Editor Extension Convention

- Public menu entries use `Tools > Palette Variant Generator > メイン画面`, `Tools > Palette Variant Generator > ライセンス`, and `Tools > Palette Variant Generator > バージョン情報`.
- Keep developer-only or helper commands under a secondary group such as `Developer` or `Utilities`.
- The license is MIT License. Keep `README.md`, package README, docs, release notes, and the Unity Editor license window aligned with MIT.
- When menu or license text changes, update README, manual, validation checklist, release notes, BOOTH/GitHub release copy, and release package contents in the same change.

## Layered Editing Extension Notes

- レイヤー/描画拡張を行う場合は `docs/requirements-layered-editing.md`、`docs/specification-layered-editing.md`、`docs/architecture-layered-editing.md` を確認する。
- UI 改修時は `D:\Claude\UnityEditor-Dev\workspace-guides\UnityEditorDesign.md` を参照し、Toolbar / Settings / Preview Workspace / Inspector / Report の責務分離を優先する。
