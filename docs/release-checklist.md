# リリースチェックリスト

## 前提

- Unity `6000.4.0f1` で検証する。
- GitHub Actions の `Release Package` workflow は、tracked files から配布 ZIP を生成する。
- Unity ライセンスを使う EditMode 検証は、現時点ではローカルまたは Unity を利用できる runner で実行する。

## ローカル検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

`Validation/scaffold-validation.txt` にすべての `ISSUE*_VALIDATION=PASS` marker が出ていることを確認する。

## 配布 ZIP 生成

```powershell
powershell -ExecutionPolicy Bypass -File tools\release\build-release.ps1 -Version 1.0.1
powershell -ExecutionPolicy Bypass -File tools\release\test-release-package.ps1 -Version 1.0.1
```

`ReleaseBuilds/PaletteVariantGenerator_v1.0.1.zip` が生成され、`Assets/`、`Library/`、`Logs/`、`Temp/`、`Validation/`、`ReleaseBuilds/` が ZIP に含まれないことを確認する。

## GitHub Actions

1. `Release Package` workflow が成功していることを確認する。
2. workflow artifact の ZIP をダウンロードし、Unity プロジェクトへ追加できることを確認する。
3. 必要に応じて GitHub Release に ZIP を添付する。

## 手動 QA

- `Tools > Palette Variant Generator > 開く` からウィンドウを開ける。
- サンプル PNG を Analyze / Auto Group / Preview / Export できる。
- Export All と Folder Batch Export が元画像を上書きしない。
- Preset Asset の作成、更新、読み込みができる。
