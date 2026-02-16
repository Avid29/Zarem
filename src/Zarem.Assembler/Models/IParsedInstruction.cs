// Avishai Dernis 2026

using Zarem.Models.Tables;

namespace Zarem.Assembler.Models;

/// <summary>
/// An interface for handling a parsed instruction.
/// </summary>
public interface IParsedInstruction
{
    /// <summary>
    /// Gets the byte sequence that makes up the instruction.
    /// </summary>
    byte[] RealizeBytes();

    /// <summary>
    /// Gets the reference info, if the instruction made a reference.
    /// </summary>
    RelocationEntry? Reference { get; }
}
