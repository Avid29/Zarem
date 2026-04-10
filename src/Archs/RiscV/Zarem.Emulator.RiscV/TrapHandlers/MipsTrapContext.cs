// Avishai Dernis 2026

using System;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Models.Enums;
using Zarem.Emulator.TrapHandlers.Interfaces;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// An <see cref="ITrapContext"/> for the mips architecture, which provides access to the relevant registers for trap handling. This is used to abstract the trap context from the architecture, so that the trap handlers can be implemented in a more generic way.
/// </summary>
public readonly struct RiscVTrapContext : ITrapContext
{
    internal RiscVTrapContext(IRiscVCpu cpu, ulong trapCode)
    {
        Cpu = cpu;
        TrapCode = trapCode;
    }

    /// <inheritdoc/>
    public IRiscVCpu Cpu { get; }

    /// <inheritdoc/>
    ICpu ITrapContext.Cpu => Cpu;

    /// <inheritdoc/>
    public ulong TrapCode { get; }

    /// <inheritdoc/>
    public ulong SyscallId => Cpu[RiscVGpRegister.Argument7];

    /// <inheritdoc/>
    public bool IsSyscall => (RiscVTrap)TrapCode is RiscVTrap.EnvironmentCallFromUMode or RiscVTrap.EnvironmentCallFromMMode or RiscVTrap.EnvironmentCallFromSMode;

    /// <inheritdoc/>
    public ulong Argument0 => Cpu[RiscVGpRegister.Argument0];

    /// <inheritdoc/>
    public ulong Argument1 => Cpu[RiscVGpRegister.Argument1];

    /// <inheritdoc/>
    public ulong Argument2 => Cpu[RiscVGpRegister.Argument2];

    /// <inheritdoc/>
    public float FloatArgument0 => throw new NotImplementedException();

    /// <inheritdoc/>
    public double DoubleArgument0 => throw new NotImplementedException();

    /// <inheritdoc/>
    public ulong Result0
    {
        get => Cpu[RiscVGpRegister.Argument0];
        set => Cpu[RiscVGpRegister.Argument0] = value;
    }

    /// <inheritdoc/>
    public ulong Result1
    {
        get => Cpu[RiscVGpRegister.Argument0];
        set => Cpu[RiscVGpRegister.Argument0] = value;
    }

    /// <inheritdoc/>
    public float FloatResult0
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public double DoubleResult0
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}
