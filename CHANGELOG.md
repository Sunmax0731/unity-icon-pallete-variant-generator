# 変更履歴

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
