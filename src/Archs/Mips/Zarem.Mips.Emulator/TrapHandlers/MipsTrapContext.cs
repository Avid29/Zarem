// Avishai Dernis 2026

using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.TrapHandlers.Interfaces;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// An <see cref="ITrapContext"/> for the mips architecture, which provides access to the relevant registers for trap handling. This is used to abstract the trap context from the architecture, so that the trap handlers can be implemented in a more generic way.
/// </summary>
public readonly struct MipsTrapContext : ITrapContext
{
    internal MipsTrapContext(IMipsCpu cpu, ulong trapCode)
    {
        Cpu = cpu;
        TrapCode = trapCode;
    }

    /// <inheritdoc/>
    public IMipsCpu Cpu { get; }

    /// <inheritdoc/>
    ICpu ITrapContext.Cpu => Cpu;

    /// <inheritdoc/>
    public ulong TrapCode { get; }

    /// <inheritdoc/>
    public ulong SyscallId => Result0;

    /// <inheritdoc/>
    public bool IsSyscall => ((MipsTrap)TrapCode) is MipsTrap.Syscall;

    /// <inheritdoc/>
    public ulong Argument0 => Cpu[MipsGpRegister.Argument0];

    /// <inheritdoc/>
    public ulong Argument1 => Cpu[MipsGpRegister.Argument1];

    /// <inheritdoc/>
    public ulong Argument2 => Cpu[MipsGpRegister.Argument2];

    /// <inheritdoc/>
    public float FloatArgument0 => Cpu.FloatProcessor.Singles[MipsFloatRegister.F12];

    /// <inheritdoc/>
    public double DoubleArgument0 => Cpu.FloatProcessor.Doubles[MipsFloatRegister.F12];

    /// <inheritdoc/>
    public ulong Result0
    {
        get => Cpu[MipsGpRegister.ReturnValue0];
        set => Cpu[MipsGpRegister.ReturnValue0] = value;
    }

    /// <inheritdoc/>
    public ulong Result1
    {
        get => Cpu[MipsGpRegister.ReturnValue1];
        set => Cpu[MipsGpRegister.ReturnValue1] = value;
    }

    /// <inheritdoc/>
    public float FloatResult0
    {
        get => Cpu.FloatProcessor.Singles[MipsFloatRegister.F0];
        set => Cpu.FloatProcessor.Singles[MipsFloatRegister.F0] = value;
    }

    /// <inheritdoc/>
    public double DoubleResult0
    {
        get => Cpu.FloatProcessor.Doubles[MipsFloatRegister.F0];
        set => Cpu.FloatProcessor.Doubles[MipsFloatRegister.F0] = value;
    }
}
