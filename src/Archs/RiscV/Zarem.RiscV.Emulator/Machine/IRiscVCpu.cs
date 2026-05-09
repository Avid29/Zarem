// Avishai Dernis 2026

using Zarem.Emulator.Machine.CPU;
using Zarem.RiscV.Emulator.Config;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Emulator.Machine;

/// <summary>
/// A class representing a RISC-V CPU.
/// </summary>
public interface IRiscVCpu : ICpu<IRiscVCpu, RiscVInstruction, RiscVTrap>
{
    /// <summary>
    /// Gets the emulation configuration.
    /// </summary>
    public RiscVEmulatorConfig Config { get; }

    /// <summary>
    /// Gets or sets the value of a general-purpose register on the processor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    ulong this[RiscVGpRegister reg] { get; set; }
}
