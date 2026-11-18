// Avishai Dernis 2026

using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.N64.Devices.RealityCoProcessor.Abstract;
using Zarem.N64.Models.Enums.Registers;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for ...
/// </summary>
public class N64AudioInterface : N64RegisterDeviceBase<AiRegister>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="N64AudioInterface"/> class.
    /// </summary>
    public N64AudioInterface() : base(new FlatMemoryStore(0x18))
    {
    }

    /// <inheritdoc/>
    public override string Name => "N64 Audio Interface (AI)";

    /// <inheritdoc/>
    public override ulong BusRangeBase => 0x0450_0000;

    /// <inheritdoc/>
    public override ulong BusRangeSize => 0x20;
}
