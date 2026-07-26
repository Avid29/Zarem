// Avishai Dernis 2026

using Zarem.Emulator.Machine.Memory;
using Zarem.Emulator.Models.Enums;

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
    public MemoryAccessResult TryTranslate(ulong virtualAddress, out ulong address)
    {
        address = virtualAddress;
        return MemoryAccessResult.Success;
    }
}
