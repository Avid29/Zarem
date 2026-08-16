// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models.Logic;

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for an AND logic operation.
/// </summary>
public struct AndLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => rs & rt;
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for an OR logic operation.
/// </summary>
public struct OrLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => rs | rt;
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for an XOR logic operation.
/// </summary>
public struct XorLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => rs ^ rt;
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for a NAND logic operation.
/// </summary>
public struct NandLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => ~(rs & rt);
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for a NOR logic operation.
/// </summary>
public struct NorLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => ~(rs | rt);
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for a XNOR logic operation.
/// </summary>
public struct XnorLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => ~(rs ^ rt);
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for a ANDN logic operation.
/// </summary>
public struct AndnLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => rs & (~rt);
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for a ORN logic operation.
/// </summary>
public struct OrnLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => rs | (~rt);
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> for a counting leading zeros operation.
/// </summary>
public struct ClzLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => T.LeadingZeroCount(rs);
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> for a counting leading ones operation.
/// </summary>
public struct CloLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => T.LeadingZeroCount(~rs);
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> for a counting trailing zeros operation.
/// </summary>
public struct CtzLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => T.TrailingZeroCount(rs);
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> for a counting trailing ones operation.
/// </summary>
public struct CtoLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => T.TrailingZeroCount(~rs);
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> for a population (set bit) counting operation.
/// </summary>
public struct CpopLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => T.PopCount(rs);
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> for a bitwise rotate left operation.
/// </summary>
public unsafe struct RolLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt)
    {
        var mask = T.CreateTruncating((sizeof(T) * 8) - 1);
        var amount = int.CreateTruncating(rt & mask);
        return T.RotateLeft(rs, amount);
    }
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> for a bitwise rotate right operation.
/// </summary>
public unsafe struct RorLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt)
    {
        var mask = T.CreateTruncating((sizeof(T) * 8) - 1);
        var amount = int.CreateTruncating(rt & mask);
        return T.RotateRight(rs, amount);
    }
}

/// <summary>
/// An <see cref="IAluLogic{T}"/> for sign extending.
/// </summary>
public struct Sext<T, TSigned, TExtend> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>
    where TExtend : unmanaged, IBinaryInteger<TExtend>, ISignedNumber<TExtend>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => T.CreateTruncating(TSigned.CreateTruncating(TExtend.CreateTruncating(rs)));
}
