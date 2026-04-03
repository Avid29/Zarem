// Avishai Dernis 2026

using Zarem.Emulator.Config;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;

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
    /// Gets the system memory
    /// </summary>
    IMemoryAccessor Memory { get; internal set; }
}
