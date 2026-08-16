// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models.Logic;

/// <summary>
/// An <see cref="IFAluLogic{T}"/> for a square root operation.
/// </summary>
public struct SqrtLogic<T> : IFAluLogic<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T fs) => T.Sqrt(fs);
}

/// <summary>
/// An <see cref="IFAluLogic{T}"/> for an absolute value operation.
/// </summary>
public struct AbsLogic<T> : IFAluLogic<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T fs) => T.Abs(fs);
}

/// <summary>
/// An <see cref="IFAluLogic{T}"/> for a move operation.
/// </summary>
public struct MovLogic<T> : IFAluLogic<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T fs) => fs;
}

/// <summary>
/// An <see cref="IFAluLogic{T}"/> for a negate operation.
/// </summary>
public struct NegLogic<T> : IFAluLogic<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T fs) => -fs;
}

/// <summary>
/// An <see cref="IFAluLogic{T}"/> for a reciprical operation.
/// </summary>
public struct RecipLogic<T> : IFAluLogic<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T fs) => T.ReciprocalEstimate(fs);
}

/// <summary>
/// An <see cref="IFAluLogic{T}"/> for a square root reciprical operation.
/// </summary>
public struct RSqrtLogic<T> : IFAluLogic<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T fs) => T.ReciprocalSqrtEstimate(fs);
}
