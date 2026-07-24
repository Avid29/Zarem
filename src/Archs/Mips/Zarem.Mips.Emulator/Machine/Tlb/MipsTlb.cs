// Avishai Dernis 2026

using Zarem.Emulator.Machine.Memory;

namespace Zarem.Mips.Emulator.Machine.Tlb;

/// <summary>
/// A MIPS Translation Lookaside Buffer.
/// </summary>
/// <remarks>
/// Not currently implemented. Just returns a flat mapping.
/// </remarks>
public class MipsTlb : IAddressTranslator
{
    /// <inheritdoc/>
    public ulong Translate(ulong virtualAddress) => virtualAddress;

    /// <inheritdoc/>
    public bool TryTranslate(ulong virtualAddress, out ulong address)
    {
        address = virtualAddress;
        return true;
    }
}
