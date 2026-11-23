// Avishai Dernis 2026

using System.Numerics;

namespace Zarem.Mips.Emulator.Machine.CoProcessors;

/// <summary>
/// An interface for a <see cref="FloatCoProcessor{T, TFloat}"/> without a concrete type.
/// </summary>
public interface IFloatCoProcessor<T> : ICoProcessor<T>, IFloatCoProcessor
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
}
