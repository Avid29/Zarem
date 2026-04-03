// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models;

public partial class LogicTable<T, TSigned>
{
    /// <summary>
    /// An <see cref="IShiftLogic{T2}"/> for a logical left shift operation on 64-bit values.
    /// </summary>
    public struct SllLogic64 : IShiftLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Execute(ulong rt, int sa) => rt << sa;
    }

    /// <summary>
    /// An <see cref="IShiftLogic{T2}"/> for a logical right shift operation on 64-bit values.
    /// </summary>
    public struct SrlLogic64 : IShiftLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Execute(ulong rt, int sa) => rt >> sa;
    }

    /// <summary>
    /// An <see cref="IShiftLogic{T2}"/> for an arithmetic left shift operation on 64-bit values.
    /// </summary>
    public struct SraLogic64 : IShiftLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Execute(ulong rt, int sa) => (ulong)((long)rt >> sa);
    }

    /// <summary>
    /// An <see cref="ICheckedAluLogic{T2, TSigned2}"/> for a signed add operation on 64-bit values.
    /// </summary>
    public struct AddLogic64 : ICheckedAluLogic<ulong, long>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => (ulong)((long)rs + (long)rt);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overflow(long a, long b, long r) => ((a ^ r) & (b ^ r)) < 0;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T2}"/> for an unsigned add operation on 64-bit values.
    /// </summary>
    public struct AdduLogic64 : IAluLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => rs + rt;
    }

    /// <summary>
    /// An <see cref="ICheckedAluLogic{T2, TSigned2}"/> for a signed subtraction operation on 64-bit values.
    /// </summary>
    public struct SubLogic64 : ICheckedAluLogic<ulong, long>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => (ulong)((long)rs - (long)rt);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overflow(long a, long b, long r) => ((a ^ b) & (a ^ r)) < 0;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T2}"/> for an unsigned subtract operation on 64-bit values.
    /// </summary>
    public struct SubuLogic64 : IAluLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => rs - rt;
    }

    /// <summary>
    /// An <see cref="IMultLogic{T2}"/> for a signed multiplication operation on 64-bit values.
    /// </summary>
    public struct MultLogic64 : IMultLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong, ulong) Compute(ulong rs, ulong rt)
        {
            var value = (UInt128)((Int128)(long)rs * (long)rt);
            return ((ulong)(value >> 64), (ulong)value);
        }
    }

    /// <summary>
    /// An <see cref="IMultLogic{T2}"/> for an unsigned multiplication operation on 64-bit values.
    /// </summary>
    public struct MultuLogic64 : IMultLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong, ulong) Compute(ulong rs, ulong rt)
        {
            var value = (UInt128)rs * rt;
            return ((ulong)(value >> 64), (ulong)value);
        }
    }

    /// <summary>
    /// An <see cref="IDivLogic{T2}"/> for a signed divison operation on 64-bit values.
    /// </summary>
    public struct DivLogic64 : IDivLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Divisor(ulong rs, ulong rt) => rt is not 0 ? (ulong)((long)rs / (long)rt) : 0;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Remainder(ulong rs, ulong rt) => rt is not 0 ? (ulong)((long)rs % (long)rt) : rs;
    }

    /// <summary>
    /// An <see cref="IDivLogic{T2}"/> for an unsigned divison operation on 64-bit values.
    /// </summary>
    public struct DivuLogic64 : IDivLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Divisor(ulong rs, ulong rt) => rt is not 0 ? rs / rt : 0;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Remainder(ulong rs, ulong rt) => rt is not 0 ? rs % rt : rs;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T2}"/> for a signed multiplication operation on 64-bit values.
    /// </summary>
    public struct MulLogic64 : IAluLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => (ulong)((long)rs * (long)rt);
    }

    /// <summary>
    /// An <see cref="IMultAddLogic{T2}"/> for a signed multiply and add operation on 64-bit values.
    /// </summary>
    public struct MultAddLogic64 : IMultAddLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong, ulong) Compute(ulong rs, ulong rt, ulong hi, ulong low)
        {
            Int128 acc = (Int128)(((UInt128)hi << 64) | low);
            acc += (Int128)(long)rs * (long)rt;
            return ((ulong)((UInt128)acc >> 64), (ulong)acc);
        }
    }

    /// <summary>
    /// An <see cref="IMultAddLogic{T2}"/> for an unsigned multiply and add operation on 64-bit values.
    /// </summary>
    public struct MultAdduLogic64 : IMultAddLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong, ulong) Compute(ulong rs, ulong rt, ulong hi, ulong low)
        {
            UInt128 acc = ((UInt128)hi << 64) | low;
            acc += rs * rt;
            return ((ulong)(acc >> 64), (ulong)acc);
        }
    }

    /// <summary>
    /// An <see cref="IMultAddLogic{T2}"/> for a signed multiply and subtract operation on 64-bit values.
    /// </summary>
    public struct MultSubLogic64 : IMultAddLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong, ulong) Compute(ulong rs, ulong rt, ulong hi, ulong low)
        {
            Int128 acc = (Int128)(((UInt128)hi << 64) | low);
            acc -= (Int128)(long)rs * (long)rt;
            return ((ulong)((UInt128)acc >> 64), (ulong)acc);
        }
    }

    /// <summary>
    /// An <see cref="IMultAddLogic{T2}"/> for an unsigned multiply and subtract operation on 64-bit values.
    /// </summary>
    public struct MultSubuLogic64 : IMultAddLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong, ulong) Compute(ulong rs, ulong rt, ulong hi, ulong low)
        {
            UInt128 acc = ((UInt128)hi << 64) | low;
            acc -= rs * rt;
            return ((ulong)(acc >> 64), (ulong)acc);
        }
    }

    /// <summary>
    /// An <see cref="IAluLogic{T2}"/> for a counting leading zeros operation on 64-bit values.
    /// </summary>
    public struct ClzLogic64 : IAluLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => (ulong)BitOperations.LeadingZeroCount(rs);
    }

    /// <summary>
    /// An <see cref="IAluLogic{T2}"/> for a counting leading ones operation on 64-bit values.
    /// </summary>
    public struct CloLogic64 : IAluLogic<ulong>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => (ulong)BitOperations.LeadingZeroCount(~rs);
    }
}
