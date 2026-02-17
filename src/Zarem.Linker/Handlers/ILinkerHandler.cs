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
    /// Gets the name of the architecture.
    /// </summary>
    string GetArchitectureName();

    /// <summary>
    /// Patches a relocation into a section.
    /// </summary>
    /// <param name="section">The section to patch.</param>
    /// <param name="relocation">The relocation to apply.</param>
    /// <param name="offset">The location of the patch within the section's stream.</param>
    /// <param name="symbolVirtual">The virtual address of the referenced symbol.</param>
    /// <param name="patchVirtual">The virtual address of the patch.</param>
    /// <param name="logger">The logger to track failures.</param>
    /// <returns><see langword="true"/> if the relocation succeeds, <see langword="false"/> otherwise.</returns>
    bool PatchRelocation(Section section, RelocationEntry relocation, ulong offset, ulong symbolVirtual, ulong patchVirtual, ILogger? logger = null);
}
