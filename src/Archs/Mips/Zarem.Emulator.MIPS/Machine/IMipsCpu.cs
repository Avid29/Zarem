// Avishai Dernis 2025

using Zarem.Emulator.Config;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a processor unit.
/// </summary>
public interface IMipsCpu : ICpu<IMipsCpu, MipsInstruction, MipsTrap>
{
    /// <summary>
    /// Gets the emulation configuration.
    /// </summary>
    MipsEmulatorConfig Config { get; }

    /// <summary>
    /// Gets the jump address in the delay slot.
    /// </summary>
    ulong? DelaySlot { get; }

    /// <summary>
    /// Gets the floating-point processor unit.
    /// </summary>
    IFloatProcessor FloatProcessor { get; }

    /// <summary>
    /// Gets or sets the value of a general-purpose register on the processor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    ulong this[MipsGpRegister reg] { get; set; }
}
