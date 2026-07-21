// Avishai Dernis 2026

using Zarem.Emulator.Machine.Memory;

namespace Zarem.RiscV.Emulator.Machine;

/// <summary>
/// A RISC-V Translation Lookaside Buffer.
/// </summary>
/// <remarks>
/// Not currently implemented. Just returns a flat mapping.
/// </remarks>
public class RiscVTlb : IAddressTranslator
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
