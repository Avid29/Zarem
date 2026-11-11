// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models.Logic;

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for an bit clear operation.
/// </summary>
public struct BClr<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt)
    {
        var shift = int.CreateTruncating(rt);
        var mask = ~(T.One << shift);
        return rs & mask;
    }
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for an bit set operation.
/// </summary>
public struct BSet<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt)
    {
        var shift = int.CreateTruncating(rt);
        var mask = T.One << shift;
        return rs | mask;
    }
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for an bit invert operation.
/// </summary>
public struct BInv<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt)
    {
        var shift = int.CreateTruncating(rt);
        var mask = T.One << shift;
        return rs ^ mask;
    }
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for an bit extract operation.
/// </summary>
public struct BExt<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt)
    {
        var shift = int.CreateTruncating(rt);
        return (rs >> shift) & T.One;
    }
}
