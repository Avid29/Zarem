// Avishai Dernis 2024

using System.Numerics;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Registers;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine.CoProcessors;

/// <summary>
/// A class representing the status/control coprocessor unit.
/// </summary>
public class CoProcessor0<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private const uint NORMAL_EXCEPTION_VECTOR = 0x8000_0180;
    private const uint BOOT_STRAPPING_EXCEPTION_VECTOR = 0xBFC0_0180;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoProcessor0{T}"/> class.
    /// </summary>
    public CoProcessor0()
    {
        RegisterFile = new(32);
    }

    /// <summary>
    /// Gets the coprocessor0's register file.
    /// </summary>
    internal RegisterFile<T> RegisterFile { get; }

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
    public T ExceptionVector => T.CreateTruncating(StatusRegister.BootStrapping
        ? BOOT_STRAPPING_EXCEPTION_VECTOR
        : NORMAL_EXCEPTION_VECTOR);

    /// <summary>
    /// Gets or sets the status register.
    /// </summary>
    public StatusRegister StatusRegister
    {
        get => (StatusRegister)uint.CreateTruncating(RegisterFile[(int)CP0Registers.Status]);
        set => RegisterFile[(int)CP0Registers.Status] = T.CreateTruncating((uint)value);
    }

    /// <summary>
    /// Gets or sets the status register.
    /// </summary>
    public CauseRegister CauseRegister
    {
        get => (CauseRegister)uint.CreateTruncating(RegisterFile[(int)CP0Registers.Cause]);
        set => RegisterFile[(int)CP0Registers.Cause] = T.CreateTruncating((uint)value);
    }

    /// <summary>
    /// Gets or sets the value of a register on the coprocessor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    public T this[CP0Registers reg]
    {
        get => RegisterFile[(int)reg];
        set => RegisterFile[(int)reg] = value;
    }

    /// <summary>
    /// Handles entering a trap.
    /// </summary>
    public void EnterTrap(MipsTrap trap, T programCounter, bool isDelaySlot)
    {
        // Don't overwrite EPC if we are already in an exception (nested exception logic)
        if (!StatusRegister.ExceptionLevel)
        {
            this[CP0Registers.ExceptionPC] = isDelaySlot
                ? programCounter - T.CreateTruncating(4)
                : programCounter;

            StatusRegister = StatusRegister with { ExceptionLevel = true };
        }

        CauseRegister = CauseRegister with
        {
            ExecptionCode = trap,
            IsBranchDelayed = isDelaySlot,
        };
    }
}
