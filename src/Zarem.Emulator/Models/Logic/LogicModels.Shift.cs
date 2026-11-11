// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models.Logic;

/// <summary>
/// An <see cref="IShiftLogic{T}"/> for a logical left shift operation.
/// </summary>
public struct SllLogic<T> : IShiftLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Execute(T rt, int sa) => rt << sa;
}

/// <summary>
/// An <see cref="IShiftLogic{T}"/> for a logical right shift operation.
/// </summary>
public struct SrlLogic<T> : IShiftLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Execute(T rt, int sa) => rt >> sa;
}

/// <summary>
/// An <see cref="IShiftLogic{T}"/> for an arithmetic right shift operation.
/// </summary>
public struct SraLogic<T> : IShiftLogic<T>
    where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Execute(T rt, int sa) => rt >> sa;
}
