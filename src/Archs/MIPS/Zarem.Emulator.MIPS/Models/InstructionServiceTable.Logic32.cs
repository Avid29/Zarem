// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Models.Enum;

namespace Zarem.Emulator.Models;

public partial class InstructionServiceTable<T, TSigned, TLong>
{
    private struct SllLogic32 : IShiftLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Execute(uint rt, int sa) => rt << sa;
    }

    private struct SrlLogic32 : IShiftLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Execute(uint rt, int sa) => rt >> sa;
    }

    private struct SraLogic32 : IShiftLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Execute(uint rt, int sa) => (uint)((int)rt >> sa);
    }

    private struct AddLogic32 : ICheckedAluLogic<uint, int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)((int)rs + (int)rt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overflow(int a, int b, int r) => ((a ^ r) & (b ^ r)) < 0;
    }

    private struct AdduLogic32 : IAluLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => rs + rt;
    }

    private struct SubLogic32 : ICheckedAluLogic<uint, int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)((int)rs - (int)rt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overflow(int a, int b, int r) => ((a ^ b) & (a ^ r)) < 0;
    }

    private struct SubuLogic32 : IAluLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => rs - rt;
    }

    private struct MultLogic32 : IMultLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint, uint) Compute(uint rs, uint rt)
        {
            var value = (ulong)((long)(int)rs * (int)rt);
            return (((uint)value >> 32), (uint)value);
        }
    }

    private struct MultuLogic32 : IMultLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint, uint) Compute(uint rs, uint rt)
        {
            var value = (ulong)rs * rt;
            return (((uint) value >> 32), (uint)(value & 0xFFFF_FFF));
        }
}

    private struct DivLogic32 : IDivLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Divisor(uint rs, uint rt) => rt is not 0 ? (uint)((int)rs / (int)rt) : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Remainder(uint rs, uint rt) => rt is not 0 ? (uint)((int)rs % (int)rt) : rs;
    }

    private struct DivuLogic32 : IDivLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Divisor(uint rs, uint rt) => rt is not 0 ? rs / rt : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Remainder(uint rs, uint rt) => rt is not 0 ? rs % rt : rs;
    }

    private struct XgeLogic32 : ICondLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => (int)rs >= (int)rt;
    }

    private struct XgeuLogic32 : ICondLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => rs >= rt;
    }

    private struct XltLogic32 : ICondLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => (int)rs < (int)rt;
    }

    private struct XltuLogic32 : ICondLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => rs < rt;
    }

    private struct XeqLogic32 : ICondLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => rs == rt;
    }

    private struct XneLogic32 : ICondLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => rs != rt;
    }

    private struct XlezLogic32 : ICondLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => (int)rs <= 0;
    }

    private struct XltzLogic32 : ICondLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => (int)rs < 0;
    }

    private struct XgezLogic32 : ICondLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => (int)rs >= 0;
    }

    private struct XgtzLogic32 : ICondLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => (int)rs > 0;
    }

    private struct SyscallLogic : ITrapLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MipsTrap Trap() => MipsTrap.Syscall;
    }

    private struct BreakLogic : ITrapLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MipsTrap Trap() => MipsTrap.Breakpoint;
    }

    private struct TrapLogic : ITrapLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MipsTrap Trap() => MipsTrap.Trap;
    }

    private struct MulLogic32 : IAluLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)((long)(int)rs * (int)rt);
    }

    private struct MultAddLogic32 : IMultAddLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint, uint) Compute(uint rs, uint rt, uint hi, uint low)
        {
            long acc = (long)(((ulong)hi << 32) | low);
            acc += (long)(int)rs * (int)rt;
            return ((uint)((ulong)acc >> 32), (uint)acc);
        }
    }

    private struct MultAdduLogic32 : IMultAddLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint, uint) Compute(uint rs, uint rt, uint hi, uint low)
        {
            ulong acc = (hi << 32) | low;
            acc += rs * rt;
            return ((uint)(acc >> 32), (uint)acc);
        }
    }

    private struct MultSubLogic32 : IMultAddLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint, uint) Compute(uint rs, uint rt, uint hi, uint low)
        {
            long acc = (long)(((ulong)hi << 32) | low);
            acc -= (long)(int)rs * (int)rt;
            return ((uint)((ulong)acc >> 32), (uint)acc);
        }
    }

    private struct MultSubuLogic32 : IMultAddLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint, uint) Compute(uint rs, uint rt, uint hi, uint low)
        {
            ulong acc = (hi << 32) | low;
            acc -= rs * rt;
            return ((uint)(acc >> 32), (uint)acc);
        }
    }

    private struct ClzLogic32 : IAluLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)BitOperations.LeadingZeroCount(rs);
    }

    private struct CloLogic32 : IAluLogic<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)BitOperations.LeadingZeroCount(~rs);
    }
}
