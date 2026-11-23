// Avishai Dernis 2026

using System.Numerics;
using Zarem.Mips.Emulator.Machine.CoProcessors;

namespace Zarem.Mips.Emulator.Interpret.CoProcessors.FloatCoProcessor;

/// <summary>
/// A <see cref="FloatCoProcessor{T, TFloat}{T}"/> that executes by interpreting each instruction.
/// </summary>
public interface IInterpretedFloatCoProcessor<T> : IFloatCoProcessor, IInterpretedCoProcessor<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
}
