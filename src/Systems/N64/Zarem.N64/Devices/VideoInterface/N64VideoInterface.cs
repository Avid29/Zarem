// Avishai Dernis 2026

using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.N64.Devices.Abstract;

namespace Zarem.N64.Devices.VideoInterface;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for output and timing.
/// </summary>
public class N64VideoInterface : N64RegisterDeviceBase<ViRegister>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="N64VideoInterface"/> class.
    /// </summary>
    public N64VideoInterface() : base(new FlatMemoryStore(0x40))
    {
    }

    /// <inheritdoc/>
    public override string Name => "N64 Video Interface (VI)";

    /// <inheritdoc/>
    public override ulong BusRangeBase => 0x0440_0000;

    /// <inheritdoc/>
    public override ulong BusRangeSize => 0x40;

    /// <inheritdoc/>
    protected override bool HasWriteSideEffects(ViRegister register, out bool directWrite)
    {
        directWrite = true;
        return false;
    }

    /// <inheritdoc/>
    protected override void HandleWriteSideEffects(ViRegister register, uint value)
    {
        // TODO: Implement
    }
}
