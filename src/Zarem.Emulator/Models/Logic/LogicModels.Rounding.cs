// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models.Logic;

/// <summary>
/// An <see cref="IRoundLogic{T}"/> for a rounding operation.
/// </summary>
public struct RoundLogic<T> : IRoundLogic<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs) => T.Round(rs);
}

/// <summary>
/// An <see cref="IRoundLogic{T}"/> for a truncate operation.
/// </summary>
public struct TruncLogic<T> : IRoundLogic<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs) => T.Truncate(rs);
}

/// <summary>
/// An <see cref="IRoundLogic{T}"/> for a ceiling operation.
/// </summary>
public struct CeilingLogic<T> : IRoundLogic<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs) => T.Ceiling(rs);
}

/// <summary>
/// An <see cref="IRoundLogic{T}"/> for a floor operation.
/// </summary>
public struct FloorLogic<T> : IRoundLogic<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs) => T.Floor(rs);
}
