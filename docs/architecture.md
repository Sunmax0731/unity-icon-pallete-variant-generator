# Unity Icon Palette Variant Generator 設計書

## 1. 設計方針

本ツールは Unity Editor 拡張として実装する。EditorWindow にすべての処理を詰め込まず、MVP 風の責務分割を行う。

基本方針:

- View: EditorWindow / UI Toolkit / IMGUI の表示と入力受付
- Presenter: UI 入力を受け取り、Model と Service を呼び出す
- Model: 現在の状態、パレット、グループ、バリエーション、出力設定を保持
- Service: 画像解析、色距離計算、グルーピング、変換、入出力を担当

## 2. 推奨フォルダ構成

```text
Agents.md
Skill.md
README.md
docs/
  README.md
  requirements.md
  specification.md
  architecture.md
  development_plan.md
  color_variant_rule.schema.json
tools/
  validation/
    run-editmode-tests.ps1
Packages/
  com.sunmax0731.icon-palette-variant-generator/
    package.json
    README.md
    Runtime/
      Models/
        PaletteVariantSession.cs
        PaletteColorEntry.cs
        ColorGroup.cs
        ColorReplacementRule.cs
        IconVariation.cs
        AnalyzeSettings.cs
        GroupSettings.cs
        ExportSettings.cs
      Services/
        ColorExtractionService.cs
        ColorQuantizationService.cs
        ColorDistanceService.cs
        ColorGroupingService.cs
        ColorReplacementService.cs
        PngExportService.cs
        SessionJsonService.cs
      Utilities/
        ColorCodeUtility.cs
    Editor/
      Services/
        TextureAssetLoader.cs
      Validation/
        PaletteVariantGeneratorValidation.cs
      Windows/
        PaletteVariantGeneratorWindow.cs
    Tests/
      Editor/
        ColorExtractionServiceTests.cs
        ColorGroupingServiceTests.cs
        ColorReplacementServiceTests.cs
        SessionJsonServiceTests.cs
```

## 3. 主要クラス

## 3.1 PaletteVariantGeneratorWindow

EditorWindow 本体。

責務:

- メニューから起動
- UI 描画
- Presenter の生成
- ユーザー操作のイベント通知
- プレビュー Texture の表示

避けること:

- 画像解析処理を直接書かない
- JSON 保存処理を直接書かない
- グルーピングアルゴリズムを直接書かない

## 3.2 IPaletteVariantGeneratorView

View と Presenter を分離するためのインターフェース。

想定メソッド:

```csharp
public interface IPaletteVariantGeneratorView
{
    void SetSession(PaletteVariantSession session);
    void SetBeforePreview(Texture2D texture);
    void SetAfterPreview(Texture2D texture);
    void SetReportMessages(IReadOnlyList<ValidationMessage> messages);
    void RepaintView();
}
```

## 3.3 PaletteVariantGeneratorPresenter

UI 操作を受け取り、各 Service を呼び出す中核クラス。

主な処理:

- Source Image 設定
- Analyze 実行
- Auto Group 実行
- Preview 更新
- Export 実行
- Save / Load Session 実行
- Validation 実行

## 3.4 PaletteVariantSession

ツール全体の状態を保持するルートモデル。

```csharp
[Serializable]
public sealed class PaletteVariantSession
{
    public string schemaVersion;
    public string sourceImageAssetPath;
    public AnalyzeSettings analyzeSettings;
    public GroupSettings groupSettings;
    public ExportSettings exportSettings;
    public List<PaletteColorEntry> paletteColors;
    public List<ColorGroup> colorGroups;
    public List<IconVariation> variations;
    public string activeVariationId;
}
```

## 3.5 PaletteColorEntry

抽出された色の情報。

```csharp
[Serializable]
public sealed class PaletteColorEntry
{
    public string id;
    public Color32 color;
    public string hex;
    public int pixelCount;
    public float pixelRatio;
    public string groupId;
    public bool lockedGroup;
    public bool ignored;
}
```

## 3.6 ColorGroup

近傍色グループ。

```csharp
[Serializable]
public sealed class ColorGroup
{
    public string id;
    public string displayName;
    public Color32 representativeColor;
    public List<string> colorEntryIds;
    public ColorReplacementMode replacementMode;
    public Color32 targetColor;
    public float blendRatio;
    public bool lockedGroup;
}
```

## 3.7 ColorReplacementRule

色変換ルール。

```csharp
[Serializable]
public sealed class ColorReplacementRule
{
    public string id;
    public string groupId;
    public string colorEntryId;
    public ColorReplacementScope scope;
    public Color32 targetColor;
    public float blendRatio;
    public bool enabled;
}
```

## 3.8 IconVariation

1 つの出力パターン。

```csharp
[Serializable]
public sealed class IconVariation
{
    public string id;
    public string displayName;
    public string fileSuffix;
    public List<ColorReplacementRule> replacementRules;
    public bool exportEnabled;
}
```

## 4. Enum 定義

```csharp
public enum ColorDistanceMode
{
    Rgb,
    Hsv,
    Lab
}

public enum ColorReplacementMode
{
    GroupUniform,
    PerColor,
    Hybrid
}

public enum ColorReplacementScope
{
    Group,
    ColorEntry
}

public enum ExportConflictMode
{
    Overwrite,
    Skip,
    Duplicate
}
```

## 5. Service 設計

## 5.1 TextureAssetLoader

責務:

- Unity アセットパスから画像を読み込む
- 解析用 Texture2D を生成する
- TextureImporter の Read/Write に依存しない読み込みを行う

主要メソッド:

```csharp
Texture2D LoadReadableTexture(string assetPath);
bool TryLoadReadableTexture(string assetPath, out Texture2D texture, out string error);
```

## 5.2 ColorExtractionService

責務:

- ピクセルを走査して色を集計
- 透明ピクセル除外
- 最小ピクセル数除外
- 出現率計算

主要メソッド:

```csharp
IReadOnlyList<PaletteColorEntry> Extract(Texture2D texture, AnalyzeSettings settings);
```

## 5.3 ColorQuantizationService

責務:

- 色の丸め
- 色数が多すぎる場合の整理

主要メソッド:

```csharp
Color32 Quantize(Color32 color, int step);
```

## 5.4 ColorDistanceService

責務:

- 色距離計算
- RGB / HSV / Lab の切り替え

主要メソッド:

```csharp
float Calculate(Color32 a, Color32 b, ColorDistanceMode mode);
```

## 5.5 ColorGroupingService

責務:

- 近傍色の自動グルーピング
- 指定グループ数への集約
- 代表色算出

主要メソッド:

```csharp
IReadOnlyList<ColorGroup> CreateGroups(
    IReadOnlyList<PaletteColorEntry> colors,
    GroupSettings settings);
```

MVP では以下の簡易実装でもよい。

1. 出現数の多い色を初期代表色にする
2. 各色を最も近い代表色に割り当てる
3. 代表色を加重平均で更新する
4. 数回繰り返す

## 5.6 ColorReplacementService

責務:

- 変換ルールをピクセルへ適用
- グループルールと個別カラールールの優先順位を処理
- アルファ維持

主要メソッド:

```csharp
Texture2D Apply(
    Texture2D source,
    PaletteVariantSession session,
    IconVariation variation);
```

### 変換対象の特定

解析時に量子化した色と同じ方法で、ピクセル色をキー化して `PaletteColorEntry` に対応付ける。

## 5.7 PreviewTextureService

責務:

- Before / After プレビュー Texture の生成
- 選択グループのハイライト表示
- チェッカー背景表示の補助

## 5.8 PngExportService

責務:

- PNG エンコード
- 出力先作成
- Conflict Mode 処理
- AssetDatabase.Refresh

主要メソッド:

```csharp
ExportResult Export(Texture2D texture, ExportSettings settings, string fileName);
```

## 5.9 SessionJsonService

責務:

- Session JSON 保存
- Session JSON 読み込み
- schemaVersion 確認

主要メソッド:

```csharp
void Save(string path, PaletteVariantSession session);
PaletteVariantSession Load(string path);
```

## 5.10 ValidationService

責務:

- 現在状態の警告・エラー一覧作成
- Export 実行可否判定

主要メソッド:

```csharp
IReadOnlyList<ValidationMessage> Validate(PaletteVariantSession session);
bool CanExport(PaletteVariantSession session, out IReadOnlyList<ValidationMessage> messages);
```

## 6. データ更新イベント

Model にイベントを持たせる場合の例:

```csharp
public event Action SessionChanged;
public event Action PaletteChanged;
public event Action GroupsChanged;
public event Action PreviewChanged;
```

ただし EditorWindow では、最初は Presenter 側で明示的に `view.RepaintView()` を呼ぶ構成でもよい。

## 7. UI 実装方針

### MVP

IMGUI ベースの EditorWindow で開始してよい。

理由:

- 実装が早い
- Editor 拡張の初期検証に向く
- List 表示、ColorField、Slider、ObjectField が簡単

### 将来

UI Toolkit 化を検討する。

理由:

- 商品として見た目を整えやすい
- パネル分割や ListView が作りやすい
- BOOTH 商品として印象を上げやすい

## 8. テスト方針

### Unit Test

- ColorQuantizationService
- ColorDistanceService
- ColorExtractionService
- ColorGroupingService
- ColorReplacementService
- SessionJsonService

### Editor Test

- PNG 読み込み
- PNG 出力
- AssetDatabase.Refresh 後のアセット確認
- セッション保存・復元

### 手動確認

- 64x64 アイコン
- 128x128 アイコン
- 透過 PNG
- アンチエイリアスあり画像
- ドット絵画像
- 色数が多い画像

## 9. 実装時の注意点

- `Texture2D` の破棄漏れに注意する。
- Editor 上で生成した一時 Texture は `DestroyImmediate` を検討する。
- `Color` と `Color32` の変換による丸め誤差に注意する。
- PNG 出力前に `Texture2D.Apply()` を呼ぶ。
- Unity の `JsonUtility` は Dictionary 非対応のため、保存形式は List 中心にする。
- ファイルパスは `Assets/` から始まる Unity 相対パスと OS 絶対パスを明確に区別する。
