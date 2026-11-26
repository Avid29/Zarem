// Avishai Dernis 2026

using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Emulator.Machine.Memory.Tlb;
using Zarem.Mips.Emulator.Machine.Registers.CoProcessor0;

namespace Zarem.Mips.Emulator.Machine.CoProcessors.Control;

/// <summary>
/// An interface for a MIPS CoProcessor0 unit.
/// </summary>
public interface IControlCoProcessor : ICoProcessor
{
    /// <summary>
    /// Gets the translation look-aside buffer.
    /// </summary>
    IMipsTlb Tlb { get; }

    /// <summary>
    /// Gets the processor's acting privilege mode.
    /// </summary>
    /// <remarks>
    /// This is not neccesarily the same as the <see cref="StatusRegister.PrivilegeMode"/>.
    /// If the processor is in <see cref="StatusRegister.ErrorLevel"/> or <see cref="StatusRegister.ExceptionLevel"/>, the privilege mode is always kernel, regardless of the value of <see cref="StatusRegister.PrivilegeMode"/>.
    /// </remarks>
    PrivilegeMode ActingPrivilegeMode { get; }

    /// <summary>
    /// Gets or sets the processor's privilege mode.
    /// </summary>
    PrivilegeMode PrivilegeMode { get; set; }

    /// <summary>
    /// Gets or sets if the processor is in 64-bit floating-point mode.
    /// </summary>
    bool FloatingPoint64BitMode { get; set; }
}
