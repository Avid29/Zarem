// Avishai Dernis 2024

using Zarem.Emulator.Machine.CPU.Registers;
using Zarem.Emulator.Machine.Enums;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine.CPU.CoProcessors;

/// <summary>
/// A class representing the 0th co-processor unit.
/// </summary>
public class CoProcessor0
{
    private const uint NORMAL_EXCEPTION_VECTOR = 0x8000_0180;
    private const uint BOOT_STRAPPING_EXCEPTION_VECTOR = 0xBFC0_0180;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoProcessor0"/> class.
    /// </summary>
    public CoProcessor0()
    {
        RegisterFile = new RegisterFile();
    }

    internal RegisterFile RegisterFile { get; }

    /// <summary>
    /// Gets the processor's current privilege mode.
    /// </summary>
    /// <remarks>
    /// This is not neccesarily the same as the <see cref="StatusRegister.PrivilegeMode"/>.
    /// If the processor is in <see cref="StatusRegister.ErrorLevel"/> or <see cref="StatusRegister.ExceptionLevel"/>, the privilege mode is always kernel, regardless of the value of <see cref="StatusRegister.PrivilegeMode"/>.
    /// </remarks>
    public PrivilegeMode PrivilegeMode
        => StatusRegister.ErrorLevel || StatusRegister.ExceptionLevel
        ? PrivilegeMode.Kernel
        : StatusRegister.PrivilegeMode;

    /// <summary>
    /// Gets the current exception vector.
    /// </summary>
    public uint ExceptionVector => StatusRegister.BootStrapping
        ? BOOT_STRAPPING_EXCEPTION_VECTOR
        : NORMAL_EXCEPTION_VECTOR;

    /// <summary>
    /// Gets or sets the status register.
    /// </summary>
    public StatusRegister StatusRegister
    {
        get => (StatusRegister)RegisterFile[CP0Registers.Status];
        set => RegisterFile[CP0Registers.Status] = (uint)value;
    }

    /// <summary>
    /// Gets or sets the status register.
    /// </summary>
    public CauseRegister CauseRegister
    {
        get => (CauseRegister)RegisterFile[CP0Registers.Cause];
        set => RegisterFile[CP0Registers.Cause] = (uint)value;
    }

    /// <summary>
    /// Gets or sets the value of a register on the coprocessor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    public uint this[CP0Registers reg]
    {
        get => RegisterFile[reg];
        set => RegisterFile[reg] = value;
    }
}
