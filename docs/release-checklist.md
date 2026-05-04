# リリースチェックリスト

## 前提

- Version: `1.1.0`
- Unity: `6000.4.0f1`
- License: MIT License
- GitHub Release には ZIP と `.unitypackage` の両方を添付する。

## 1. ドキュメント

- `README.md` が v1.1.0 を参照している。
- `Packages/com.sunmax0731.icon-palette-variant-generator/README.md` が v1.1.0 の機能に一致している。
- `CHANGELOG.md` に v1.1.0 がある。
- `docs/manual.md` にレイヤー、読み込み画像直接編集、塗りつぶし、JPEG 透過、Export alpha 設定が記載されている。
- `docs/release-notes-v1.1.0.md` がある。
- `docs/validation-checklist.md` が Issue #48 marker を含む。
- `docs/booth-copy.md` が BOOTH Product Page Autofill の入力形式に沿っている。
- `Agents.md` と `Skill.md` が最新の責務分離とリリース方針を含む。

## 2. 自動検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

`Validation/scaffold-validation.txt` または Unity log にすべての `ISSUE*_VALIDATION=PASS` marker が出力されることを確認する。

## 3. リリース成果物生成

```powershell
powershell -ExecutionPolicy Bypass -File tools\release\build-release.ps1 -Version 1.1.0
powershell -ExecutionPolicy Bypass -File tools\release\test-release-package.ps1 -Version 1.1.0
```

生成物:

```text
ReleaseBuilds/PaletteVariantGenerator_v1.1.0.zip
ReleaseBuilds/PaletteVariantGenerator_v1.1.0.unitypackage
```

## 4. 成果物確認

- ZIP に `Packages/com.sunmax0731.icon-palette-variant-generator/package.json` が含まれている。
- ZIP に `README.md`、`LICENSE.md`、`CHANGELOG.md`、`docs/manual.md`、`docs/release-checklist.md`、`docs/validation-checklist.md` が含まれている。
- ZIP に `Assets/`、`Library/`、`Logs/`、`Temp/`、`Validation/`、`ReleaseBuilds/` が含まれていない。
- `.unitypackage` が 0 byte ではない。
- Release 公開時は `gh release view v1.1.0 --json assets` で ZIP と `.unitypackage` の両方を確認する。

## 5. 手動 QA

- メニューからメイン画面、ライセンス、バージョン情報が開く。
- サンプル PNG を Analyze / Auto Group / Preview / Export できる。
- JPEG 読み込み画像へ消しゴムを使い、透明化結果が PNG に出力される。
- Brush / Eraser / Fill / Blur / Smooth / Noise Removal が Preview 上で反応する。
- Paint Layer / Image Layer、Visible / Lock、Opacity が Preview / Export に反映される。
- Session Save / Load でレイヤーと読み込み画像への直接編集が復元される。
- `Assets/` 配下へ Export した PNG の `Alpha Is Transparency` が ON になる。

## 6. 公開前確認

- 未追跡のユーザー検証用 `Assets/` ファイルをコミットしていない。
- GitHub Issue に検証結果をコメントする。
- 必要に応じて Issue を close する。
- BOOTH 商品ページには `docs/booth-copy.md` を使い、タグ、価格、カテゴリ、作品ファイルは BOOTH 側で手動設定する。
