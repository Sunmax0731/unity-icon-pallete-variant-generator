# 05 - Release Packaging Skill

## 目的

リリースに向けたドキュメント、パッケージ、GitHub Release、BOOTH 紹介文を整えるための工程ガイドです。

## 開始条件

- 対象 Issue の実装と手動確認が完了している。
- open Issue にリリース blocker が残っていない。
- `CHANGELOG.md` とリリースノートが更新済みである。

## 更新対象

- `README.md`
- `Packages/com.sunmax0731.icon-palette-variant-generator/README.md`
- `CHANGELOG.md`
- `docs/manual.md`
- `docs/release-notes-v1.1.0.md`
- `docs/validation-checklist.md`
- `docs/release-checklist.md`
- `docs/booth-copy.md`
- `Agents.md`
- `Skill.md`

## リリース検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
powershell -ExecutionPolicy Bypass -File tools\release\build-release.ps1 -Version 1.1.0
powershell -ExecutionPolicy Bypass -File tools\release\test-release-package.ps1 -Version 1.1.0
```

生成物:

```text
ReleaseBuilds/PaletteVariantGenerator_v1.1.0.zip
ReleaseBuilds/PaletteVariantGenerator_v1.1.0.unitypackage
```

GitHub Release には ZIP と `.unitypackage` の両方を添付する。

## BOOTH Copy

`docs/booth-copy.md` を商品紹介編集ページ用 Markdown とする。BOOTH Product Page Autofill の入力仕様に合わせ、次の見出しを使う。

- `## 商品名`
- `## 概要`
- `## 詳細`
- `## 内容物`
- `## 対応環境`
- `## アップデート履歴`
- `## 注意事項`

補助情報として `## 根拠`、`## タグ案`、`## 商品画像構成案`、`## BOOTH入力設定` を置いてよい。これらは autofill 側で入力対象から除外される想定です。

## 完了条件

- 自動検証が通る。
- ZIP と `.unitypackage` を生成し、検証できる。
- Issue に検証結果をコメントする。
- commit / push が完了する。
- Release 公開時は `gh release view v1.1.0 --json assets` で両方の asset を確認する。
