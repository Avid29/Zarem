// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models.Logic;

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for a set less than logic operation.
/// </summary>
public struct SltLogic<T> : IAluLogic<T>
    where T : unmanaged, INumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => rs < rt ? T.One : T.Zero;
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for a min logic operation.
/// </summary>
public struct MinLogic<T> : IAluLogic<T>
    where T : unmanaged, INumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => T.Min(rs, rt);
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for a max logic operation.
/// </summary>
public struct MaxLogic<T> : IAluLogic<T>
    where T : unmanaged, INumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => T.Max(rs, rt);
}
