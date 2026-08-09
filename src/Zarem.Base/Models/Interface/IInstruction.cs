// Avishai Dernis 2026

namespace Zarem.Models.Interface;

/// <summary>
/// An interface for a binary instruction.
/// </summary>
public interface IInstruction
{
    /// <summary>
    /// Gets the length of the instruction in bytes.
    /// </summary>
    public int Length { get; }
}
