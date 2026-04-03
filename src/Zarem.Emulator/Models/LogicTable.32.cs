// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models;

public partial class LogicTable<T, TSigned>
{
    /// <summary>
    /// An <see cref="IShiftLogic{T2}"/> for a logical left shift operation on 32-bit values.
    /// </summary>
    public struct SllLogic32 : IShiftLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Execute(uint rt, int sa) => rt << sa;
    }

    /// <summary>
    /// An <see cref="IShiftLogic{T2}"/> for a logical right shift operation on 32-bit values.
    /// </summary>
    public struct SrlLogic32 : IShiftLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Execute(uint rt, int sa) => rt >> sa;
    }

    /// <summary>
    /// An <see cref="IShiftLogic{T2}"/> for an arithmetic left shift operation on 32-bit values.
    /// </summary>
    public struct SraLogic32 : IShiftLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Execute(uint rt, int sa) => (uint)((int)rt >> sa);
    }

    /// <summary>
    /// An <see cref="ICheckedAluLogic{T2, TSigned2}"/> for a signed add operation on 32-bit values.
    /// </summary>
    public struct AddLogic32 : ICheckedAluLogic<uint, int>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)((int)rs + (int)rt);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overflow(int a, int b, int r) => ((a ^ r) & (b ^ r)) < 0;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T2}"/> for an unsigned add operation on 32-bit values.
    /// </summary>
    public struct AdduLogic32 : IAluLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => rs + rt;
    }

    /// <summary>
    /// An <see cref="ICheckedAluLogic{T2, TSigned2}"/> for a signed subtraction operation on 32-bit values.
    /// </summary>
    public struct SubLogic32 : ICheckedAluLogic<uint, int>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)((int)rs - (int)rt);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overflow(int a, int b, int r) => ((a ^ b) & (a ^ r)) < 0;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T2}"/> for an unsigned subtract operation on 32-bit values.
    /// </summary>
    public struct SubuLogic32 : IAluLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => rs - rt;
    }

    /// <summary>
    /// An <see cref="IMultLogic{T2}"/> for a signed multiplication operation on 32-bit values.
    /// </summary>
    public struct MultLogic32 : IMultLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint, uint) Compute(uint rs, uint rt)
        {
            var value = (ulong)((long)(int)rs * (int)rt);
            return ((uint)(value >> 32), (uint)value);
        }
    }

    /// <summary>
    /// An <see cref="IMultLogic{T2}"/> for an unsigned multiplication operation on 32-bit values.
    /// </summary>
    public struct MultuLogic32 : IMultLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint, uint) Compute(uint rs, uint rt)
        {
            var value = (ulong)rs * rt;
            return ((uint)(value >> 32), (uint)value);
        }
    }

    /// <summary>
    /// An <see cref="IDivLogic{T2}"/> for a signed divison operation on 32-bit values.
    /// </summary>
    public struct DivLogic32 : IDivLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Divisor(uint rs, uint rt) => rt is not 0 ? (uint)((int)rs / (int)rt) : 0;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Remainder(uint rs, uint rt) => rt is not 0 ? (uint)((int)rs % (int)rt) : rs;
    }

    /// <summary>
    /// An <see cref="IDivLogic{T2}"/> for an unsigned divison operation on 32-bit values.
    /// </summary>
    public struct DivuLogic32 : IDivLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Divisor(uint rs, uint rt) => rt is not 0 ? rs / rt : 0;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Remainder(uint rs, uint rt) => rt is not 0 ? rs % rt : rs;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T2}"/> for a signed multiplication operation on 32-bit values.
    /// </summary>
    public struct MulLogic32 : IAluLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)((int)rs * (int)rt);
    }

    /// <summary>
    /// An <see cref="IMultAddLogic{T2}"/> for a signed multiply and add operation on 32-bit values.
    /// </summary>
    public struct MultAddLogic32 : IMultAddLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint, uint) Compute(uint rs, uint rt, uint hi, uint low)
        {
            long acc = (long)(((ulong)hi << 32) | low);
            acc += (long)(int)rs * (int)rt;
            return ((uint)((ulong)acc >> 32), (uint)acc);
        }
    }

    /// <summary>
    /// An <see cref="IMultAddLogic{T2}"/> for an unsigned multiply and add operation on 32-bit values.
    /// </summary>
    public struct MultAdduLogic32 : IMultAddLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint, uint) Compute(uint rs, uint rt, uint hi, uint low)
        {
            ulong acc = ((ulong)hi << 32) | low;
            acc += rs * rt;
            return ((uint)(acc >> 32), (uint)acc);
        }
    }

    /// <summary>
    /// An <see cref="IMultAddLogic{T2}"/> for a signed multiply and subtract operation on 32-bit values.
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
    /// An <see cref="IMultAddLogic{T2}"/> for an unsigned multiply and subtract operation on 32-bit values.
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

    /// <summary>
    /// An <see cref="IAluLogic{T2}"/> for a counting leading zeros operation on 32-bit values.
    /// </summary>
    public struct ClzLogic32 : IAluLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)BitOperations.LeadingZeroCount(rs);
    }

    /// <summary>
    /// An <see cref="IAluLogic{T2}"/> for a counting leading ones operation on 32-bit values.
    /// </summary>
    public struct CloLogic32 : IAluLogic<uint>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)BitOperations.LeadingZeroCount(~rs);
    }
}
