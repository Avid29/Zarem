// Avishai Dernis 2025

using Zarem.Emulator.Machine.CPU;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Emulator.Machine.Tlb;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Machine;

/// <summary>
/// A interface for a MIPS processor unit.
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
    /// Gets the coprocessor 0 unit of the computer system.
    /// </summary>
    ICoProcessor0 CoProcessor0 { get; }

    /// <summary>
    /// Gets the translation look-aside buffer.
    /// </summary>
    IMipsTlb Tlb { get; }

    /// <summary>
    /// Gets or sets the value of a general-purpose register on the processor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    ulong this[MipsGpRegister reg] { get; set; }
}
