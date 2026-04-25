namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Result details for a PNG export request.
    /// </summary>
    public sealed class PngExportResult
    {
        public PngExportResult(PngExportStatus status, string outputPath, string message)
        {
            Status = status;
            OutputPath = outputPath;
            Message = message;
        }

        public PngExportStatus Status { get; }
        public string OutputPath { get; }
        public string Message { get; }
    }
}
