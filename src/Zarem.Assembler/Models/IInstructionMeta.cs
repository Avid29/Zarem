// Avishai Dernis 2026

namespace Zarem.Assembler.Models;

/// <summary>
/// An interface for the metadata of an instruction.
/// </summary>
public interface IInstructionMeta
{
    /// <summary>
    /// Gets the name of the instruction.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the number of arguments the instruction takes.
    /// </summary>
    int ArgumentCount { get; }
}
