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
- GitHub Issue の優先順位を確認してから実装に入る。
- ユーザーが検証用に追加した未追跡アセットは、明示されない限りコミットしない。

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

### 置換モード

- `GroupUniform`: グループの Target Color / Blend Ratio をグループ内の全色に適用する。
- `PerColor`: 有効な個別色ルールだけを適用し、未設定色は変更しない。
- `Hybrid`: 有効な個別色ルールを優先し、未設定色はグループ設定へフォールバックする。

### 近傍色しきい値

`Max Color Distance` は、自動グルーピング後に代表色から遠い色を別グループへ分離するために使う。

## 5. 品質チェック

実装・修正後は以下を確認する。

- 64x64 の透過 PNG で動作する。
- 128x128 の透過 PNG で動作する。
- 透明部分が透明のまま出力される。
- 反映率 0 / 0.5 / 1.0 が正しく動く。
- グループ数指定が反映される。
- セッション JSON の保存と読み込みができる。
- 画像出力後に Project ビューへ反映される。
- 言語メニューと Help window が開く。
- Palette がスクロールできる。
- `Max Color Distance` の変更でグループ結果が変わる。
- `GroupUniform` / `PerColor` / `Hybrid` の置換優先順位が崩れていない。

標準検証:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

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

## 9. Release 前の必須順序

1. `#7` 複数バリエーション管理と一括出力を完了する。
2. `#8` 手動 QA、サンプル、検証チェックリストを完了する。
3. `#9` README / CHANGELOG / Manual / Terms / BOOTH copy / GitHub Release を整備する。
