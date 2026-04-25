using System;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Settings used when writing generated icon variants to the project.
    /// </summary>
    [Serializable]
    public sealed class ExportSettings
    {
        public string outputFolder = "Assets/GeneratedIcons";
        public string filePrefix = "icon";
        public string fileSuffix = "variant";
        public ExportConflictMode conflictMode = ExportConflictMode.Duplicate;
        public bool refreshAssetDatabase = true;
    }
}
