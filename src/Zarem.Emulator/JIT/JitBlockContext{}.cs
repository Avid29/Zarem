// Avishai Dernis 2026

using System.Numerics;
using System.Reflection.Emit;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A class for managing the results of the compiler scan stage.
/// </summary>
public  class JitBlockContext<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JitBlockContext{T, TRegister, TTrap}"/> class.
    /// </summary>
    public JitBlockContext(ILGenerator il, T start, T end)
    {
        ILGenerator = il;
        Start = start;
        End = end;
    }

    /// <summary>
    /// Gets the IL Generator for the block.
    /// </summary>
    public ILGenerator ILGenerator { get; }

    /// <summary>
    /// Gets the start address for the block.
    /// </summary>
    public T Start { get; }

    /// <summary>
    /// Gets the end address for the block.
    /// </summary>
    public T End { get; }
}
