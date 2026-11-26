// Avishai Dernis 2026

using System.Numerics;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Interpret;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Machine.CoProcessors;

/// <summary>
/// An interface for a MIPS co-processor.
/// </summary>
public interface IMipsCoProcessor<T> : ICoProcessor<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Gets the executor.
    /// </summary>
    ICoExecutor<MipsInstruction, MipsExecution<T>, MipsGpRegister, MipsTrap> Executor { get; }
}
