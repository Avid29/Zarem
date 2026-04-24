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
/// An <see cref="IAluLogic{T}"/> for a signed multiplication operation on 32-bit values.
/// </summary>
public unsafe struct MulhLogic<T, TLong> : IAluLogic<T>
    where T : unmanaged, INumber<T>
    where TLong : struct, INumber<TLong>, IBinaryInteger<TLong>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt)
    {
        var shift = sizeof(T) * 4;
        var result = TLong.CreateTruncating(rs) * TLong.CreateTruncating(rt);
        return T.CreateTruncating(result >> shift);
    }
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> for a signed multiplication operation on 32-bit values.
/// </summary>
public unsafe struct MulhsuLogic<T, TLong> : IAluLogic<T>
    where T : unmanaged, INumber<T>
    where TLong : struct, INumber<TLong>, IBinaryInteger<TLong>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt)
    {
        var shift = sizeof(T) * 4;
        var result = TLong.CreateTruncating(rs) * TLong.CreateTruncating(rt);
        return T.CreateTruncating(result >> shift);
    }
}
