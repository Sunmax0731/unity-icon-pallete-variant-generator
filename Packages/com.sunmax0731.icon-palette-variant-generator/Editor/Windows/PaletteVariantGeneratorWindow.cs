using Sunmax0731.IconPaletteVariantGenerator.Models;
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
        private Texture2D sourceImage;
        private Vector2 scrollPosition;

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

                using (new EditorGUI.DisabledScope(true))
                {
                    GUILayout.Button("Analyze", EditorStyles.toolbarButton);
                    GUILayout.Button("Auto Group", EditorStyles.toolbarButton);
                    GUILayout.Button("Preview", EditorStyles.toolbarButton);
                    GUILayout.Button("Export", EditorStyles.toolbarButton);
                    GUILayout.Button("Save Session", EditorStyles.toolbarButton);
                    GUILayout.Button("Load Session", EditorStyles.toolbarButton);
                }
            }
        }

        private void DrawCurrentScaffoldState()
        {
            EditorGUILayout.LabelField("Phase 0 Scaffold", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "The package scaffold is active. Image analysis, grouping, preview, export, and session IO are intentionally disabled until their implementation issues are completed.",
                MessageType.Info);
        }

        private void DrawSessionBaseline()
        {
            EditorGUILayout.LabelField("Default Session Settings", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Schema Version", session.schemaVersion);
                EditorGUILayout.IntField("Alpha Threshold", session.analyzeSettings.alphaThreshold);
                EditorGUILayout.IntField("Quantize Step", session.analyzeSettings.quantizeStep);
                EditorGUILayout.IntField("Target Group Count", session.groupSettings.targetGroupCount);
                EditorGUILayout.EnumPopup("Distance Mode", session.groupSettings.distanceMode);
                EditorGUILayout.TextField("Output Folder", session.exportSettings.outputFolder);
                EditorGUILayout.EnumPopup("Conflict Mode", session.exportSettings.conflictMode);
            }
        }
    }
}
