# 01 - Issue And Planning Skill

## 目的

GitHub Issue、要件、仕様、設計を確認し、作業範囲と完了条件を明確にしてから実装するための工程ガイドです。

## 参照ファイル

- `Agents.md`
- `Skill.md`
- `docs/requirements.md`
- `docs/specification.md`
- `docs/architecture.md`
- `docs/development_plan.md`
- `docs/requirements-layered-editing.md`
- `docs/specification-layered-editing.md`
- `docs/architecture-layered-editing.md`
- GitHub Issues

## 手順

1. `gh issue list --state open` で open Issue を確認する。
2. 優先度と依存関係から、次に進める Issue を 1 つ選ぶ。
3. Issue の要件と関連 docs を読む。
4. 既存実装との差分を確認する。
5. 更新が必要な docs / tests / validation marker を作業範囲に含める。
6. 実装後に Issue へ検証結果を日本語でコメントする。

## 判断ルール

- ユーザーの手動確認結果は、Issue の受け入れ判断として扱う。
- docs と実装が食い違う場合は、現行仕様に合わせて docs も更新する。
- 機能追加時は、最低 1 つ以上の自動検証 marker または EditMode test を追加 / 更新する。
- Release 準備は、open Issue が blocker ではないことを確認してから進める。

## Issue 記載ルール

- タイトル、本文、コメント、完了報告は日本語を基本にする。
- API 名、class 名、file path、menu path は英語表記のままでよい。
- 完了コメントには、変更概要、検証コマンド、主要 marker、残リスクを含める。
