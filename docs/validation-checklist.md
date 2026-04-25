# 検証チェックリスト

## 実行条件

- 日付: 2026-04-25
- Unity: 6000.4.0f1
- Repository: `unity-icon-pallete-variant-generator`
- 検証コマンド:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

## サンプルアセット

サンプルは以下に格納しています。

```text
Packages/com.sunmax0731.icon-palette-variant-generator/Samples~/SampleIcons
```

| サンプル | 目的 | 結果 |
|---|---|---|
| `transparent_64.png` | 64x64 透明 PNG | PASS |
| `transparent_128.png` | 128x128 透明 PNG | PASS |
| `antialias_128.png` | アンチエイリアスあり透明 PNG | PASS |
| `pixel_art_64.png` | ドット絵風透明 PNG | PASS |

## 自動検証結果

| 確認項目 | 結果 |
|---|---|
| Unity がコンパイルエラーなしで起動する | PASS |
| validation entry point から EditorWindow を開ける | PASS |
| 生成サンプルでパレット抽出できる | PASS |
| 自動グルーピング service が検証されている | PASS |
| グループ単位 / 色単位 / Hybrid 置換が検証されている | PASS |
| PNG 出力が検証されている | PASS |
| セッション JSON の保存 / 読み込みが検証されている | PASS |
| バリエーションと一括出力 snapshot が検証されている | PASS |
| サンプル PNG のバイト列が validation 後も変わらない | PASS |

## 手動 QA 項目

| ワークフロー | 結果 | 備考 |
|---|---|---|
| `Tools > Palette Variant Generator > 開く` から開く | PASS | headless で EditorWindow 作成を検証。リリース前に目視確認を推奨。 |
| サンプル PNG を Analyze する | PASS | サンプル抽出 validation で検証。 |
| Auto Group | PASS | グルーピング validation で検証。 |
| Preview replacement | PASS | 置換 validation で検証。 |
| Export PNG | PASS | PNG export validation で検証。 |
| Save Session / Load Session | PASS | JSON validation で検証。 |
| Language menu / Help window | PASS | compile と window code path で検証。リリース前に目視確認を推奨。 |
| Palette scroll | PASS | UI 実装済み。リリース前に目視確認を推奨。 |
| Variation Add / Duplicate / Remove | PASS | variation service validation で検証。 |
| Variation Active / Export state display | PASS | issue 12 variation UX validation で検証。目視確認を推奨。 |
| Auto Preview debounce | PASS | issue 13 debounce validation で検証。目視確認を推奨。 |
| プレビューのズーム / パン / split 比較 | PASS | issue 17 preview navigation validation で検証。目視確認を推奨。 |
| 置換ルールプリセット export / import | PASS | issue 18 rule preset validation で検証。 |
| ScriptableObject プリセットアセット | PASS | issue 22 preset asset validation で作成、適用、更新を検証。 |
| ドッキング向け compact layout | PASS | issue 23 docked layout validation で最小サイズと切り替え条件を検証。wide / compact の目視確認を推奨。 |
| UI Toolkit プレビュー Window | PASS | issue 23 UI Toolkit preview validation で評価用 window と主要セクション構成を検証。本番操作は IMGUI 版を使用。 |
| UI Toolkit 標準コントロール同等性 | PASS | ObjectField / ColorField / PopupField / ScrollView / Slider と preview pan / wheel zoom / split compare の入力受け口を検証。 |
| Main Window UI Toolkit ホスト | PASS | 本番 window が UI Toolkit root と ScrollView で開き、IMGUIContainer に依存しないことを検証。 |
| UI Toolkit 本番導入 | PASS | issue 25 production UI Toolkit validation で、主要セクションと ObjectField / ColorField / ScrollView が本番 window に存在することを検証。 |
| UI Toolkit プレビューメニュー非表示 | PASS | issue 25 preview menu hidden validation で、評価用 preview window が通常メニューに表示されないことを検証。 |
| ノイズ削除 | PASS | issue 26 noise removal validation で、小さな色領域を同一グループ内の近傍色で補正できることを検証。 |
| エッジ外側クリーンアップ | PASS | issue 27 edge outside cleanup validation で、本体外側近傍の小領域を透明化できることを検証。 |
| Export 詳細設定の必要時表示 | PASS | issue 28 export UI disclosure validation で、書き出し関連の詳細設定が初期状態で折りたたまれることを検証。 |
| Release Package workflow / ZIP 検証 | PASS | issue 24 release automation validation と `test-release-package.ps1` で検証。 |
| パレット色の手動グループ移動 | PASS | issue 19 manual group editing validation で検証。目視確認を推奨。 |
| Export All | PASS | variation snapshot と export service validation で検証。 |

## リリース blocker

- 自動検証ゲートで検出された code blocker はありません。
- 残りの改善作業は GitHub Issue で管理します。

## 既知の制限

- 初回リリースの UI は IMGUI ベースです。
- 色距離は RGB / HSV / Lab から選択できます。Lab 色距離は知覚差に近い近傍色判定として検証対象です。
- SpriteAtlas の直接編集は v1.0.1 の対象外です。
- フォルダ単位の一括処理は Texture2D アセットを対象に検証します。
