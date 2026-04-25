# 02 - Core Implementation Skill

## 目的

画像解析、色抽出、グルーピング、置換、PNG 出力、JSON 保存などの中核処理を実装する工程の Skill。

## 配置ルール

- Models: `Packages/com.sunmax0731.icon-palette-variant-generator/Runtime/Models`
- Services: `Packages/com.sunmax0731.icon-palette-variant-generator/Runtime/Services`
- Utilities: `Packages/com.sunmax0731.icon-palette-variant-generator/Runtime/Utilities`
- Editor-only asset loading: `Packages/com.sunmax0731.icon-palette-variant-generator/Editor/Services`

## 実装原則

- `EditorWindow` にアルゴリズムを直接書かない。
- Service は EditMode テストしやすい API にする。
- Model は `[Serializable]` を基本にし、`JsonUtility` で保存しやすい List 中心の構造にする。
- Dictionary や `UnityEngine.Object` 参照を保存形式にしない。
- 一時生成した `Texture2D` は破棄責任を明確にする。

## 機能仕様

### 色抽出

- `Alpha Threshold` 以下のピクセルは解析対象外。
- `Quantize Step` でアンチエイリアス由来の近似色をまとめる。
- 出現数と出現率を保持する。

### グルーピング

- 初期実装は RGB 距離と簡易 K-Means を基本にする。
- `Max Color Distance` で代表色から遠い色を別グループへ分離する。
- グループ ID と PaletteColorEntry の `groupId` を同期する。

### 色置換

```text
output = Lerp(original, target, blendRatio)
```

- `GroupUniform`: グループの Target Color / Blend Ratio をグループ内の全色に適用する。
- `PerColor`: 有効な個別色ルールだけを適用し、未設定色は変更しない。
- `Hybrid`: 有効な個別色ルールを優先し、未設定色はグループ設定へフォールバックする。
- `preserveAlpha` が有効な場合、元ピクセルの alpha を維持する。

## 追加時の検証

- Service ごとに focused EditMode test を追加または更新する。
- `PaletteVariantGeneratorValidation` に headless marker を追加する。
- `tools/validation/run-editmode-tests.ps1` の marker チェックを更新する。
