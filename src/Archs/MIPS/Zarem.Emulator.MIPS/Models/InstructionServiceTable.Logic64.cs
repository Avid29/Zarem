// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models;

public partial class InstructionServiceTable<T, TSigned>
{
    private struct SllLogic64 : IShiftLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Execute(ulong rt, int sa) => rt << sa;
    }

    private struct SrlLogic64 : IShiftLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Execute(ulong rt, int sa) => rt >> sa;
    }

    private struct SraLogic64 : IShiftLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Execute(ulong rt, int sa) => (ulong)((long)rt >> sa);
    }

    private struct AddLogic64 : ICheckedAluLogic<ulong, long>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => (ulong)((long)rs + (long)rt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overflow(long a, long b, long r) => ((a ^ r) & (b ^ r)) < 0;
    }

    private struct AdduLogic64 : IAluLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => rs + rt;
    }

    private struct SubLogic64 : ICheckedAluLogic<ulong, long>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => (ulong)((long)rs - (long)rt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overflow(long a, long b, long r) => ((a ^ b) & (a ^ r)) < 0;
    }

    private struct SubuLogic64 : IAluLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => rs - rt;
    }

    private struct MultLogic64 : IMultLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong, ulong) Compute(ulong rs, ulong rt)
        {
            var value = (UInt128)((Int128)(long)rs * (long)rt);
            return ((ulong)(value >> 64), (ulong)value);
        }
    }

    private struct MultuLogic64 : IMultLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong, ulong) Compute(ulong rs, ulong rt)
        {
            var value = (UInt128)rs * rt;
            return ((ulong)(value >> 64), (ulong)value);
        }
}

    private struct DivLogic64 : IDivLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Divisor(ulong rs, ulong rt) => rt is not 0 ? (ulong)((long)rs / (long)rt) : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Remainder(ulong rs, ulong rt) => rt is not 0 ? (ulong)((long)rs % (long)rt) : rs;
    }

    private struct DivuLogic64 : IDivLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Divisor(ulong rs, ulong rt) => rt is not 0 ? rs / rt : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Remainder(ulong rs, ulong rt) => rt is not 0 ? rs % rt : rs;
    }

    private struct MulLogic64 : IAluLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => (ulong)((long)rs * (long)rt);
    }

    private struct MultAddLogic64 : IMultAddLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong, ulong) Compute(ulong rs, ulong rt, ulong hi, ulong low)
        {
            Int128 acc = (Int128)(((UInt128)hi << 64) | low);
            acc += (Int128)(long)rs * (long)rt;
            return ((ulong)((UInt128)acc >> 64), (ulong)acc);
        }
    }

    private struct MultAdduLogic64 : IMultAddLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong, ulong) Compute(ulong rs, ulong rt, ulong hi, ulong low)
        {
            UInt128 acc = ((UInt128)hi << 64) | low;
            acc += rs * rt;
            return ((ulong)(acc >> 64), (ulong)acc);
        }
    }

    private struct MultSubLogic64 : IMultAddLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong, ulong) Compute(ulong rs, ulong rt, ulong hi, ulong low)
        {
            Int128 acc = (Int128)(((UInt128)hi << 64) | low);
            acc -= (Int128)(long)rs * (long)rt;
            return ((ulong)((UInt128)acc >> 64), (ulong)acc);
        }
    }

    private struct MultSubuLogic64 : IMultAddLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong, ulong) Compute(ulong rs, ulong rt, ulong hi, ulong low)
        {
            UInt128 acc = ((UInt128)hi << 64) | low;
            acc -= rs * rt;
            return ((ulong)(acc >> 64), (ulong)acc);
        }
    }

    private struct ClzLogic64 : IAluLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => (ulong)BitOperations.LeadingZeroCount(rs);
    }

    private struct CloLogic64 : IAluLogic<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ulong rs, ulong rt) => (ulong)BitOperations.LeadingZeroCount(~rs);
    }
}
