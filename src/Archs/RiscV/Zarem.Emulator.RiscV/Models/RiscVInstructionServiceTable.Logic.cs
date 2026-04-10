// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Emulator.Models.Enums;

namespace Zarem.Emulator.Models;

public partial class RiscVInstructionServiceTable<T, TS>
{
    private interface ITrapLogic
    {
        static abstract RiscVTrap Trap();
    }

    private struct ECallLogic : ITrapLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RiscVTrap Trap() => RiscVTrap.EnvironmentCallFromUMode;
    }

    private struct BreakLogic : ITrapLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RiscVTrap Trap() => RiscVTrap.Breakpoint;
    }
}
