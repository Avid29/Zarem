// Avishai Dernis 2026

namespace Zarem.Assembler.Models;

/// <summary>
/// An interface for pseudo instruction meta definitions.
/// </summary>
public interface IPseudoInstructionMeta
{
    /// <summary>
    /// Gets the expansion of the pseudo-instruction into real instructions.
    /// </summary>
    string[][] Expansion { get; init; }
}
