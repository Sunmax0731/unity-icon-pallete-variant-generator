# Export Alpha Transparency Manual Tests

## Purpose

JPEG source images are edited internally as RGBA PNG-compatible buffers. Exported PNG assets must keep eraser-created alpha and must be imported by Unity with `Alpha Is Transparency` enabled automatically.

## Common Setup

1. Open Unity `6000.4.0f1`.
2. Open `Tools > Palette Variant Generator > メイン画面`.
3. Select a JPEG source image, then run `Analyze`, `Auto Group`, and `Preview`.
4. Set `Preview Mode` to `描画`.
5. Set `編集対象` to `読み込み画像`.
6. Set `ツール` to `消しゴム`.
7. Drag on the Preview until part of the image becomes transparent.
8. Keep the output folder under `Assets/`, for example `Assets/GeneratedIcons`.

## TC-EXP-ALPHA-01 Export

1. Click `Export`.
2. Select the exported PNG in the Unity Project window.
3. In Inspector, open the texture import settings.

Expected result:

- The PNG visually matches the Preview, including transparent erased areas.
- `Alpha Is Transparency` is ON automatically.
- The original JPEG asset is not overwritten.

## TC-EXP-ALPHA-02 Export All

1. Create at least two variations.
2. Keep both variations enabled for export.
3. Click `Export All`.
4. Select each exported PNG in the Unity Project window.

Expected result:

- Every exported PNG keeps the transparent erased areas.
- `Alpha Is Transparency` is ON automatically for every exported PNG.

## TC-EXP-ALPHA-03 Folder Batch Export

1. Set a valid batch source folder.
2. Keep the output folder under `Assets/`.
3. Run folder batch export from the export settings window.
4. Select the exported PNG files in the Unity Project window.

Expected result:

- Batch exported PNG files are generated without overwriting source images.
- `Alpha Is Transparency` is ON automatically for every exported PNG that is under `Assets/`.

## Note

Unity can only apply `Alpha Is Transparency` to files imported as project assets. If the output folder is outside `Assets/`, the PNG still preserves alpha in the file, but Unity has no TextureImporter setting to update automatically.
