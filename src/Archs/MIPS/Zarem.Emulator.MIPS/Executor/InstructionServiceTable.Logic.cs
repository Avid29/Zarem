// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Emulator.Executor.Enum;

namespace Zarem.Emulator.Executor;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="Execution"/> models.
/// </summary>
public partial struct InstructionServiceTable
{
    private interface IShiftLogic
    {
        static abstract uint Execute(uint rt, int sa);
    }

    private interface IAluLogic
    {
        static abstract uint Compute(uint rs, uint rt);
    }

    private interface ICheckedAluLogic : IAluLogic
    {
        static abstract bool Overflow(int a, int b, int r);
    }

    private interface IMultLogic
    {
        static abstract ulong Compute(uint rs, uint rt);
    }

    private interface IDivLogic
    {
        static abstract uint Divisor(uint rs, uint rt);

        static abstract uint Remainder(uint rs, uint rt);
    }

    private interface ITrapLogic
    {
        static abstract MipsTrap Trap();
    }

    private interface ICondLogic
    {
        static abstract bool Check(uint rs, uint rt);
    }

    private struct SllLogic : IShiftLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Execute(uint rt, int sa) => rt << sa;
    }

    private struct SrlLogic : IShiftLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Execute(uint rt, int sa) => rt >> sa;
    }

    private struct SraLogic : IShiftLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Execute(uint rt, int sa) => (uint)((int)rt >> sa);
    }

    private struct AddLogic : ICheckedAluLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)((int)rs + (int)rt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overflow(int a, int b, int r) => ((a ^ r) & (b ^ r)) < 0;
    }

    private struct AdduLogic : IAluLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => rs + rt;
    }

    private struct SubLogic : ICheckedAluLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)((int)rs - (int)rt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overflow(int a, int b, int r) => ((a ^ b) & (a ^ r)) < 0;
    }

    private struct SubuLogic : IAluLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => rs - rt;
    }

    private struct MultLogic : IMultLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(uint rs, uint rt) => (ulong)((long)(int)rs * (int)rt);
    }

    private struct MultuLogic : IMultLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(uint rs, uint rt) => (ulong)rs * rt;
    }

    private struct DivLogic : IDivLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Divisor(uint rs, uint rt) => rt is not 0 ? (uint)((int)rs / (int)rt) : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Remainder(uint rs, uint rt) => rt is not 0 ? (uint)((int)rs % (int)rt) : rs;
    }

    private struct DivuLogic : IDivLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Divisor(uint rs, uint rt) => rt is not 0 ? rs / rt : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Remainder(uint rs, uint rt) => rt is not 0 ? rs % rt : rs;
    }

    private struct AndLogic : IAluLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => rs & rt;
    }

    private struct OrLogic : IAluLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => rs | rt;
    }

    private struct XorLogic : IAluLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => rs ^ rt;
    }

    private struct NorLogic : IAluLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => ~(rs | rt);
    }

    private struct SltLogic : IAluLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)((int)rs < (int)rt ? 1 : 0);
    }

    private struct SltuLogic : IAluLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint rs, uint rt) => (uint)(rs < rt ? 1 : 0);
    }

    private struct XgeLogic : ICondLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => (int)rs >= (int)rt;
    }

    private struct XgeuLogic : ICondLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => rs >= rt;
    }

    private struct XltLogic : ICondLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => (int)rs < (int)rt;
    }

    private struct XltuLogic : ICondLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => rs < rt;
    }

    private struct XeqLogic : ICondLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => rs == rt;
    }

    private struct XneLogic : ICondLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(uint rs, uint rt) => rs != rt;
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
}
