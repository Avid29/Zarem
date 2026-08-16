// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models.Logic;

/// <summary>
/// An <see cref="IMultAddLogic{T, TL}"/> for a signed multiply and add operation on 32-bit values.
/// </summary>
public struct MultAddLogic<T, TL> : IMultAddLogic<T, TL>
    where T : unmanaged, IBinaryInteger<T>
    where TL : unmanaged, IBinaryInteger<TL>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TL Compute(T rs, T rt, TL @base) => @base + MultLogic<T, TL>.Compute(rs, rt);
}

/// <summary>
/// An <see cref="IMultAddLogic{T, TL}"/> for a signed multiply and subtract operation on 32-bit values.
/// </summary>
public struct MultSubLogic<T, TL> : IMultAddLogic<T, TL>
    where T : unmanaged, IBinaryInteger<T>
    where TL : unmanaged, IBinaryInteger<TL>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TL Compute(T rs, T rt, TL @base) => @base - MultLogic<T, TL>.Compute(rs, rt);
}
