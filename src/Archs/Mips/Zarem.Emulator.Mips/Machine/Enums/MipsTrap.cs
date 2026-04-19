// Avishai Dernis 2026


// Avishai Dernis 2026

using Zarem.Emulator.Interpret;

namespace Zarem.Emulator.Machine.Enums;

/// <summary>
/// An enum describing the kind of trap that occurred during an <see cref="MipsExecution{T}"/>.
/// </summary>
public enum MipsTrap : byte
{
#pragma warning disable CS1591

    None = 0,
    Interrupt = 0,               // Int (0) - External interrupt
    TlbModification = 1,         // Mod (1) - TLB modification exception
    TlbMissLoad = 2,             // TLBL (2) - TLB exception (load or instruction fetch)
    TlbMissStore = 3,            // TLBS (3) - TLB exception (store)
    AddressErrorLoad = 4,        // AdEL (4) - Address error exception (load or instruction fetch)
    AddressErrorStore = 5,       // AdES (5) - Address error exception (store)
    AddressErrorInstruction = 4, // Alias for AdEL (instruction fetch is a load)
    BusErrorInstruction = 6,     // IBE (6) - Bus error exception (instruction fetch)
    BusErrorData = 7,            // DBE (7) - Bus error exception (data reference)
    Syscall = 8,                 // Sys (8) - Syscall exception
    Breakpoint = 9,              // Bp (9) - Breakpoint exception
    ReservedInstruction = 10,    // RI (10) - Reserved instruction exception
    CoprocessorUnusable = 11,    // CpU (11) - Coprocessor Unusable exception
    ArithmeticOverflow = 12,     // Ov (12) - Arithmetic overflow exception
    Trap = 13,                   // Tr (13) - Trap exception
    FloatingPoint = 15,          // FPE (15) - Floating point exception

#pragma warning restore CS1591
}
