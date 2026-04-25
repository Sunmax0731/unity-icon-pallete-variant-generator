# 01 - Issue And Planning Skill

## 目的

実装前に GitHub Issue、要件、設計、仕様を確認し、作業範囲を明確にする工程の Skill。

## 参照ファイル

- `Agents.md`
- `docs/requirements.md`
- `docs/specification.md`
- `docs/architecture.md`
- `docs/development_plan.md`
- `docs/color_variant_rule.schema.json`
- GitHub Issues

## 手順

1. GitHub の open Issue を確認する。
2. P1 の中から、依存関係上もっとも先に進めるべき Issue を選ぶ。
3. 該当 Issue の参照ドキュメントを読む。
4. 既存実装との差分を確認する。
5. 作業範囲、検証方法、更新すべきドキュメントを決める。

## 判断基準

- Issue と docs の記載が食い違う場合は、現行実装とユーザー要望を優先し、docs を更新する。
- Release packaging は #7 と #8 が完了するまで開始しない。
- ユーザーから「引き続き」と言われた場合は、open Issue の優先順に沿って進める。

## 完了条件

- 対応する Issue が明確である。
- 実装対象ファイルと検証方法が明確である。
- 必要なら docs / skill 更新も作業範囲に含めている。
