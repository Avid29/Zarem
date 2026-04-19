// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Emulator.Machine.Enums;

namespace Zarem.Emulator.Models;

public partial class MipsInstructionServiceTable<T, TS>
{
    private interface ITrapLogic
    {
        static abstract MipsTrap Trap();
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
}
