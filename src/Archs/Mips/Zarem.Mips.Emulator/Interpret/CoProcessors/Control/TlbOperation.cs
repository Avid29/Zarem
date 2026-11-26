// Avishai Dernis 2026

namespace Zarem.Mips.Emulator.Interpret.CoProcessors.Control;

/// <summary>
/// An enum describing the TLB effect of a <see cref="MipsControlExecution{T}"/>.
/// </summary>
public enum TlbOperation
{
    /// <summary>
    /// No operation
    /// </summary>
    None, 

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
