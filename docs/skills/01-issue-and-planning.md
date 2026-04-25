# 01 - Issue And Planning Skill

## 目的

実装前に GitHub Issue、要件、設計、仕様を確認し、作業範囲を明確にする工程の Skill。

## 参照ファイル

- `Agents.md`
- `Skill.md`
- `docs/requirements.md`
- `docs/specification.md`
- `docs/architecture.md`
- `docs/development_plan.md`
- `docs/color_variant_rule.schema.json`
- GitHub Issues

## 手順

1. GitHub の open Issue を確認する。
2. 優先度と依存関係を見て、次に進める Issue を選ぶ。
3. 該当 Issue の参照ドキュメントを読む。
4. 既存実装との差分を確認する。
5. 作業範囲、検証方法、更新すべきドキュメントを決める。

## Issue 記載ルール

- Issue のタイトル、本文、コメント、完了報告は日本語で記載する。
- 英語の API 名、クラス名、メニュー名、ファイル名は原文のままでよい。
- 新規 Issue は、概要、対応範囲、完了条件が分かる粒度で作成する。
- 複数の独立した改善案は、1つの巨大 Issue にまとめず個別 Issue に分ける。

## 判断基準

- Issue と docs の記載が食い違う場合は、現行実装とユーザー要望を優先し、必要に応じて docs を更新する。
- ユーザーから「引き続き」と言われた場合は、open Issue の優先順位に沿って進める。
- Release packaging は、前提となる機能と検証が完了してから開始する。

## 完了条件

- 対応する Issue が明確である。
- 実装対象ファイルと検証方法が明確である。
- 必要な docs / skill 更新も作業範囲に含めている。
