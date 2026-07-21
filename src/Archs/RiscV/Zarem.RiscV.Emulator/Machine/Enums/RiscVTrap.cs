// Avishai Dernis 2026

using Zarem.RiscV.Emulator.Interpret;

namespace Zarem.RiscV.Emulator.Machine.Enums;

/// <summary>
/// An enum describing the kind of trap that occurred during an <see cref="RiscVExecution{T}"/>.
/// </summary>
public enum RiscVTrap : uint
{
#pragma warning disable CS1591

    None = 0xFFFFFFFF, // Using a sentinel value since 0 is a valid exception

    // --- Synchronous Exceptions (mcause bit 31 = 0) ---
    InstructionAddressMisaligned = 0,
    InstructionAccessFault = 1,
    IllegalInstruction = 2,
    Breakpoint = 3,
    LoadAddressMisaligned = 4,
    LoadAccessFault = 5,
    StoreAddressMisaligned = 6,
    StoreAccessFault = 7,
    EnvironmentCallFromUMode = 8,  // User-level Syscall
    EnvironmentCallFromSMode = 9,  // Supervisor-level Syscall
    EnvironmentCallFromMMode = 11, // Machine-level Syscall
    InstructionPageFault = 12,
    LoadPageFault = 13,
    StorePageFault = 15,

    // --- Interrupts (mcause bit 31 = 1) ---
    // Note: These usually have the high bit set in the actual hardware register
    UserSoftwareInterrupt = 0x80000000,
    SupervisorSoftwareInterrupt = 0x80000001,
    MachineSoftwareInterrupt = 0x80000003,
    UserTimerInterrupt = 0x80000004,
    SupervisorTimerInterrupt = 0x80000005,
    MachineTimerInterrupt = 0x80000007,
    UserExternalInterrupt = 0x80000008,
    SupervisorExternalInterrupt = 0x80000009,
    MachineExternalInterrupt = 0x8000000b

#pragma warning restore CS1591
}
