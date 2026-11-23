// Avishai Dernis 2026

using System.Numerics;
using Zarem.Mips.Emulator.Machine.CoProcessors;

namespace Zarem.Mips.Emulator.JIT.CoProcessors.FloatCoProcessor;

/// <summary>
/// An <see cref="JitFloatCoProcessor{T, TFloat}"/> that executes by interpreting each instruction.
/// </summary>
public interface IJitFloatCoProcessor<T> : IFloatCoProcessor, IJitCoProcessor<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
}
