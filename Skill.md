# Skill.md - Unity Icon Palette Variant Generator

このファイルは工程別 Skill の入口です。作業内容に応じて `docs/skills/` 配下の該当ファイルを参照してください。

## Skill Index

1. `docs/skills/01-issue-and-planning.md`
   - GitHub Issue 確認、優先順位、要件・設計・仕様の確認
2. `docs/skills/02-core-implementation.md`
   - Model / Service / JSON / PNG 出力などの中核実装
3. `docs/skills/03-editor-ui-workflow.md`
   - EditorWindow、IMGUI レイアウト、Help、言語設定、プレビュー overlay
4. `docs/skills/04-validation-and-qa.md`
   - Unity `6000.4.0f1` 検証、EditMode テスト、手動 QA
5. `docs/skills/05-release-packaging.md`
   - README / CHANGELOG / Manual / Terms / BOOTH copy / GitHub Release

## Always Apply

- 元画像アセットを直接変更しない。
- 出力画像は別ファイルとして保存する。
- 透明ピクセルを意図せず塗らない。
- JSON に `UnityEngine.Object` 参照を保存しない。
- `Assets/...` 相対パスと OS 絶対パスを混同しない。
- GitHub Issue の優先順位を確認してから実装に入る。
- ユーザーが検証用に追加した未追跡アセットは、明示されない限りコミットしない。

## Current Required Order

1. `#7` 複数バリエーション管理と一括出力
2. `#8` 手動 QA、サンプル、検証チェックリスト
3. `#9` Release packaging と GitHub Release `v1.0.0`

Release 作業は #7 と #8 の完了後に行う。
