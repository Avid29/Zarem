// Avishai Dernis 2025

using Zarem.Emulator.Models.Interpret;

namespace Zarem.Emulator.Models.Enums;

/// <summary>
/// An enum describing the secondary effect of an <see cref="MipsExecution{T}"/>.
/// </summary>
public enum SideEffect
{
    /// <summary>
    /// No secondary effect.
    /// </summary>
    None,

    /// <summary>
    /// Writes to the low register.
    /// </summary>
    /// <remarks>
    /// Not flagged.
    /// </remarks>
    Low = 0x1,

    /// <summary>
    /// Writes to the high register.
    /// </summary>
    /// <remarks>
    /// Not flagged.
    /// </remarks>
    High = 0x2,

    /// <summary>
    /// Writes to both the low and high register.
    /// </summary>
    /// <remarks>
    /// Somewhat by coincidence, this is equivalent to <see cref="Low"/> | <see cref="High"/>,
    /// but other values could also use to two low bits, so they are not flagged.
    /// </remarks>
    HighLow = Low | High,

    /// <summary>
    /// Reads from memory.
    /// </summary>
    ReadMemory,

    /// <summary>
    /// Reads from memory, preserving the sign.
    /// </summary>
    ReadMemorySigned,

    /// <summary>
    /// Writes to memory.
    /// </summary>
    WriteMemory,

    /// <summary>
    /// Writes back to the program counter (with a delay if enabled).
    /// </summary>
    ProgramCounter,

    /// <summary>
    /// Writes to co-processor.
    /// </summary>
    WriteCoProc0,

    /// <summary>
    /// Writes a word to the floating-point processor.
    /// </summary>
    /// <remarks>
    /// Could be a float, could be an int.
    /// </remarks>
    WriteFloat,

    /// <summary>
    /// Writes a dword to the floating-point processor.
    /// </summary>
    /// <remarks>
    /// Could be a double, could be a long.
    /// </remarks>
    WriteDouble,

    /// <summary>
    /// Reads a TLB entry into EntryHi/Lo registers.
    /// </summary>
    TLBRead,

    /// <summary>
    /// Writes EntryHi/Lo registers into a specific TLB slot.
    /// </summary>
    TLBWriteIndexed,

    /// <summary>
    /// Writes EntryHi/Lo registers into a random TLB slot.
    /// </summary>
    TLBWriteRandom,

    /// <summary>
    /// Searches TLB for an entry matching EntryHi and updates Index register.
    /// </summary>
    TLBProbe,
}
