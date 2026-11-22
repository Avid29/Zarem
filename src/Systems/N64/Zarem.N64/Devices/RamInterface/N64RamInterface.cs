// Avishai Dernis 2026

using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.N64.Devices.Abstract;

namespace Zarem.N64.Devices.RamInterface;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for ...
/// </summary>
public class N64RamInterface : N64RegisterDeviceBase<RiRegister>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="N64RamInterface"/> class.
    /// </summary>
    public N64RamInterface() : base(new FlatMemoryStore(0x20))
    {
        // Initialize standard default configuration values for RDRAM/RI registers if required
        // e.g., default latency or mode settings
    }

    /// <inheritdoc/>
    public override string Name => "N64 RAM Interface (RI)";

    /// <inheritdoc/>
    public override ulong BusRangeBase => 0x0470_0000;

    /// <inheritdoc/>
    public override ulong BusRangeSize => 0x20;
}
