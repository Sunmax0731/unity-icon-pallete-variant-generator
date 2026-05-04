# 03 - Editor UI Workflow Skill

## 目的

Unity EditorWindow の画面構成、操作導線、Preview、Palette、Layers、Language 表示を整理するための工程ガイドです。

## 基本レイアウト

- 上部: Source Image / Analyze / Auto Group / Preview / Export / Export All / Undo / Redo / Save Session / Load Session / Help / Language。
- 左: Source 情報、解析設定、グループ設定、ツール設定、書き出し設定、プリセット。
- 中央: Preview、選択色情報、Palette。
- 右: Layers、Variations、置換ルール、色別ルール。

## UI 原則

- Unity Editor 拡張として自然な Inspector 風パネルを使う。
- 日本語モードでは、主要な UI 項目、ツール名、説明文を日本語で表示する。
- API 名や mode 名が必要な箇所は、英語を残してもよい。
- `Tool Settings` は折りたたみ可能にする。
- Preview キャンバスはドラッグで高さを変更できる。
- Paint モードではリアルタイム編集表示を優先する。

## Preview 操作

- クリック: ピクセル色選択。
- ドラッグ: pan、ブラシ選択、描画のいずれかを mode に応じて行う。
- Wheel: zoom。
- Split slider: Before / After の分割位置を変更する。

## 描画 UI

- `編集対象`: `読み込み画像` / `アクティブレイヤー`。
- `ツール`: `ブラシ` / `消しゴム` / `塗りつぶし` / `ぼかし` / `スムース` / `ノイズ除去`。
- Brush Size、強さ、描画不透明度、描画色、ノイズ領域、ノイズ閾値、ぼかし半径、スムース回数を表示する。

## 確認観点

- メニューから Window が開く。
- Source 未選択、Analyze 未実行、Layer 未作成などの空状態で例外が出ない。
- 日本語 / 英語を切り替えても状態が壊れない。
- Preview の比較モード変更後も Paint 入力が失われない。
- レイヤーの visible / lock / opacity が Preview と Export に反映される。
