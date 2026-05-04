# 変更履歴

## 1.1.0 - 2026-05-05

### 追加

- Unity Editor 拡張の画面を Toolbar / Settings / Preview / Inspector / Report の責務に沿って整理し、日本語モードの主要 UI 文言を日本語化しました。
- Paint Layer / Image Layer、表示切替、ロック、並び替え、複製、削除、不透明度を含むレイヤー編集を追加しました。
- 読み込み画像へ直接編集できる `編集対象` を追加しました。元画像ファイルは上書きせず、セッション内の RGBA バッファに保持します。
- ブラシ、消しゴム、ぼかし、スムース、ノイズ除去、塗りつぶしツールを追加しました。
- JPEG 読み込み画像を内部で PNG 相当の RGBA バッファとして扱い、消しゴムで透明化できるようにしました。
- Preview キャンバスのリサイズ、描画中のリアルタイム反映、ブラシストローク補間を追加しました。
- Export / Export All / Folder Batch Export でレイヤー合成と読み込み画像への直接編集を反映するようにしました。
- `Assets/` 配下に出力した PNG の `Alpha Is Transparency` を自動で ON にする処理を追加しました。
- Issue #48 の直接編集、JPEG 透過、Fill、Tool popup 同期、Export alpha import 設定の検証 marker を追加しました。

### 変更

- 描画処理を `RasterPaintService`、ストローク中バッファ管理を `PaintStrokeSessionService`、出力後 import 設定を `ExportedTextureImportSettingsService` に分離し、Window から画像処理責務を切り出しました。
- Session JSON に source pixel buffer、レイヤー、描画ツール設定を保存するようにしました。

## 1.0.4 - 2026-04-26

### 変更

- 公開メニューを `Tools > Palette Variant Generator > メイン画面`、`ライセンス`、`バージョン情報` に統一しました。
- MIT License 表記、README、Manual、Release checklist、Validation checklist、配布物の整合性を更新しました。
- ZIP と `.unitypackage` の両方をリリース成果物として扱う方針を明文化しました。

## 1.0.3 - 2026-04-26

### 追加

- Preview上のピクセルをクリックして、対応するパレット色を選択できる機能を追加。
- 選択中のパレット色またはグループをPreview上でハイライト表示する機能を追加。
- ハイライト表示のON/OFF切り替えを追加。
- UI Toolkit Previewをドラッグして表示位置を移動できる機能を追加。
- Preview付近に、選択中の色のHEX / RGBA / Group / pixel count / 置換後色 / 色別ルール状態を表示する情報パネルを追加。
- 解析カテゴリプリセット、折りたたみ設定、Previewミニツールバー、Undo/Redoを追加。
- Difference Preview、Export前チェック、Boundary Trimクリーンアップを追加。
- Preview上で複数色を選択できるブラシ選択と、選択色の表示ON/OFFを追加。
- 解析、グループ、ノイズ削除、エッジ外側クリーンアップの各パラメータに、言語設定に応じたtooltip説明を追加。

### 修正

- UI Toolkit Preview の Zoom スライダーが表示に反映されない問題を修正。
- Preview表示用テクスチャをキャッシュ化し、同じ表示状態で不要な再生成を行わないように軽量化。
- Preview更新とハイライト生成を遅延実行し、連続操作時は最新の要求だけを処理するように軽量化。
- Unity 6 の非推奨警告に対応し、`GetInstanceID()` を使わないように修正。

## 1.0.2 - 2026-04-25

### 追加

- ノイズ削除: 同一グループ内に囲まれた小さな色領域を、Preview / Export 前に近傍色で補正する機能を追加。
- メインウィンドウを本番 UI Toolkit 構成へ移行し、主要セクションと操作ボタンを UI Toolkit 上で直接表示するように変更。
- エッジ外側クリーンアップ: 主要な本体領域の外側近傍にある小領域を Preview / Export 前に透明化する機能を追加。
- 書き出し設定ウィンドウ: Export Settings と Folder Batch Export を専用ウィンドウから編集・実行できるように変更。

### 修正

- 不透明画像でもエッジ外側クリーンアップが効くように、画像端の色グループから外側背景を推定する処理を追加。
- 書き出し設定を表示しても設定欄が開かない問題を修正。

## 1.0.1 - 2026-04-25

### 追加

- RGB / HSV / Lab 色距離モードを追加。
- フォルダ一括処理と複数ソース画像の出力ワークフローを追加。
- ScriptableObject プリセットアセットによる置換ルール共有を追加。
- UI Toolkit 移行準備として main window を UI Toolkit root + `IMGUIContainer` の hybrid 構成へ移行。

### 変更

- パレット、バリエーション、置換ルール、色別ルール周辺のスクロールと横幅を調整。
- 評価用 UI Toolkit preview window を通常メニューから非表示化。
- ローカル QA 用アセットを `.gitignore` に追加。

## 1.0.0 - 2026-04-25

初回リリース。

### 追加

- Unity プロジェクト内の PNG / Texture2D アセットからのパレット抽出。
- Alpha Threshold、最小ピクセル数、量子化ステップ、最大パレット色数の設定。
- 近傍色グルーピング、目標グループ数、最大色距離の設定。
- グループ単位、色単位、Hybrid の置換モード。
- checkerboard 背景付き Before / After プレビューと選択色 overlay。
- スクロール可能なパレット一覧。
- Add、Duplicate、Remove、active 選択、Export / Skip を含む複数バリエーション管理。
- 単体 PNG 出力と、出力対象バリエーションの一括出力。
- セッション JSON の保存 / 読み込み。
- Help ウィンドウ、言語設定、Auto Preview。
- 検証用 UPM サンプルアイコン。
- Unity `6000.4.0f1` 検証ゲート。

### 既知の制限

- UI は IMGUI ベースです。
- 色距離は RGB ベースです。
- SpriteAtlas の直接編集は含みません。
- フォルダ単位の一括処理は含みません。
