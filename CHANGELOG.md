# 変更履歴

## 未リリース

### 追加

- ノイズ削除: 同一グループ内に囲まれた小さな色領域を、Preview / Export 前に近傍色で補正する機能を追加。
- メインウィンドウを本番 UI Toolkit 構成へ移行し、主要セクションと操作ボタンを UI Toolkit 上で直接表示するように変更。
- エッジ外側クリーンアップ: 主要な本体領域の外側近傍にある小領域を Preview / Export 前に透明化する機能を追加。
- UI 整理: Export Settings と Folder Batch Export を通常時は折りたたみ、必要時に表示する構成へ変更。

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
