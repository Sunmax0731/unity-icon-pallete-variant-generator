# UI Toolkit 移行メモ

## 現在の判断

現行の IMGUI 実装は、Analyze、Auto Group、Preview、Palette、Variations、Replacement Rules、Batch Export、Preset Asset まで主要ワークフローを扱えている。

そのため、UI Toolkit への全面移行は一度に置き換えず、以下の順で進める。

1. IMGUI のまま狭いドッキング幅で破綻しにくい compact layout を入れる。
2. UI Toolkit 版の VisualElement 構成を別 branch または別 window で試作する。
3. ObjectField、ColorField、PopupField、ScrollView、Preview 表示、drag pan、split compare の同等操作を確認する。
4. 同等操作と検証観点が揃ってから main window を置き換える。

## UI Toolkit プレビュー Window

UI Toolkit preview window は本番置換前の評価用としてコード上に残しているが、通常メニューには表示しない。

- 本番作業は引き続き IMGUI 版の `Tools > Palette Variant Generator > 開く` を使う。
- preview window はソース情報、解析設定、グループ設定、出力設定、プリセット、プレビュー、パレット、バリエーション、置換ルール、色別ルールの主要セクション構成を UI Toolkit で検証する。
- `ISSUE23_UI_TOOLKIT_PREVIEW_VALIDATION=PASS` で、window の生成、タイトル、最小サイズ、主要セクションの存在を headless validation する。
- `ISSUE23_UI_TOOLKIT_INTERACTION_VALIDATION=PASS` で、ObjectField、ColorField、PopupField、ScrollView、Slider、プレビューの pan / wheel zoom / split compare の入力受け口を headless validation する。
- `ISSUE23_MAIN_WINDOW_UI_TOOLKIT_HOST_VALIDATION=PASS` で、本番 main window が UI Toolkit root と `IMGUIContainer` のホスト構成で開くことを headless validation する。

## main window の移行方針

本番 window は UI Toolkit root で `IMGUIContainer` をホストする hybrid 構成に移行した。

- Analyze、Auto Group、Preview、Export、Session、Preset、Batch Export などの既存 IMGUI ワークフローは維持する。
- Window の管理、root、将来の置き換え単位は UI Toolkit 側に寄せる。
- 純 UI Toolkit 化は preview window で標準コントロールと入力受け口を検証しながら、セクション単位で置き換える。

## compact layout

`PaletteVariantGeneratorWindow` は、幅が狭い場合に 3 pane 横並びから縦積みへ切り替える。

- wide: 左 settings、中央 preview / palette、右 variations / rules
- compact: settings、preview / palette、variations / rules を上から順に表示
- compact toolbar: 操作ボタンを複数行に分け、横スクロールを避ける
- minimum window size: docked layout を想定して `760 x 540`

## UI Toolkit 移行時に維持する操作

- `Tools > Palette Variant Generator` 以下のメニュー導線
- Source Image の選択と Analyze
- Auto Group と近傍色しきい値
- Preview、Auto Preview、Zoom、Pan、Split compare
- Palette のスクロールと手動 group 移動
- Variations の追加、複製、削除、Export 対象切り替え
- Group / PerColor / Hybrid の置換ルール
- Export、Export All、Folder Batch Export
- JSON Session、JSON Preset、ScriptableObject Preset Asset
- Help、Language、Version、License

## 置き換え前の確認

- compact 幅でも横スクロールが出にくいこと
- UI Toolkit の ScrollView 内で preview drag pan が入力を奪われないこと
- ColorField と ObjectField の Undo / dirty state が想定どおりであること
- IMGUI 版と同等の headless validation marker を維持すること
- 手動 QA で wide / compact / docked の 3 状態を確認すること
