// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models;

/// <summary>
/// A base class for an instruction service table.
/// </summary>
public partial class LogicTable
{
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
    public struct SraLogic<T, TSigned> : IShiftLogic<T>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Execute(T rt, int sa) => T.CreateTruncating((TSigned.CreateTruncating(rt) >> sa));
    }

    #endregion

    #region Add/Subtract

    /// <summary>
    /// An <see cref="ICheckedAluLogic{T, TS}"/> for a signed add operation.
    /// </summary>
    public struct AddLogic<T, TS> : ICheckedAluLogic<T, TS>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => T.CreateTruncating((TS.CreateTruncating(rs)) + TS.CreateTruncating(rt));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overflow(TS a, TS b, TS r) => ((a ^ r) & (b ^ r)) < TS.Zero;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T}"/> for an unsigned add operation.
    /// </summary>
    public struct AdduLogic<T> : IAluLogic<T>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => rs + rt;
    }

    /// <summary>
    /// An <see cref="ICheckedAluLogic{T, TS}"/> for a signed subtraction operation.
    /// </summary>
    public struct SubLogic<T, TS> : ICheckedAluLogic<T, TS>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => T.CreateTruncating((TS.CreateTruncating(rs)) - TS.CreateTruncating(rt));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overflow(TS a, TS b, TS r) => ((a ^ b) & (a ^ r)) < TS.Zero;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T}"/> for an unsigned subtract operation.
    /// </summary>
    public struct SubuLogic<T> : IAluLogic<T>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => rs - rt;
    }

    #endregion

    #region Multiply/Divide

    /// <summary>
    /// An <see cref="IAluLogic{T}"/> for a signed multiplication operation.
    /// </summary>
    public struct MulLogic<T, TS> : IAluLogic<T>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => T.CreateTruncating(TS.CreateTruncating(rs) * TS.CreateTruncating(rt));
    }

    /// <summary>
    /// An <see cref="IMultLogic{T, TL}"/> for a signed multiplication operation on 32-bit values.
    /// </summary>
    public struct MultLogic<T, TL, TLS> : IMultLogic<T, TL>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TL : unmanaged, IBinaryInteger<TL>, IUnsignedNumber<TL>
        where TLS : unmanaged, IBinaryInteger<TLS>, ISignedNumber<TLS>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TL Compute(T rs, T rt) => TL.CreateTruncating(TLS.CreateSaturating(rs) * TLS.CreateSaturating(rt));
    }

    /// <summary>
    /// An <see cref="IMultLogic{TL, TL}"/> for an unsigned multiplication operation on 32-bit values.
    /// </summary>
    public struct MultuLogic<T, TL> : IMultLogic<T, TL>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TL : unmanaged, IBinaryInteger<TL>, IUnsignedNumber<TL>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TL Compute(T rs, T rt) => TL.CreateTruncating(rs) * TL.CreateTruncating(rt);
    }

    /// <summary>
    /// An <see cref="IDivLogic{T}"/> for a signed divison operation.
    /// </summary>
    public struct DivLogic<T, TS> : IDivLogic<T>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Divisor(T rs, T rt) => rt is not 0 ? T.CreateTruncating(TS.CreateTruncating(rs) / TS.CreateTruncating(rt)) : T.Zero;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Remainder(T rs, T rt) => rt is not 0 ? T.CreateTruncating(TS.CreateTruncating(rs) % TS.CreateTruncating(rt)) : rs;
    }

    /// <summary>
    /// An <see cref="IDivLogic{T}"/> for an unsigned divison operation.
    /// </summary>
    public struct DivuLogic<T> : IDivLogic<T>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Divisor(T rs, T rt) => rt is not 0 ? rs / rt : T.Zero;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Remainder(T rs, T rt) => rt is not 0 ? rs % rt : rs;
    }

    #endregion

    #region Multiply and Add/Subtract



    /// <summary>
    /// An <see cref="IMultAddLogic{T, TL}"/> for a signed multiply and add operation on 32-bit values.
    /// </summary>
    public struct MultAddLogic<T, TL, TLS> : IMultAddLogic<T, TL>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TL : unmanaged, IBinaryInteger<TL>, IUnsignedNumber<TL>
        where TLS : unmanaged, IBinaryInteger<TLS>, ISignedNumber<TLS>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TL Compute(T rs, T rt, TL @base) => TL.CreateTruncating(TLS.CreateSaturating(rs) * TLS.CreateSaturating(rt)) + @base;
    }

    /// <summary>
    /// An <see cref="IMultAddLogic{T, TL}"/> for an unsigned multiply and add operation on 32-bit values.
    /// </summary>
    public struct MultAdduLogic<T, TL> : IMultAddLogic<T, TL>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TL : unmanaged, IBinaryInteger<TL>, IUnsignedNumber<TL>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TL Compute(T rs, T rt, TL @base) => TL.CreateSaturating(rs) * TL.CreateSaturating(rt) + @base;
    }

    /// <summary>
    /// An <see cref="IMultAddLogic{T, TL}"/> for a signed multiply and subtract operation on 32-bit values.
    /// </summary>
    public struct MultSubLogic32 : IMultAddLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint, uint) Compute(uint rs, uint rt, uint hi, uint low)
        {
            long acc = (long)(((ulong)hi << 32) | low);
            acc -= (long)(int)rs * (int)rt;
            return ((uint)((ulong)acc >> 32), (uint)acc);
        }
    }

    /// <summary>
    /// An <see cref="IMultAddLogic{T, TL}"/> for an unsigned multiply and subtract operation on 32-bit values.
    /// </summary>
    public struct MultSubuLogic32 : IMultAddLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint, uint) Compute(uint rs, uint rt, uint hi, uint low)
        {
            ulong acc = ((ulong)hi << 32) | low;
            acc -= rs * rt;
            return ((uint)(acc >> 32), (uint)acc);
        }
    }

    #endregion

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
        public static T Compute(T rs, T rt) => T.CreateTruncating(BitOperations.LeadingZeroCount(uint.CreateTruncating(rs)));
    }

    /// <summary>
    /// An <see cref="IAluLogic{T}"/> for a counting leading ones operation.
    /// </summary>
    public struct CloLogic<T> : IAluLogic<T>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => T.CreateTruncating(BitOperations.LeadingZeroCount(uint.CreateTruncating(~rs)));
    }

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
    public struct Xeqz<T> : ICondLogic<T>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rt == T.Zero;
    }

    /// <summary>
    /// An <see cref="ICondLogic{T}"/> implementation for a not equal to zero logic operation.
    /// </summary>
    public struct Xnez<T> : ICondLogic<T>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rt != T.Zero;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T}"/> implementation for a signed set less than logic operation.
    /// </summary>
    public struct SltLogic<T, TS> : IAluLogic<T>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => TS.CreateTruncating(rs) < TS.CreateTruncating(rt) ? T.One : T.Zero;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T}"/> implementation for an unsigned set less than logic operation.
    /// </summary>
    public struct SltuLogic<T> : IAluLogic<T>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => rs < rt ? T.One : T.Zero;
    }
}
