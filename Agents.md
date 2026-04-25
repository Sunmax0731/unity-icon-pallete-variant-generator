# AGENTS.md - Unity Icon Palette Variant Generator

## 1. 目的

このファイルは、AI Agent / Codex が `Unity Icon Palette Variant Generator` の実装を進めるための指示書である。

本ツールは Unity Editor 上で画像の色を解析し、近傍色のグルーピングと色置換により、単一アイコンから複数の色違いパターンを生成するエディタ拡張である。

## 2. 実装対象

対象は Unity Editor 拡張のみ。

ランタイム機能は実装しない。

推奨メニュー:

```text
Tools > Palette Variant Generator > 開く
Tools > Palette Variant Generator
```

## 3. 参照すべきドキュメント

実装前に以下を確認する。

1. `docs/requirements.md`
2. `docs/specification.md`
3. `docs/architecture.md`
4. `docs/development_plan.md`
5. `docs/color_variant_rule.schema.json`
6. `Skill.md`
7. 作業工程に対応する `docs/skills/*.md`

## 4. 実装原則

- GitHub Issue を確認し、Issue 起点で作業する。
- Issue は日本語で作成・更新する。タイトル、本文、コメント、完了報告も日本語を基本とする。
- 実装 Issue ごとに作業範囲を区切る。
- Unity `6000.4.0f1` で検証してから commit / close する。
- 検証用にユーザーが追加した `Assets/` 配下の画像は、タスクで明示されない限りコミットしない。
- EditorWindow に全ロジックを詰め込まない。
- 画像解析、色抽出、グルーピング、置換、出力、JSON 保存は Service として分離する。
- Model は `[Serializable]` を基本とし、Unity の `JsonUtility` で保存しやすい構造にする。
- Dictionary をそのまま保存形式に使わない。
- 元画像を直接変更しない。
- 出力画像は別ファイルとして保存する。
- 一時生成した `Texture2D` の破棄漏れに注意する。
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
- public method には目的、引数、戻り値が分かるコメントを付ける。
- null チェックを省略しない。
- ファイル IO は例外処理を入れる。
- UI 表示文言は分かりやすく短くする。
- ユーザー向け UI 文言は日本語を基本とする。
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

## 7. UI 実装方針

MVP は IMGUI でよい。

現行 UI は以下のパネル分割を基本にする。

- 上部: Source / Analyze / Auto Group / Preview / Export / Session / Help / Language
- 左: 解析設定 / グループ設定 / 出力設定 / 画像情報
- 中央: Before / After プレビュー / スクロール可能な Palette
- 右: グループ置換ルール / 色別置換ルール / バリエーション

選択中のグループまたは色は、プレビュー上の overlay と連動させる。

## 8. 重要な仕様

### 8.1 透明ピクセル

透明ピクセルは初期設定では解析・変換対象外にする。

Alpha Threshold 以下のピクセルは無視する。

### 8.2 色置換

基本式は以下。

```text
output = Lerp(original, target, ratio)
```

ratio は 0.0 から 1.0。

### 8.3 優先順位

色置換ルールの優先順位は以下。

1. `GroupUniform`: グループルールを適用する。
2. `PerColor`: 有効な個別カラーコードルールのみ適用する。
3. `Hybrid`: 有効な個別カラーコードルールを優先し、未設定色はグループルールへフォールバックする。
4. 該当ルールがない場合は変換しない。

### 8.4 PNG 出力

- 元画像と同じ幅・高さで出力する。
- アルファを維持する。
- 既存ファイルがある場合は Conflict Mode に従う。
- 出力後に `AssetDatabase.Refresh` を行う。

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
- JSON に `UnityEngine.Object` 参照を直接保存しない。
- 画像解析時に毎フレーム重い処理を走らせない。

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

標準検証コマンド:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

現時点の主要マーカー:

```text
ISSUE1_SCAFFOLD_VALIDATION=PASS
ISSUE2_IMAGE_PALETTE_VALIDATION=PASS
ISSUE3_COLOR_GROUPING_VALIDATION=PASS
ISSUE4_REPLACEMENT_PREVIEW_VALIDATION=PASS
ISSUE5_PNG_EXPORT_VALIDATION=PASS
ISSUE6_SESSION_JSON_VALIDATION=PASS
ISSUE7_VARIATION_BATCH_EXPORT_VALIDATION=PASS
ISSUE8_SAMPLE_QA_VALIDATION=PASS
ISSUE10_UI_POLISH_VALIDATION=PASS
```

## 12. 残タスク方針

GitHub Issue の open 状態を確認し、優先度と実行可能性が高いものから進める。
