// Avishai Dernis 2026

using Zarem.Assembler.Logging.Interfaces;
using Zarem.Models.Tables;

namespace Zarem.Linker.Handlers;

/// <summary>
/// An interface for an architecture-specific linker handler.
/// </summary>
public interface ILinkerHandler
{
    /// <summary>
    /// Patches a relocation into a section.
    /// </summary>
    /// <param name="section">The section to patch.</param>
    /// <param name="relocation">The relocation to apply.</param>
    /// <param name="symbolAddress">The address of the symbol within the referenced.</param>
    /// <param name="place">The address to apply the relocation.</param>
    /// <param name="logger">The .</param>
    /// <returns><see langword="true"/> if the relocation succeeds, <see langword="false"/> otherwise.</returns>
    bool PatchRelocation(Section section, RelocationEntry relocation, ulong symbolAddress, ulong place, ILogger? logger = null);
}
