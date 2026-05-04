# UI Toolkit 移行メモ

## 現状

v1.1.0 のメイン画面は、既存の EditorWindow 実装を維持しつつ、UnityEditor 拡張デザインガイドラインに沿ったパネル構成へ整理しています。現時点では、描画ツール、Preview 入力、レイヤー操作の安定性を優先し、全面的な UI Toolkit 移行は行いません。

## 移行時の優先順位

1. Preview 入力と表示更新を View から分離する。
2. Tool Settings / Layers / Variations / Rules を独立した UI コンポーネントに分ける。
3. Preview キャンバスの pan / zoom / split compare / paint input を移植する。
4. ScrollView と Inspector 風パネルを UI Toolkit で再構成する。
5. IMGUI 依存を削除する前に、既存 validation marker をすべて通す。

## 維持する操作

- `Tools > Palette Variant Generator > メイン画面` から起動する。
- Source Image、Analyze、Auto Group、Preview、Export、Session は上部 toolbar に置く。
- Preview 上のクリック選択、ブラシ選択、描画、ズーム、パンを維持する。
- Palette / Layers / Variations / Rules のスクロール操作を維持する。
- 日本語 / 英語表示を維持する。

## 移行リスク

- Preview 座標変換の差異により Brush / Eraser / Fill がずれる。
- Slider / Popup / ObjectField の更新タイミング差で Auto Preview が過剰発火する。
- 描画中 Repaint の頻度が増え、Paint モードが重くなる。
- Docked window の横幅が狭い場合に Preview と右ペインが崩れる。

## 検証観点

- Paint モードで白飛びせずリアルタイムに表示される。
- Brush の高速ドラッグが補間される。
- Eraser が読み込み画像と描画レイヤーの両方に効く。
- Fill が 1 クリックで対象領域だけに効く。
- Export の結果が Preview と一致する。
- `Alpha Is Transparency` が `Assets/` 配下の出力 PNG で ON になる。
