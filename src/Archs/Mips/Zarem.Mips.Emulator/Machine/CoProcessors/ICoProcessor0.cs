// Avishai Dernis 2026

using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Emulator.Machine.Registers.CoProcessor0;

namespace Zarem.Mips.Emulator.Machine.CoProcessors;

/// <summary>
/// An interface for a MIPS CoProcessor0 unit.
/// </summary>
public interface ICoProcessor0
{
    /// <summary>
    /// Gets the processor's acting privilege mode.
    /// </summary>
    /// <remarks>
    /// This is not neccesarily the same as the <see cref="StatusRegister.PrivilegeMode"/>.
    /// If the processor is in <see cref="StatusRegister.ErrorLevel"/> or <see cref="StatusRegister.ExceptionLevel"/>, the privilege mode is always kernel, regardless of the value of <see cref="StatusRegister.PrivilegeMode"/>.
    /// </remarks>
    public PrivilegeMode ActingPrivilegeMode { get; }

    /// <summary>
    /// Gets or sets the processor's privilege mode.
    /// </summary>
    public PrivilegeMode PrivilegeMode { get; set; }
}
