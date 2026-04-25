using System.Collections.Generic;
using Sunmax0731.IconPaletteVariantGenerator.Editor.Services;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Windows
{
    /// <summary>
    /// Entry EditorWindow for the Icon Palette Variant Generator.
    /// </summary>
    public sealed class PaletteVariantGeneratorWindow : EditorWindow
    {
        private readonly PaletteVariantSession session = new PaletteVariantSession();
        private readonly TextureAssetLoader textureAssetLoader = new TextureAssetLoader();
        private readonly ColorExtractionService colorExtractionService = new ColorExtractionService();
        private readonly ColorGroupingService colorGroupingService = new ColorGroupingService();
        private Texture2D sourceImage;
        private Texture2D readableSourceImage;
        private Vector2 scrollPosition;
        private string sourceAssetPath = string.Empty;
        private string reportMessage = "Select a project PNG or Texture2D asset, then click Analyze.";

        [MenuItem("Tools/Icon Tools/Palette Variant Generator")]
        public static void Open()
        {
            PaletteVariantGeneratorWindow window = GetWindow<PaletteVariantGeneratorWindow>();
            window.titleContent = new GUIContent("Palette Variant Generator");
            window.minSize = new Vector2(760f, 480f);
            window.Show();
        }

        private void OnGUI()
        {
            DrawToolbar();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.Space(8f);
            DrawCurrentScaffoldState();
            EditorGUILayout.Space(8f);
            DrawSessionBaseline();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                sourceImage = (Texture2D)EditorGUILayout.ObjectField(sourceImage, typeof(Texture2D), false, GUILayout.MinWidth(220f));

                using (new EditorGUI.DisabledScope(sourceImage == null))
                {
                    if (GUILayout.Button("Analyze", EditorStyles.toolbarButton))
                    {
                        AnalyzeSourceImage();
                    }
                }

                using (new EditorGUI.DisabledScope(session.paletteColors.Count == 0))
                {
                    if (GUILayout.Button("Auto Group", EditorStyles.toolbarButton))
                    {
                        AutoGroupPalette();
                    }
                }

                using (new EditorGUI.DisabledScope(true))
                {
                    GUILayout.Button("Preview", EditorStyles.toolbarButton);
                    GUILayout.Button("Export", EditorStyles.toolbarButton);
                    GUILayout.Button("Save Session", EditorStyles.toolbarButton);
                    GUILayout.Button("Load Session", EditorStyles.toolbarButton);
                }
            }
        }

        private void DrawCurrentScaffoldState()
        {
            EditorGUILayout.LabelField("Source Image", EditorStyles.boldLabel);
            if (readableSourceImage != null)
            {
                Rect previewRect = GUILayoutUtility.GetRect(128f, 128f, GUILayout.ExpandWidth(false));
                EditorGUI.DrawPreviewTexture(previewRect, readableSourceImage, null, ScaleMode.ScaleToFit);
                EditorGUILayout.LabelField("Asset Path", sourceAssetPath);
                EditorGUILayout.LabelField("Size", $"{readableSourceImage.width} x {readableSourceImage.height}");
                EditorGUILayout.LabelField("Palette Colors", session.paletteColors.Count.ToString());
            }
            else
            {
                EditorGUILayout.HelpBox("No source image has been analyzed yet.", MessageType.None);
            }

            EditorGUILayout.HelpBox(
                reportMessage,
                string.IsNullOrEmpty(reportMessage) ? MessageType.None : MessageType.Info);
        }

        private void DrawSessionBaseline()
        {
            EditorGUILayout.LabelField("Analyze Settings", EditorStyles.boldLabel);

            session.analyzeSettings.alphaThreshold = EditorGUILayout.IntSlider("Alpha Threshold", session.analyzeSettings.alphaThreshold, 0, 255);
            session.analyzeSettings.minimumPixelCount = Mathf.Max(1, EditorGUILayout.IntField("Minimum Pixel Count", session.analyzeSettings.minimumPixelCount));
            session.analyzeSettings.quantizeStep = EditorGUILayout.IntSlider("Quantize Step", session.analyzeSettings.quantizeStep, 1, 64);
            session.analyzeSettings.maxPaletteColors = Mathf.Max(1, EditorGUILayout.IntField("Max Palette Colors", session.analyzeSettings.maxPaletteColors));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Group Settings", EditorStyles.boldLabel);
            session.groupSettings.targetGroupCount = EditorGUILayout.IntSlider("Target Group Count", session.groupSettings.targetGroupCount, 1, 64);
            session.groupSettings.distanceMode = (ColorDistanceMode)EditorGUILayout.EnumPopup("Distance Mode", session.groupSettings.distanceMode);
            session.groupSettings.preserveDarkOutline = EditorGUILayout.Toggle("Preserve Dark Outline", session.groupSettings.preserveDarkOutline);
            session.groupSettings.preserveAlpha = EditorGUILayout.Toggle("Preserve Alpha", session.groupSettings.preserveAlpha);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Palette", EditorStyles.boldLabel);

            if (session.paletteColors.Count == 0)
            {
                EditorGUILayout.HelpBox("Palette colors will appear here after analysis.", MessageType.None);
                return;
            }

            DrawPaletteList(session.paletteColors);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Groups", EditorStyles.boldLabel);
            if (session.colorGroups.Count == 0)
            {
                EditorGUILayout.HelpBox("Color groups will appear here after Auto Group.", MessageType.None);
                return;
            }

            DrawGroupList(session.colorGroups);
        }

        private void DrawPaletteList(IReadOnlyList<PaletteColorEntry> paletteColors)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                foreach (PaletteColorEntry entry in paletteColors)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        Rect swatchRect = GUILayoutUtility.GetRect(22f, 18f, GUILayout.Width(22f));
                        EditorGUI.DrawRect(swatchRect, entry.color);
                        EditorGUILayout.LabelField(entry.hex, GUILayout.Width(80f));
                        EditorGUILayout.LabelField($"{entry.color.r},{entry.color.g},{entry.color.b},{entry.color.a}", GUILayout.Width(120f));
                        EditorGUILayout.LabelField(entry.pixelCount.ToString(), GUILayout.Width(64f));
                        EditorGUILayout.LabelField($"{entry.pixelRatio:P1}", GUILayout.Width(64f));
                        EditorGUILayout.LabelField(entry.groupId, GUILayout.Width(80f));
                    }
                }
            }
        }

        private void DrawGroupList(IReadOnlyList<ColorGroup> colorGroups)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                foreach (ColorGroup group in colorGroups)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        Rect swatchRect = GUILayoutUtility.GetRect(22f, 18f, GUILayout.Width(22f));
                        EditorGUI.DrawRect(swatchRect, group.representativeColor);
                        EditorGUILayout.LabelField(group.displayName, GUILayout.Width(80f));
                        EditorGUILayout.LabelField(group.id, GUILayout.Width(80f));
                        EditorGUILayout.LabelField($"{group.colorEntryIds.Count} colors", GUILayout.Width(80f));
                        EditorGUILayout.LabelField($"{group.pixelRatio:P1}", GUILayout.Width(64f));
                    }
                }
            }
        }

        private void AnalyzeSourceImage()
        {
            if (!textureAssetLoader.TryLoadReadableTexture(sourceImage, out Texture2D loadedTexture, out string assetPath, out string error))
            {
                reportMessage = error;
                return;
            }

            DestroyReadableSourceImage();
            readableSourceImage = loadedTexture;
            sourceAssetPath = assetPath;
            session.sourceImageAssetPath = assetPath;
            session.paletteColors = new List<PaletteColorEntry>(colorExtractionService.Extract(readableSourceImage, session.analyzeSettings));
            session.colorGroups.Clear();
            reportMessage = $"Analyzed {session.paletteColors.Count} palette colors from {assetPath}.";
        }

        private void AutoGroupPalette()
        {
            session.colorGroups = new List<ColorGroup>(colorGroupingService.CreateGroups(session.paletteColors, session.groupSettings));
            reportMessage = $"Created {session.colorGroups.Count} color groups.";
        }

        private void OnDisable()
        {
            DestroyReadableSourceImage();
        }

        private void DestroyReadableSourceImage()
        {
            if (readableSourceImage == null)
            {
                return;
            }

            DestroyImmediate(readableSourceImage);
            readableSourceImage = null;
        }
    }
}
