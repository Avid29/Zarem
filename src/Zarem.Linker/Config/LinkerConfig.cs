// Avishai Dernis 2026

using Zarem.Linker.Enums;

namespace Zarem.Linker.Config;

/// <summary>
/// A base class for a linker configuration.
/// </summary>
public class LinkerConfig
{
    /// <summary>
    /// Gets or sets the link mode.
    /// </summary>
    public LinkMode LinkMode { get; set; }

    /// <summary>
    /// Gets or sets the base virtual address of sections in the output module.
    /// </summary>
    public ulong BaseAddress { get; set; }
}
