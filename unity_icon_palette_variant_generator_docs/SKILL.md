# SKILL.md - Unity Icon Palette Variant Generator Development Skill

## 1. この Skill の目的

この Skill は、Unity Editor 拡張 `Unity Icon Palette Variant Generator` を安全かつ保守しやすく開発するためのルールを定義する。

本ツールは、Unity プロジェクト内の画像から色を抽出し、近傍色をグルーピングし、グループ単位またはカラーコード単位の色置換でアイコンの色違いを生成する。

## 2. 必ず守ること

- 元画像アセットを直接変更しない。
- 出力画像は別ファイルとして保存する。
- 透明ピクセルを意図せず塗らない。
- 解析用 Texture と表示用 Texture を区別する。
- EditorWindow に処理を集中させない。
- Service 単位でテストしやすい設計にする。
- JSON 保存時は UnityEngine.Object 参照を保存しない。
- Unity 相対パス `Assets/...` と OS 絶対パスを混同しない。

## 3. 推奨アーキテクチャ

MVP 風の構成を使う。

```text
View        : EditorWindow / UI
Presenter   : UI 操作と処理の接続
Model       : セッション、パレット、グループ、設定
Service     : 画像解析、色距離、置換、出力、保存
Utility     : 色コード、パス、プレビュー補助
```

## 4. 実装時の判断基準

### UI

MVP では IMGUI でよい。

商品化の段階で UI Toolkit 化を検討する。

### 色距離

初期実装は RGB 距離でよい。

見た目の自然さを高める段階で Lab 距離を追加する。

### グルーピング

初期実装では簡易 K-Means でよい。

グループ結果を手動修正できる余地を残す。

### JSON

Unity の `JsonUtility` で扱いやすいよう、List 中心の構造にする。

## 5. 品質チェック

実装・修正後は以下を確認する。

- 64x64 の透過 PNG で動作する。
- 128x128 の透過 PNG で動作する。
- 透明部分が透明のまま出力される。
- 反映率 0 / 0.5 / 1.0 が正しく動く。
- グループ数指定が反映される。
- セッション JSON の保存と読み込みができる。
- 画像出力後に Project ビューへ反映される。

## 6. エラー対応方針

- ユーザーが直せる問題は Report Panel に警告として出す。
- ファイル IO の例外は握りつぶさず、メッセージとして表示する。
- Export 不可能な状態では Export ボタンを無効化するか、押下時に明確な理由を出す。

## 7. 追加機能を入れるときの基準

追加機能は以下の基準で判断する。

- アイコン色違い生成の効率が上がるか
- 元画像非破壊の原則を守れるか
- UI が過度に複雑にならないか
- MVP の安定性を落とさないか

## 8. 将来拡張候補

- 一括変換
- フォルダ単位処理
- SpriteAtlas 連携
- Hue/Saturation/Value 操作
- Outline Protection
- Before/After スライダー
- Preset ScriptableObject
- UI Toolkit 化

