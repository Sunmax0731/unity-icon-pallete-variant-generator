# Unity Icon Palette Variant Generator 仕様書

## 1. ツール概要

`Unity Icon Palette Variant Generator` は、Unity Editor 上で画像アセットの色を解析し、カラーパレットを生成・グルーピング・色変換・PNG 出力するための EditorWindow である。

想定メニュー:

```text
Tools > Palette Variant Generator > 開く
```

## 2. 基本操作フロー

```text
1. Source Image を選択
2. Analyze Colors を実行
3. Auto Group を実行
4. グループ数・色距離・除外条件を調整
5. 置換色・反映率を設定
6. Preview を確認
7. Variation を追加、または現在の設定で Export
8. 必要に応じて Session JSON を保存
```

## 3. 画面構成

## 3.1 全体レイアウト

```text
+--------------------------------------------------------------------------------+
| Toolbar                                                                         |
| [Source Image] [Analyze] [Auto Group] [Preview] [Export] [Save Session]          |
+--------------------------+-----------------------------+-----------------------+
| Left Panel               | Center Preview              | Right Panel           |
| - Image Info             | - Before                    | - Palette / Groups    |
| - Analyze Settings       | - After                     | - Replacement Rules   |
| - Group Settings         | - Selected Group Overlay    | - Variations          |
| - Export Settings        |                             |                       |
+--------------------------+-----------------------------+-----------------------+
| Bottom Report Panel                                                              |
| Warnings / Logs / Export Results                                                 |
+--------------------------------------------------------------------------------+
```

## 3.2 Toolbar

| UI | 内容 |
|---|---|
| Source Image | Texture2D / PNG を選択する ObjectField |
| Analyze | 色抽出を実行 |
| Auto Group | 近傍色グルーピングを実行 |
| Preview | 置換後プレビューを更新 |
| Export | 現在のバリエーションを PNG 出力 |
| Save Session | 現在設定を JSON 保存 |
| Load Session | 保存済み JSON を読み込み |

## 3.3 Left Panel

### Image Info

- Asset Path
- Size
- Format
- Alpha 有無
- Total Pixels
- Analyzed Pixels
- Ignored Transparent Pixels

### Analyze Settings

| 項目 | 型 | 初期値 | 内容 |
|---|---:|---:|---|
| Alpha Threshold | int 0-255 | 8 | この値以下のアルファを解析対象外にする |
| Minimum Pixel Count | int | 1 | 出現数が少なすぎる色の除外閾値 |
| Quantize Step | int | 4 | RGB の丸め単位。色数が多い画像で使用 |
| Max Palette Colors | int | 256 | 表示対象の最大色数 |

### Group Settings

| 項目 | 型 | 初期値 | 内容 |
|---|---:|---:|---|
| Target Group Count | int | 6 | 生成するグループ数 |
| Distance Mode | enum | RGB | 色距離の計算方式 |
| Preserve Dark Outline | bool | true | 暗い輪郭色を別グループとして保持する補助設定 |
| Preserve Alpha | bool | true | 出力時に元アルファを保持 |

### Export Settings

| 項目 | 型 | 初期値 | 内容 |
|---|---:|---:|---|
| Output Folder | string | `Assets/GeneratedIcons` | PNG 出力先 |
| File Prefix | string | source name | 出力ファイル接頭辞 |
| File Suffix | string | variation name | 出力ファイル接尾辞 |
| Conflict Mode | enum | Duplicate | 上書き / スキップ / 複製名 |
| Refresh AssetDatabase | bool | true | 出力後に AssetDatabase.Refresh を実行 |

## 3.4 Center Preview

- Before Preview
- After Preview
- Checker Background Toggle
- Zoom Slider
- Selected Group Overlay Toggle
- Pixel Grid Toggle

ドット絵や 64x64 アイコンでは、ズーム時にピクセル境界を確認できるようにする。

## 3.5 Right Panel

### Palette List

抽出色の一覧を表示する。

| 表示 | 内容 |
|---|---|
| Color Swatch | 色見本 |
| HEX | `#RRGGBB` |
| RGBA | `R,G,B,A` |
| Count | 出現ピクセル数 |
| Ratio | 画像内比率 |
| Group | 所属グループ |

### Group List

| 表示 | 内容 |
|---|---|
| Group Name | 任意名称 |
| Representative Color | 代表色 |
| Color Count | 含まれる色数 |
| Pixel Ratio | グループ全体の出現比率 |
| Replacement Mode | 一律 / 個別 |
| Target Color | 一律置換色 |
| Blend Ratio | 反映率 |

### Replacement Rules

#### Group Uniform Mode

グループ内の全カラーを同一の置換色に寄せる。

```text
output = Lerp(original, groupTargetColor, groupBlendRatio)
```

#### Per Color Mode

グループ内の個別カラーごとに置換色・反映率を指定する。

```text
output = Lerp(original, colorTargetColor, colorBlendRatio)
```

#### Hybrid Mode

個別カラー設定が存在する場合は個別設定を優先し、未設定カラーはグループ設定を使用する。

```text
if colorRule exists:
    use colorRule
else:
    use groupRule
```

## 4. データ処理仕様

## 4.1 画像読み込み

### 推奨読み込み方法

1. `AssetDatabase.GetAssetPath(texture)` でアセットパス取得
2. `File.ReadAllBytes(path)` で PNG 等のバイト列取得
3. `ImageConversion.LoadImage(texture2D, bytes)` で解析用 Texture2D 作成
4. `GetPixels32()` でピクセル取得

この方法により、元画像の TextureImporter の Read/Write 設定への依存を下げる。

## 4.2 色抽出

処理手順:

```text
for each pixel in image:
    if pixel.a <= alphaThreshold:
        ignore
    else:
        quantized = Quantize(pixel.rgb, quantizeStep)
        count[quantized]++
```

### Quantize

RGB 各成分を指定 step で丸める。

```text
quantizedR = round(r / step) * step
quantizedG = round(g / step) * step
quantizedB = round(b / step) * step
```

例:

- step = 1: 完全一致色を扱う
- step = 4: 近い色を軽くまとめる
- step = 8: アンチエイリアスを強めにまとめる

## 4.3 代表色の算出

グループ代表色は、グループ内カラーの出現数を重みとした加重平均とする。

```text
representative.r = sum(color.r * count) / sum(count)
representative.g = sum(color.g * count) / sum(count)
representative.b = sum(color.b * count) / sum(count)
```

## 4.4 近傍色グルーピング

### 実装

距離計算は `Distance Mode` で RGB / HSV / Lab を切り替えられる。初期値は RGB とし、既存の安定した結果を維持する。

RGB 距離:

```text
distance = sqrt((r1-r2)^2 + (g1-g2)^2 + (b1-b2)^2)
```

### Lab 距離

色の見た目に近いグルーピングにするため、将来的には CIE Lab 変換後の距離を使用する。

```text
distance = sqrt((L1-L2)^2 + (a1-a2)^2 + (b1-b2)^2)
```

### グループ数指定

ユーザーが `Target Group Count` を指定すると、その数になるようクラスタリングする。

例:

- 3: ベース色 / 影色 / ハイライト程度
- 5: 輪郭 / 濃影 / 中間 / 明部 / ハイライト
- 8: 色違いの精密調整向け

## 4.5 色変換

### 基本ルール

- 元ピクセルのアルファ値は維持する。
- 対象外ピクセルはそのまま出力する。
- 変換後 RGB は 0-255 にクランプする。

### 変換式

```text
output.r = original.r + (target.r - original.r) * ratio
output.g = original.g + (target.g - original.g) * ratio
output.b = original.b + (target.b - original.b) * ratio
output.a = preserveAlpha ? original.a : target.a
```

### 個別カラー優先順位

```text
1. カラーコード個別ルール
2. グループルール
3. 変換なし
```

## 5. バリエーション仕様

バリエーションは、同じ元画像・同じパレット構造に対して異なる置換ルールを持つデータである。

### バリエーション項目

| 項目 | 型 | 内容 |
|---|---|---|
| id | string | 内部 ID |
| displayName | string | 表示名 |
| fileSuffix | string | 出力ファイル名に使う接尾辞 |
| groupRules | array | グループ単位の置換ルール |
| colorRules | array | 色単位の置換ルール |

### 例

```text
iron_ore.png
copper_ore.png
silver_ore.png
mithril_ore.png
```

1 枚の鉱石アイコンから、色替えにより素材ランク違いを生成できる。

## 6. セッション JSON 仕様

保存対象:

- schemaVersion
- sourceImageAssetPath
- analyzeSettings
- groupSettings
- exportSettings
- paletteColors
- colorGroups
- variations

JSON スキーマ草案は `color_variant_rule.schema.json` を参照する。

## 7. 出力仕様

### PNG 出力

- `Texture2D.EncodeToPNG()` を使用する。
- 出力先が存在しない場合は作成する。
- 出力後に `AssetDatabase.Refresh()` を行う。
- 必要に応じて `TextureImporter` の設定を自動調整する余地を残す。

### ファイル名規則

```text
{prefix}_{variationName}.png
```

例:

```text
ore_copper.png
ore_iron.png
ore_mithril.png
```

### フォルダ一括出力

- `Source Folder` に指定した Unity プロジェクト内フォルダから `Texture2D` を検索する。
- 各ソース画像を解析し、現在のグループ設定と有効な Variation を適用する。
- 出力名は `{sourceFileName}_{variationSuffix}.png` とする。
- 既定の競合モードは `Duplicate` のため、元画像は上書きしない。
- 成功、スキップ、失敗はソースと Variation の組み合わせごとに記録する。

### Conflict Mode

| Mode | 内容 |
|---|---|
| Overwrite | 既存ファイルを上書き |
| Skip | 既存ファイルがある場合は出力しない |
| Duplicate | `name_001.png` のように複製名で出力 |

## 8. エラー・警告仕様

| コード | 内容 | 対応 |
|---|---|---|
| W001 | Source Image が未指定 | 画像選択を促す |
| W002 | 解析対象ピクセルが 0 | Alpha Threshold の見直しを促す |
| W003 | 色数が多すぎる | Quantize Step の増加を提案 |
| W004 | 出力先が未指定 | Output Folder を指定させる |
| W005 | 同名ファイルが存在 | Conflict Mode に従って処理 |
| W006 | グループが空 | Auto Group 再実行を提案 |
| W007 | セッション内の画像パスが見つからない | 手動再指定を促す |

## 9. MVP 画面で最低限必要な UI

- Source Image ObjectField
- Analyze Button
- Auto Group Button
- Target Group Count IntField
- Palette / Group List
- Group Replacement ColorField
- Group Blend Ratio Slider
- Before Preview
- After Preview
- Output Folder Field
- Export Button
- Save / Load Session Button
- Report Panel

## 10. 今後の仕様拡張

### 10.1 色相寄せモード

元画像の陰影を保ったまま、色相だけをターゲット色に寄せるモード。

### 10.2 Outline Protection

暗い輪郭色や黒線を自動判定して変換対象から外す。

### 10.3 Batch Variant Table

表形式で複数バリエーションを作成する。

```text
Variation | Group A | Group B | Group C | Export
Copper    | #B87333 | #7A4A28 | #FFD19A | true
Iron      | #7A6A5F | #4A403A | #C0B8AE | true
```

### 10.4 ScriptableObject Preset

JSON に加えて ScriptableObject としてプリセット保存できるようにする。

