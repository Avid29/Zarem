// Avishai Dernis 2026

using System.Numerics;
using System.Reflection.Emit;
using Zarem.Mips.Emulator.Interpret;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;

namespace Zarem.Mips.Emulator.Machine.CoProcessors;

/// <summary>
/// An interface for a MIPS co-processor.
/// </summary>
public interface ICoProcessor<T> : ICoProcessor
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Executes a <see cref="MipsInstruction"/>.
    /// </summary>
    /// <param name="instruction">The instruction to execute.</param>
    /// <param name="execution">The execution result.</param>
    /// <returns>The resulting trap if any.</returns>
    MipsTrap Execute(MipsInstruction instruction, out MipsExecution<T> execution);

    /// <summary>
    /// Compiles an instruction 
    /// </summary>
    /// <param name="il">The <see cref="ILGenerator"/> to write to.</param>
    /// <param name="instruction">The instruction to cross compile.</param>
    /// <param name="pc">The address the instruction is at.</param>
    void CompileInstruction(ILGenerator il, MipsInstruction instruction, T pc);
}
