// Avishai Dernis 2026

namespace Zarem.Assembler.Models.Meta;

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

    /// <summary>
    /// Gets an identifier for the instruction, including the argument pattern.
    /// </summary>
    /// <remarks>
    /// Introduces a variable argument count. Currently this appears to be an adequate approach.
    /// </remarks>
    string Identifier { get; }
}
