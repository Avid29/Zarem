// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models.Logic;

/// <summary>
/// An <see cref="ICondLogic{T}"/> implementation for a signed greater than or equal to logic operation.
/// </summary>
public struct XgeLogic<T, TSigned> : ICondLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(T rs, T rt) => TSigned.CreateTruncating(rs) >= TSigned.CreateTruncating(rt);
}

/// <summary>
/// An <see cref="ICondLogic{T}"/> implementation for an unsigned greater than or equal to logic operation.
/// </summary>
public struct XgeuLogic<T> : ICondLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(T rs, T rt) => rs >= rt;
}

/// <summary>
/// An <see cref="ICondLogic{T}"/> implementation for a signed less than to logic operation.
/// </summary>
public struct XltLogic<T, TS> : ICondLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(T rs, T rt) => TS.CreateTruncating(rs) < TS.CreateTruncating(rt);
}

/// <summary>
/// An <see cref="ICondLogic{T}"/> implementation for an unsigned less than to logic operation.
/// </summary>
public struct XltuLogic<T> : ICondLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(T rs, T rt) => rs < rt;
}

/// <summary>
/// An <see cref="ICondLogic{T}"/> implementation for an equal to logic operation.
/// </summary>
public struct XeqLogic<T> : ICondLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(T rs, T rt) => rs == rt;
}

/// <summary>
/// An <see cref="ICondLogic{T}"/> implementation for a not equal to logic operation.
/// </summary>
public struct XneLogic<T> : ICondLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(T rs, T rt) => rs != rt;
}

/// <summary>
/// An <see cref="ICondLogic{T}"/> implementation for a less than or equal to zero logic operation.
/// </summary>
public struct XlezLogic<T, TS> : ICondLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(T rs, T rt) => TS.CreateTruncating(rs) <= TS.Zero;
}

/// <summary>
/// An <see cref="ICondLogic{T}"/> implementation for a less than zero logic operation.
/// </summary>
public struct XltzLogic<T, TS> : ICondLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(T rs, T rt) => TS.CreateTruncating(rs) < TS.Zero;
}

/// <summary>
/// An <see cref="ICondLogic{T}"/> implementation for a greater than or equal to zero logic operation.
/// </summary>
public struct XgezLogic<T, TS> : ICondLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(T rs, T rt) => TS.CreateTruncating(rs) >= TS.Zero;
}

/// <summary>
/// An <see cref="ICondLogic{T}"/> implementation for a greater than zero logic operation.
/// </summary>
public struct XgtzLogic<T, TS> : ICondLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(T rs, T rt) => TS.CreateTruncating(rs) > TS.Zero;
}

/// <summary>
/// An <see cref="ICondLogic{T}"/> implementation for an equal to zero logic operation.
/// </summary>
public struct XeqzLogic<T> : ICondLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(T rs, T rt) => rt == T.Zero;
}

/// <summary>
/// An <see cref="ICondLogic{T}"/> implementation for a not equal to zero logic operation.
/// </summary>
public struct XnezLogic<T> : ICondLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(T rs, T rt) => rt != T.Zero;
}
