// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models;

#region Interfaces

/// <summary>
/// An interface for shift logic operations.
/// </summary>
public interface IShiftLogic<T>
    where T : unmanaged, IBinaryInteger<T>
{
    /// <summary>
    /// Executes the shift logic operation.
    /// </summary>
    static abstract T Execute(T rt, int sa);
}

/// <summary>
/// An interface for ALU logic operations.
/// </summary>
public interface IAluLogic<T>
    where T : unmanaged, INumber<T>
{
    /// <summary>
    /// Executes the ALU logic operation.
    /// </summary>
    static abstract T Compute(T rs, T rt);
}

/// <summary>
/// An interface for floating-point ALU logic operations.
/// </summary>
public interface IFAluLogic<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>
    /// Executes the ALU logic operation.
    /// </summary>
    static abstract T Compute(T fs);
}

/// <summary>
/// An interface for a rounding operations.
/// </summary>
public interface IRoundLogic<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>
    /// Executes the round operation.
    /// </summary>
    static abstract T Compute(T rs);
}

/// <summary>
/// An interface for ALU logic operations with an overflow check.
/// </summary>
public interface ICheckedAluLogic<T> : IAluLogic<T>
    where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T>
{
    /// <summary>
    /// Checks if an overflow occured.
    /// </summary>
    static abstract bool Overflow(T a, T b, T r);
}

/// <summary>
/// An interface for multiply logic.
/// </summary>
public interface IMultLogic<T, TL>
    where T : unmanaged, INumber<T>
    where TL : unmanaged, INumber<TL>
{
    /// <summary>
    /// Executes the multiply logic operation.
    /// </summary>
    static abstract TL Compute(T rs, T rt);
}

/// <summary>
/// An interface for multiply and add logic operations.
/// </summary>
public interface IMultAddLogic<T, TL>
    where T : unmanaged, INumber<T>
    where TL : unmanaged, INumber<TL>
{
    /// <summary>
    /// Executes the multiply and add logic operation.
    /// </summary>
    static abstract TL Compute(T rs, T rt, TL @base);
}

/// <summary>
/// An interface for divide logic operations.
/// </summary>
public interface IDivLogic<T>
    where T : unmanaged, INumber<T>
{
    /// <summary>
    /// Executes the divisor component divide logic operation.
    /// </summary>
    static abstract T Divisor(T rs, T rt);

    /// <summary>
    /// Executes the remainder component divide logic operation.
    /// </summary>
    static abstract T Remainder(T rs, T rt);
}

/// <summary>
/// An interface for a conditional logic operation.
/// </summary>
public interface ICondLogic<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Executes the conditional logic operation.
    /// </summary>
    static abstract bool Check(T rs, T rt);
}

/// <summary>
/// An interface for a trap operatio.
/// </summary>
public interface ITrapLogic<TTrap>
{
    /// <summary>
    /// Executes the trap operation.
    /// </summary>
    static abstract TTrap Trap();
}

#endregion

#region Shift

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

#endregion

#region Add/Subtract

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

#endregion

#region Multiply/Divide

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

#endregion

#region Multiply and Add/Subtract

/// <summary>
/// An <see cref="IMultAddLogic{T, TL}"/> for a signed multiply and add operation on 32-bit values.
/// </summary>
public struct MultAddLogic<T, TL> : IMultAddLogic<T, TL>
    where T : unmanaged, IBinaryInteger<T>
    where TL : unmanaged, IBinaryInteger<TL>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TL Compute(T rs, T rt, TL @base) => @base + MultLogic<T, TL>.Compute(rs, rt);
}

/// <summary>
/// An <see cref="IMultAddLogic{T, TL}"/> for a signed multiply and subtract operation on 32-bit values.
/// </summary>
public struct MultSubLogic<T, TL> : IMultAddLogic<T, TL>
    where T : unmanaged, IBinaryInteger<T>
    where TL : unmanaged, IBinaryInteger<TL>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TL Compute(T rs, T rt, TL @base) => @base - MultLogic<T, TL>.Compute(rs, rt);
}

#endregion

#region Logical

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

#endregion

#region Conditional

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

#endregion

#region Set Less Than

/// <summary>
/// An <see cref="IAluLogic{T}"/> implementation for a signed set less than logic operation.
/// </summary>
public struct SltLogic<T> : IAluLogic<T>
    where T : unmanaged, INumber<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Compute(T rs, T rt) => rs < rt ? T.One : T.Zero;
}


#endregion

#region Float

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

#endregion

#region Rounding

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

#endregion
