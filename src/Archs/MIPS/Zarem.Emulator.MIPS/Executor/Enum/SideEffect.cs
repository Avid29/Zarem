// Avishai Dernis 2025

namespace Zarem.Emulator.Executor.Enum;

/// <summary>
/// An enum describing the secondary effect of an <see cref="Execution"/>.
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
    /// Writes to memory.
    /// </summary>
    ReadMemory,

    /// <summary>
    /// Writes to memory.
    /// </summary>
    WriteMemory,

    /// <summary>
    /// Writes an absolute value to the program counter.
    /// </summary>
    JumpProgramCounter,

    /// <summary>
    /// Writes a relative value to the program counter.
    /// </summary>
    BranchProgramCounter,

    /// <summary>
    /// Writes to co-processor.
    /// </summary>
    WriteCoProc,
}
