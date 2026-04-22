// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Models;

namespace Zarem.Emulator.Interpret;


/// <summary>
/// An <see cref="ITrapLogic{TTrap}"/> for a <see cref="RiscVTrap.EnvironmentCallFromUMode"/> trap.
/// </summary>
public struct ECallULogic : ITrapLogic<RiscVTrap>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RiscVTrap Trap() => RiscVTrap.EnvironmentCallFromUMode;
}

/// <summary>
/// An <see cref="ITrapLogic{TTrap}"/> for a <see cref="RiscVTrap.Breakpoint"/> trap.
/// </summary>
public struct BreakLogic : ITrapLogic<RiscVTrap>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RiscVTrap Trap() => RiscVTrap.Breakpoint;
}


/// <summary>
/// An <see cref="IMultLogic{T, TL}"/> for a signed multiplication operation on 32-bit values.
/// </summary>
public unsafe struct MulhLogic<T, TL> : IAluLogic<T>
    where T : unmanaged, INumber<T>
    where TL : struct, INumber<TL>, IBinaryInteger<TL>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt)
    {
        var shift = sizeof(T) * 4;
        var result = TL.CreateTruncating(rs) * TL.CreateTruncating(rt);
        return T.CreateTruncating(result >> shift);
    }
}

/// <summary>
/// An <see cref="IMultLogic{T, TL}"/> for a signed multiplication operation on 32-bit values.
/// </summary>
public unsafe struct MulhsuLogic<T, TL> : IAluLogic<T>
    where T : unmanaged, INumber<T>
    where TL : struct, INumber<TL>, IBinaryInteger<TL>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt)
    {
        var shift = sizeof(T) * 4;
        var result = TL.CreateTruncating(rs) * TL.CreateTruncating(rt);
        return T.CreateTruncating(result >> shift);
    }
}
