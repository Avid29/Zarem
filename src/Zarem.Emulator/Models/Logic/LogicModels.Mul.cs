// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models.Logic;

/// <summary>
/// An <see cref="IAluLogic{T}"/> for a multiplication operation.
/// </summary>
public struct MulLogic<T> : IAluLogic<T>
    where T : unmanaged, INumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => rs * rt;
}

/// <summary>
/// An <see cref="IMultLogic{T, TL}"/> for a signed multiplication operation on 32-bit values.
/// </summary>
public struct MultLogic<T, TL> : IMultLogic<T, TL>
    where T : unmanaged, INumber<T>
    where TL : unmanaged, INumber<TL>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TL Compute(T rs, T rt) => TL.CreateTruncating(rs) * TL.CreateTruncating(rt);
}

/// <summary>
/// An <see cref="IDivLogic{T}"/> for a divison operation.
/// </summary>
public struct DivLogic<T> : IAluLogic<T>, IDivLogic<T>
    where T : unmanaged, INumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => Divisor(rs, rt);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Divisor(T rs, T rt) => rt != T.Zero ? rs / rt : T.Zero;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Remainder(T rs, T rt) => rt != T.Zero ? rs % rt : rs;
}

/// <summary>
/// An <see cref="IDivLogic{T}"/> for a divison operation.
/// </summary>
public struct RemLogic<T> : IAluLogic<T>
    where T : unmanaged, INumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => DivLogic<T>.Remainder(rs, rt);
}
