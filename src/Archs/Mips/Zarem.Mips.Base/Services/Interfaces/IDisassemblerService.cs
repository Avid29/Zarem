// Avishai Dernis 2025

using Zarem.Models.Instructions;

namespace Zarem.Services.Interfaces;

#if DEBUG

/// <summary>
/// An interface for a disassembly service.
/// </summary>
public interface IDisassemblerService
{
    /// <summary>
    /// Disassembles an instruction.
    /// </summary>
    /// <param name="instruction">The instruction to disassemble.</param>
    /// <returns>The encoded instruction as assembly code.</returns>
    public string Disassemble(MipsInstruction instruction);
}

#endif
