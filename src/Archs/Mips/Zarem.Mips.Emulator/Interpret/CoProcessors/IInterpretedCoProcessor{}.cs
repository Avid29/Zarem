// Avishai Dernis 2026

using System.Numerics;
using Zarem.Mips.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;

namespace Zarem.Mips.Emulator.Interpret.CoProcessors;

/// <summary>
/// An interface for an interpreted co-processor.
/// </summary>
public interface IInterpretedCoProcessor<T> : ICoProcessor
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Executes a <see cref="MipsInstruction"/>.
    /// </summary>
    /// <param name="instruction">The instruction to execute.</param>
    /// <param name="execution">The execution result.</param>
    /// <returns>The resulting trap if any.</returns>
    MipsTrap Execute(MipsInstruction instruction, out MipsExecution<T> execution);
}
