// Avishai Dernis 2025

using System;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Models.Enum;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a processor unit.
/// </summary>
public interface IMipsCpu : ICpu<IMipsCpu, MipsInstruction, MipsTrap>
{
    /// <summary>
    /// An event invoked when the processor requests a shutdown of the emulator.
    /// </summary>
    event EventHandler? ShutdownRequested;

    /// <summary>
    /// Gets the emulation configuration.
    /// </summary>
    MIPSEmulatorConfig Config { get; }

    /// <summary>
    /// Gets the general-purpose register file of the processor.
    /// </summary>
    IRegisterFile RegisterFile { get; }

    /// <summary>
    /// Gets the system memory
    /// </summary>
    IMemorySystem Memory { get; }

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
    ulong this[GPRegister reg] { get; set; }

    /// <summary>
    /// Requests a shutdown.
    /// </summary>
    void RequestShutdown();
}
