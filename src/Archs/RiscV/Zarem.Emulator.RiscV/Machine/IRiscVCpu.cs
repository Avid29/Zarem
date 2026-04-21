// Avishai Dernis 2026

using Zarem.Emulator.Config;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine;

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
