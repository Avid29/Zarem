// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Emulator.Models.Logic;
using Zarem.Mips.Emulator.Machine.Enums;

namespace Zarem.Emulator.Models;

public partial class MipsInstructionServiceTable<T, TSigned>
{
    private struct SyscallLogic : ITrapLogic<MipsTrap>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MipsTrap Trap() => MipsTrap.Syscall;
    }

    private struct BreakLogic : ITrapLogic<MipsTrap>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MipsTrap Trap() => MipsTrap.Breakpoint;
    }

    private struct TrapLogic : ITrapLogic<MipsTrap>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MipsTrap Trap() => MipsTrap.Trap;
    }
}
