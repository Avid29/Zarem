// Avishai Dernis 2025

using Zarem.Mips.Services.Interfaces;

namespace Zarem.Mips.Services;

/// <summary>
/// A collection of global services.
/// </summary>
public static class ServiceCollection
{
    #if DEBUG

    /// <summary>
    /// Gets the <see cref="DisassemblerService"/>.
    /// </summary>
    public static IDisassemblerService? DisassemblerService { get; set; }

    #endif
}
