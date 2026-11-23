// Avishai Dernis 2026

using System.Reflection.Emit;
using Zarem.Mips.Emulator.Machine.CoProcessors;
using Zarem.Mips.Models.Instructions;

namespace Zarem.Mips.Emulator.JIT.CoProcessors;

/// <summary>
/// An interface for a JIT co-processor.
/// </summary>
public interface IJitCoProcessor<T> : ICoProcessor
{
    /// <summary>
    /// Compiles an instruction 
    /// </summary>
    /// <param name="il">The <see cref="ILGenerator"/> to write to.</param>
    /// <param name="instruction">The instruction to cross compile.</param>
    /// <param name="pc">The address the instruction is at.</param>
    void CompileInstruction(ILGenerator il, MipsInstruction instruction, T pc);
}
