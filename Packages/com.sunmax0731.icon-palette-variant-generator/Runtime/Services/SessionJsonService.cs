using System.Collections.Generic;
using System.IO;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Saves and loads palette variant sessions as JSON.
    /// </summary>
    public sealed class SessionJsonService
    {
        public const string CurrentSchemaVersion = "1.0.0";

        public void Save(string path, PaletteVariantSession session)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new System.ArgumentException("Session path is required.", nameof(path));
            }

            if (session == null)
            {
                throw new System.ArgumentNullException(nameof(session));
            }

            session.schemaVersion = string.IsNullOrWhiteSpace(session.schemaVersion)
                ? CurrentSchemaVersion
                : session.schemaVersion;

            string folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, JsonUtility.ToJson(session, true));
        }

        public SessionLoadResult Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return new SessionLoadResult(null, new[] { "Session JSON was not found." });
            }

            string json = File.ReadAllText(path);
            PaletteVariantSession session = JsonUtility.FromJson<PaletteVariantSession>(json);
            List<string> warnings = Validate(session);
            return new SessionLoadResult(session, warnings);
        }

        private static List<string> Validate(PaletteVariantSession session)
        {
            List<string> warnings = new List<string>();
            if (session == null)
            {
                warnings.Add("Session JSON could not be parsed.");
                return warnings;
            }

            if (session.schemaVersion != CurrentSchemaVersion)
            {
                warnings.Add($"Session schemaVersion is {session.schemaVersion}; expected {CurrentSchemaVersion}.");
            }

            if (string.IsNullOrWhiteSpace(session.sourceImageAssetPath))
            {
                warnings.Add("Session has no source image path.");
            }
            else if (!File.Exists(session.sourceImageAssetPath))
            {
                warnings.Add($"Source image was not found: {session.sourceImageAssetPath}");
            }

            session.analyzeSettings ??= new AnalyzeSettings();
            session.groupSettings ??= new GroupSettings();
            session.exportSettings ??= new ExportSettings();
            session.paletteColors ??= new List<PaletteColorEntry>();
            session.colorGroups ??= new List<ColorGroup>();
            session.colorRules ??= new List<ColorReplacementRule>();

            return warnings;
        }
    }
}
