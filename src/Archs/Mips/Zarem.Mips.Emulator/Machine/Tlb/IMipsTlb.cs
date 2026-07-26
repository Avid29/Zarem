// Avishai Dernis 2026

using Zarem.Emulator.Machine.Memory;

namespace Zarem.Mips.Emulator.Machine.Tlb;

/// <summary>
/// An interface for the MIPS Tlb.
/// </summary>
public interface IMipsTlb : IAddressTranslator
{
    /// <summary>
    /// Gets the number of TLB slots.
    /// </summary>
    int SlotCount { get; }

    /// <summary>
    /// Initializes a TLB segment for the given address space.
    /// </summary>
    int InitilizeSegment(int index, ulong virtualStartAddress, ulong size);
}
