using System.Collections.Generic;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Result from loading a saved palette variant session.
    /// </summary>
    public sealed class SessionLoadResult
    {
        public SessionLoadResult(PaletteVariantSession session, IReadOnlyList<string> warnings)
        {
            Session = session;
            Warnings = warnings;
        }

        public PaletteVariantSession Session { get; }
        public IReadOnlyList<string> Warnings { get; }
        public bool Success => Session != null;
    }
}
