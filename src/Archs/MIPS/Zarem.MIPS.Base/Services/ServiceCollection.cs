// Avishai Dernis 2025

using Zarem.Services.Interfaces;

namespace Zarem.Services;

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
