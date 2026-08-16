// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models.Logic;

/// <summary>
/// An <see cref="ICheckedAluLogic{T}"/> for a checked signed add operation.
/// </summary>
public struct CheckedAddLogic<T> : ICheckedAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => rs + rt;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Overflow(T a, T b, T r) => ((a ^ r) & (b ^ r)) < T.Zero;
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> for an unchecked add operation.
/// </summary>
public struct AddLogic<T> : IAluLogic<T>
    where T : unmanaged, INumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => rs + rt;
}

/// <summary>
/// An <see cref="ICheckedAluLogic{T}"/> for a checked signed subtraction operation.
/// </summary>
public struct CheckedSubLogic<T> : ICheckedAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => rs - rt;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Overflow(T a, T b, T r) => ((a ^ b) & (a ^ r)) < T.Zero;
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> for an unsigned subtract operation.
/// </summary>
public struct SubLogic<T> : IAluLogic<T>
    where T : unmanaged, INumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => rs - rt;
}
