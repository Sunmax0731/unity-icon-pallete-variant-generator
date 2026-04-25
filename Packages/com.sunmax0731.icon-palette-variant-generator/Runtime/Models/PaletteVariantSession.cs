using System;
using System.Collections.Generic;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Root model for the current palette variant generation session.
    /// </summary>
    [Serializable]
    public sealed class PaletteVariantSession
    {
        public string schemaVersion = "1.0.0";
        public string sourceImageAssetPath = string.Empty;
        public AnalyzeSettings analyzeSettings = new AnalyzeSettings();
        public GroupSettings groupSettings = new GroupSettings();
        public ExportSettings exportSettings = new ExportSettings();
        public List<PaletteColorEntry> paletteColors = new List<PaletteColorEntry>();
        public List<ColorGroup> colorGroups = new List<ColorGroup>();
        public string activeVariationId = string.Empty;
    }
}
