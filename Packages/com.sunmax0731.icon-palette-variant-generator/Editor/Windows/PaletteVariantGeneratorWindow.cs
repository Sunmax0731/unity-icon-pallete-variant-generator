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
        private const float LeftPaneWidth = 300f;
        private const float RightPaneWidth = 330f;
        private const float PaneGap = 14f;
        private const float MinPreviewHeight = 260f;
        private static readonly Color SeparatorColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);

        private readonly PaletteVariantSession session = new PaletteVariantSession();
        private readonly TextureAssetLoader textureAssetLoader = new TextureAssetLoader();
        private readonly ColorExtractionService colorExtractionService = new ColorExtractionService();
        private readonly ColorGroupingService colorGroupingService = new ColorGroupingService();
        private readonly ColorReplacementService colorReplacementService = new ColorReplacementService();
        private Texture2D sourceImage;
        private Texture2D readableSourceImage;
        private Texture2D afterPreview;
        private Texture2D checkerboardTexture;
        private Vector2 leftScroll;
        private Vector2 rightScroll;
        private string sourceAssetPath = string.Empty;
        private string reportMessage = "Select a project PNG or Texture2D asset, then click Analyze.";
        private MessageType reportType = MessageType.Info;

        [MenuItem("Tools/Icon Tools/Palette Variant Generator")]
        public static void Open()
        {
            PaletteVariantGeneratorWindow window = GetWindow<PaletteVariantGeneratorWindow>();
            window.titleContent = new GUIContent("Palette Variant Generator");
            window.minSize = new Vector2(1080f, 620f);
            window.Show();
        }

        private void OnEnable()
        {
            checkerboardTexture = CreateCheckerboardTexture();
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.HelpBox(reportMessage, reportType);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPane();
                DrawPaneSeparator();
                GUILayout.Space(PaneGap);
                DrawCenterPane();
                GUILayout.Space(PaneGap);
                DrawPaneSeparator();
                DrawRightPane();
            }
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
                    GUILayout.Button("Export", EditorStyles.toolbarButton);
                    GUILayout.Button("Save Session", EditorStyles.toolbarButton);
                    GUILayout.Button("Load Session", EditorStyles.toolbarButton);
                }

                using (new EditorGUI.DisabledScope(readableSourceImage == null || session.colorGroups.Count == 0))
                {
                    if (GUILayout.Button("Preview", EditorStyles.toolbarButton, GUILayout.Width(78f)))
                    {
                        RefreshAfterPreview();
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(sourceAssetPath, EditorStyles.miniLabel, GUILayout.MinWidth(160f));
            }
        }

        private void DrawLeftPane()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(LeftPaneWidth)))
            {
                leftScroll = EditorGUILayout.BeginScrollView(leftScroll);
                DrawSectionHeader("Analyze Settings");
                session.analyzeSettings.alphaThreshold = EditorGUILayout.IntSlider("Alpha Threshold", session.analyzeSettings.alphaThreshold, 0, 255);
                session.analyzeSettings.minimumPixelCount = Mathf.Max(1, EditorGUILayout.IntField("Minimum Pixel Count", session.analyzeSettings.minimumPixelCount));
                session.analyzeSettings.quantizeStep = EditorGUILayout.IntSlider("Quantize Step", session.analyzeSettings.quantizeStep, 1, 64);
                session.analyzeSettings.maxPaletteColors = Mathf.Max(1, EditorGUILayout.IntField("Max Palette Colors", session.analyzeSettings.maxPaletteColors));

                DrawSectionSeparator();
                DrawSectionHeader("Group Settings");
                session.groupSettings.targetGroupCount = EditorGUILayout.IntSlider("Target Group Count", session.groupSettings.targetGroupCount, 1, 64);
                session.groupSettings.distanceMode = (ColorDistanceMode)EditorGUILayout.EnumPopup("Distance Mode", session.groupSettings.distanceMode);
                session.groupSettings.preserveDarkOutline = EditorGUILayout.Toggle("Preserve Dark Outline", session.groupSettings.preserveDarkOutline);
                session.groupSettings.preserveAlpha = EditorGUILayout.Toggle("Preserve Alpha", session.groupSettings.preserveAlpha);

                DrawSectionSeparator();
                DrawSectionHeader("Source Info");
                if (readableSourceImage != null)
                {
                    EditorGUILayout.LabelField("Asset Path", sourceAssetPath);
                    EditorGUILayout.LabelField("Size", $"{readableSourceImage.width} x {readableSourceImage.height}");
                    EditorGUILayout.LabelField("Palette Colors", session.paletteColors.Count.ToString());
                    EditorGUILayout.LabelField("Groups", session.colorGroups.Count.ToString());
                }
                else
                {
                    EditorGUILayout.HelpBox("No source image has been analyzed yet.", MessageType.None);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawCenterPane()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(360f), GUILayout.ExpandWidth(true)))
            {
                DrawSectionHeader("Preview");
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawPreviewPanel("Before", readableSourceImage);
                    DrawPreviewPanel("After", afterPreview);
                }

                DrawSectionSeparator();
                DrawSectionHeader("Palette");
                if (session.paletteColors.Count == 0)
                {
                    EditorGUILayout.HelpBox("Palette colors will appear here after analysis.", MessageType.None);
                    return;
                }

                DrawPaletteList(session.paletteColors);
            }
        }

        private void DrawRightPane()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(RightPaneWidth)))
            {
                rightScroll = EditorGUILayout.BeginScrollView(rightScroll);
                DrawSectionHeader("Replacement Rules");
                if (session.colorGroups.Count == 0)
                {
                    EditorGUILayout.HelpBox("Color groups will appear here after Auto Group.", MessageType.None);
                    EditorGUILayout.EndScrollView();
                    return;
                }

                DrawGroupList(session.colorGroups);
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawPreviewPanel(string title, Texture2D texture)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                Rect previewRect = GUILayoutUtility.GetRect(10f, 10000f, MinPreviewHeight, MinPreviewHeight, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(previewRect, new Color(0.13f, 0.13f, 0.13f));
                DrawCheckerboard(previewRect);

                if (texture == null)
                {
                    DrawCenteredLabel(previewRect, "No preview");
                    return;
                }

                Rect imageRect = FitRect(previewRect, texture.width, texture.height);
                GUI.DrawTexture(imageRect, texture, ScaleMode.StretchToFill, true);
            }
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
                    DrawGroupRule(group);
                }
            }
        }

        private void DrawGroupRule(ColorGroup group)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    Rect representativeRect = GUILayoutUtility.GetRect(20f, 18f, GUILayout.Width(20f));
                    EditorGUI.DrawRect(representativeRect, group.representativeColor);
                    EditorGUILayout.LabelField(group.displayName, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"{group.colorEntryIds.Count} colors", GUILayout.Width(76f));
                    EditorGUILayout.LabelField($"{group.pixelRatio:P1}", GUILayout.Width(56f));
                }

                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    group.targetColor = EditorGUILayout.ColorField("Target Color", group.targetColor);
                    group.blendRatio = EditorGUILayout.Slider("Blend Ratio", group.blendRatio, 0f, 1f);
                    group.replacementMode = (ColorReplacementMode)EditorGUILayout.EnumPopup("Mode", group.replacementMode);
                    if (change.changed && afterPreview != null)
                    {
                        RefreshAfterPreview();
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
            DestroyAfterPreview();
            reportMessage = $"Analyzed {session.paletteColors.Count} palette colors from {assetPath}.";
            reportType = MessageType.Info;
        }

        private void AutoGroupPalette()
        {
            session.colorGroups = new List<ColorGroup>(colorGroupingService.CreateGroups(session.paletteColors, session.groupSettings));
            reportMessage = $"Created {session.colorGroups.Count} color groups.";
            reportType = session.colorGroups.Count == 0 ? MessageType.Warning : MessageType.Info;
            RefreshAfterPreview();
        }

        private void RefreshAfterPreview()
        {
            if (readableSourceImage == null)
            {
                reportMessage = "Analyze a source image before preview.";
                reportType = MessageType.Warning;
                return;
            }

            if (session.colorGroups.Count == 0)
            {
                reportMessage = "Create color groups before preview.";
                reportType = MessageType.Warning;
                return;
            }

            DestroyAfterPreview();
            afterPreview = colorReplacementService.Apply(readableSourceImage, session);
            reportMessage = "After preview updated.";
            reportType = MessageType.Info;
        }

        private void OnDisable()
        {
            DestroyReadableSourceImage();
            DestroyAfterPreview();
            if (checkerboardTexture != null)
            {
                DestroyImmediate(checkerboardTexture);
                checkerboardTexture = null;
            }
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

        private void DestroyAfterPreview()
        {
            if (afterPreview == null)
            {
                return;
            }

            DestroyImmediate(afterPreview);
            afterPreview = null;
        }

        private void DrawSectionHeader(string title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        private void DrawSectionSeparator()
        {
            EditorGUILayout.Space(6f);
            Rect rect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, SeparatorColor);
            EditorGUILayout.Space(6f);
        }

        private void DrawPaneSeparator()
        {
            Rect rect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.Width(1f), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, SeparatorColor);
        }

        private void DrawCheckerboard(Rect rect)
        {
            if (checkerboardTexture == null)
            {
                return;
            }

            GUI.DrawTextureWithTexCoords(rect, checkerboardTexture, new Rect(0f, 0f, rect.width / 16f, rect.height / 16f));
        }

        private static Rect FitRect(Rect container, float width, float height)
        {
            float scale = Mathf.Min(container.width / width, container.height / height);
            float fittedWidth = width * scale;
            float fittedHeight = height * scale;
            return new Rect(
                container.x + ((container.width - fittedWidth) * 0.5f),
                container.y + ((container.height - fittedHeight) * 0.5f),
                fittedWidth,
                fittedHeight);
        }

        private static void DrawCenteredLabel(Rect rect, string text)
        {
            GUIStyle style = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(rect, text, style);
        }

        private static Texture2D CreateCheckerboardTexture()
        {
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point
            };

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    bool light = ((x / 4) + (y / 4)) % 2 == 0;
                    texture.SetPixel(x, y, light ? new Color(0.72f, 0.72f, 0.72f) : new Color(0.48f, 0.48f, 0.48f));
                }
            }

            texture.Apply();
            return texture;
        }
    }
}
