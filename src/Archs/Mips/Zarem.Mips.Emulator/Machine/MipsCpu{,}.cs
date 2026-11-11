// Avishai Dernis 2025

using System.Numerics;
using Zarem.Emulator.Machine.Memory;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Emulator.Machine.CoProcessors;

namespace Zarem.Mips.Emulator.Machine;

/// <summary>
/// A base class representing a processor unit.
/// </summary>
public abstract partial class MipsCpu<T, TFloat> : MipsCpu<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCpu{T, TFloat}"/> class.
    /// </summary>
    public MipsCpu(MipsEmulatorConfig config, PhysicalBus bus) : base(config, bus)
    {
        FloatProcessor = new(CoProcessor0);
    }

    /// <inheritdoc/>
    public override FloatProcessor<TFloat> FloatProcessor { get; }

    /// <inheritdoc/>
    public override void Dispose()
    {
        base.Dispose();
        FloatProcessor.Dispose();
    }
}
