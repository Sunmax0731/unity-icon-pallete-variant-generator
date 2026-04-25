# Unity Icon Palette Variant Generator 開発準備ドキュメント

## 概要

`Unity Icon Palette Variant Generator` は、Unity プロジェクト内の画像アセットから色を抽出し、近傍色を自動グルーピングした上で、グループ単位またはカラーコード単位の置換ルールを使って、単一のアイテムアイコンから複数の色違いパターンを生成する Unity エディタ拡張です。

MMORPG / RPG / アイテム制作向けに、鉱石、木材、薬草、料理、装備、宝石などの素材アイコンを効率よく量産することを主目的とします。

## ドキュメント一覧

| ファイル | 内容 |
|---|---|
| `requirements.md` | 要件定義書。目的、対象ユーザー、機能要件、非機能要件、受け入れ基準を整理。 |
| `specification.md` | 仕様書。画面構成、処理フロー、パレット生成、グルーピング、置換、エクスポート仕様を整理。 |
| `architecture.md` | 設計書。MVP 構成、主要クラス、データモデル、サービス分割、フォルダ構成を整理。 |
| `development_plan.md` | 開発計画。実装フェーズ、タスク、優先度、テスト観点、リリース準備を整理。 |
| `color_variant_rule.schema.json` | セッション / プリセット保存用 JSON スキーマ草案。 |
| `../Agents.md` | AI Agent / Codex に実装を依頼するための作業指示書。 |
| `../Skill.md` | 工程別 Skill の入口。 |
| `skills/` | Issue、実装、UI、検証、Release の工程別 Skill。 |

## 想定メニュー

```text
Tools > Icon Tools > Palette Variant Generator
```

## 最小 MVP

1. Unity プロジェクト内の PNG / Texture2D を選択する
2. 画像から色を抽出する
3. 近い色を任意グループ数にまとめる
4. グループごとに置換色と反映率を設定する
5. 変換後プレビューを表示する
6. PNG として別名出力する
7. 設定を JSON として保存 / 読み込みできる
