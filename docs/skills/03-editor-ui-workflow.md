# 03 - Editor UI Workflow Skill

## 目的

Unity EditorWindow の UI、操作導線、プレビュー、ヘルプ、言語表示を整える工程の Skill。

## 基本レイアウト

- 上部: Source / Analyze / Auto Group / Preview / Export / Session / Help / Language
- 左: 解析設定 / グループ設定 / 出力設定 / 画像情報
- 中央: Before / After preview / スクロール可能な Palette
- 右: グループ置換ルール / 色別置換ルール / バリエーション

## UI 原則

- IMGUI で Unity Editor らしい密度と操作感を保つ。
- パネルの責務を混ぜない。
- Palette と Group の選択状態は preview overlay と連動させる。
- Help と Language は Analyze に近い上部 toolbar に置く。
- Palette は色数が多くても操作できるようスクロール可能にする。
- ボタンや field の表示文言は短く保つ。
- ユーザー向け文言は日本語を基本にする。

## 言語設定

- `Auto` / `English` / `Japanese` を基本にする。
- 表示言語は `EditorPrefs` に保存する。
- コード識別子や enum 名は翻訳しない。
- 日本語 UI でも `Analyze` など Unity 操作上わかりやすい英語は残してよい。

## Help

- Help window は現在の操作導線を説明する。
- 長文マニュアルではなく、パラメータの意味と操作順に絞る。
- Release 前の詳細マニュアルは release packaging 工程で整備する。

## UI 変更時の確認

- メニューから window が開く。
- Analyze なしの状態で無効化すべき操作が壊れていない。
- Palette がスクロールできる。
- Preview が null のときに例外が出ない。
- 選択 overlay が表示され、選択解除や切り替えで破綻しない。
