# Unity Icon Palette Variant Generator 開発計画

## 現在の到達点

v1.1.0 では、当初の色替えワークフローに加えて、Unity Editor 拡張としての画面再設計、レイヤー、描画ツール、読み込み画像への直接編集、JPEG 透過編集、塗りつぶし、Export alpha import 設定まで実装済みです。

## フェーズ

| Phase | 内容 | 状態 |
|---|---|---|
| 0 | パッケージ構成、menu、基本 Window | 完了 |
| 1 | 画像読み込み、プレビュー、画像情報表示 | 完了 |
| 2 | 色抽出、パレット表示、透明ピクセル除外 | 完了 |
| 3 | 近傍色グルーピング、代表色、距離設定 | 完了 |
| 4 | 色置換、Before / After Preview | 完了 |
| 5 | PNG Export、Conflict Mode、AssetDatabase.Refresh | 完了 |
| 6 | Session Save / Load | 完了 |
| 7 | Variation 管理、Export All | 完了 |
| 8 | UI polish、Help、Language、Release packaging | 完了 |
| 9 | レイヤー、描画ツール、読み込み画像直接編集 | 完了 |
| 10 | JPEG 透過編集、塗りつぶし、Export alpha import | 完了 |
| 11 | v1.1.0 リリース準備 | 完了 |

## v1.1.0 の実装対象

- UnityEditor 拡張デザインガイドラインに沿った画面構成。
- Preview 上の Brush / Eraser / Blur / Smooth / NoiseRemoval。
- Paint bucket 相当の Fill。
- 描画レイヤー / 画像レイヤー。
- 読み込み画像への直接編集。
- JPEG 読み込み画像の内部 RGBA 化。
- Export PNG の alpha 保持。
- `Assets/` 配下 Export 後の `Alpha Is Transparency` 自動 ON。
- 描画処理の `RasterPaintService` / `PaintStrokeSessionService` 分離。
- 日本語 UI 文言の拡充。
- 手動確認ドキュメントと BOOTH 紹介文の更新。

## リリース前チェック

1. open Issue を確認する。
2. 変更差分を確認する。
3. `tools\validation\run-editmode-tests.ps1` を実行する。
4. `tools\release\build-release.ps1 -Version 1.1.0` を実行する。
5. `tools\release\test-release-package.ps1 -Version 1.1.0` を実行する。
6. `ReleaseBuilds/PaletteVariantGenerator_v1.1.0.zip` と `.unitypackage` を確認する。
7. GitHub Issue #48 に検証結果をコメントする。
8. commit / push する。
9. GitHub Release `v1.1.0` を作成する場合は、ZIP と `.unitypackage` の両方を添付する。

## 今後の候補

- 大きな画像での描画性能改善。
- ブラシ形状の追加。
- レイヤーマスク。
- UI Toolkit への段階的な再整理。
- SpriteAtlas / Addressables 連携。
- BOOTH 商品画像テンプレートの追加。

## 配布物

```text
ReleaseBuilds/
  PaletteVariantGenerator_v1.1.0.zip
  PaletteVariantGenerator_v1.1.0.unitypackage
```

ZIP には UPM パッケージ、README、CHANGELOG、manual、license を含める。.unitypackage は Unity Editor への直接 import 用として提供する。
