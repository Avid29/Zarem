// Avishai Dernis 2026

using System.Numerics;
using Zarem.Emulator.Machine.Memory;
using Zarem.Emulator.Machine.Registers;
using Zarem.RiscV.Emulator.Config;

namespace Zarem.RiscV.Emulator.Machine;

/// <summary>
/// A class representing a RISC-V CPU.
/// </summary>
public abstract class RiscVCpu<T, TFloat> : RiscVCpu<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    private const int FloatRegisterCount = 32;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVCpu{T, TFloat}"/> class.
    /// </summary>
    public RiscVCpu(RiscVEmulatorConfig config, PhysicalBus bus) : base(config, bus)
    {
        FloatRegisterFile = new FormattedRegisterFile<TFloat>(FloatRegisterCount);
    }

    /// <inheritdoc/>
    public override IFormattedRegisterFile<TFloat>? FloatRegisterFile { get; }
}
