# AGENTS.md - Unity Icon Palette Variant Generator

## 1. 目的

このファイルは、AI Agent / Codex が `Unity Icon Palette Variant Generator` の実装を進めるための指示書である。

本ツールは Unity Editor 上で画像の色を解析し、近傍色グルーピングと色置換により、単一アイコンから複数の色違いパターンを生成するエディタ拡張である。

## 2. 実装対象

対象は Unity Editor 拡張のみ。

ランタイム機能は実装しない。

推奨メニュー:

```text
Tools > Icon Tools > Palette Variant Generator
```

## 3. 参照すべきドキュメント

実装前に以下を読むこと。

1. `requirements.md`
2. `specification.md`
3. `architecture.md`
4. `development_plan.md`
5. `color_variant_rule.schema.json`
6. `SKILL.md`

## 4. 実装原則

- EditorWindow に全ロジックを詰め込まない。
- 画像解析、色抽出、グルーピング、置換、出力、JSON 保存は Service として分離する。
- Model は `[Serializable]` を基本とし、Unity の `JsonUtility` で保存しやすい構造にする。
- Dictionary をそのまま保存形式に使わない。
- 元画像を直接変更しない。
- 出力画像は別ファイルとして保存する。
- 一時生成した Texture2D の破棄漏れに注意する。
- Editor 専用コードは `Editor` フォルダ配下に配置する。

## 5. 推奨実装順

1. フォルダ構成作成
2. Model / enum 作成
3. Service の単体実装
4. EditorWindow の最小 UI 作成
5. Presenter 接続
6. プレビュー処理追加
7. PNG 出力追加
8. セッション保存 / 読み込み追加
9. バリエーション管理追加
10. テスト追加

## 6. コードスタイル

- C# の public class には概要コメントを付ける。
- public method には目的・引数・戻り値をコメントする。
- null チェックを省略しない。
- ファイル IO は例外処理を入れる。
- UI 表示文言は分かりやすく短くする。
- 命名は役割が分かるようにする。

例:

```csharp
public sealed class ColorExtractionService
{
    /// <summary>
    /// Texture2D から解析対象ピクセルの色を抽出し、出現数付きのパレットを生成する。
    /// </summary>
    public IReadOnlyList<PaletteColorEntry> Extract(Texture2D texture, AnalyzeSettings settings)
    {
        // implementation
    }
}
```

## 7. UI 実装指針

MVP は IMGUI でよい。

ただし、以下のパネル分割を意識すること。

- 上部: ツールバー
- 左: 画像情報 / 解析設定 / 出力設定
- 中央: Before / After プレビュー
- 右: パレット / グループ / バリエーション
- 下部: バリデーション / ログ

## 8. 重要な仕様

### 8.1 透明ピクセル

透明ピクセルは初期設定では解析・変換対象外にする。

Alpha Threshold 以下のピクセルは無視する。

### 8.2 色置換

基本式は以下。

```text
output = Lerp(original, target, ratio)
```

ratio は 0.0 ～ 1.0。

### 8.3 優先順位

色置換ルールの優先順位は以下。

1. 個別カラーコードルール
2. グループルール
3. 変換なし

### 8.4 PNG 出力

- 元画像と同じ幅・高さで出力する。
- アルファを維持する。
- 既存ファイルがある場合は Conflict Mode に従う。
- 出力後に AssetDatabase.Refresh を行う。

## 9. テスト対象

最低限、以下のテストを作成する。

- `ColorQuantizationServiceTests`
- `ColorDistanceServiceTests`
- `ColorExtractionServiceTests`
- `ColorReplacementServiceTests`
- `SessionJsonServiceTests`

## 10. 禁止事項

- 元画像ファイルを直接上書きしない。
- EditorWindow 内に重い処理をベタ書きしない。
- `Resources` フォルダ前提の実装にしない。
- `Application.dataPath` と `Assets/` 相対パスを混同しない。
- JSON に UnityEngine.Object 参照を直接保存しない。
- 画像解析時に毎フレーム処理を走らせない。

## 11. 完了時の確認

実装後、以下を確認する。

- Unity がコンパイルエラーなしで起動する。
- メニューからウィンドウが開く。
- PNG を選択して色抽出できる。
- グループ数を指定して自動グループ化できる。
- 色変更プレビューが表示される。
- PNG 出力できる。
- JSON 保存 / 読み込みできる。
- 元画像が変更されていない。

