// Avishai Dernis 2026

using Zarem.Emulator.Machine;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.TrapHandlers.Base;

/// <summary>
/// A struct for the context of a 
/// </summary>
public readonly struct MipsTrapContext
{
    internal MipsTrapContext(MipsCpu cpu, ulong trapCode)
    {
        Cpu = cpu;
        TrapCode = trapCode;
    }

    /// <summary>
    /// Gets the cpu that trapped.
    /// </summary>
    public MipsCpu Cpu { get; }

    /// <summary>
    /// Gets the trap code.
    /// </summary>
    public ulong TrapCode { get; }

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    public uint A0 => Cpu[GPRegister.Argument0];

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    public uint A1 => Cpu[GPRegister.Argument1];

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    public uint A2 => Cpu[GPRegister.Argument2];

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    public uint A3 => Cpu[GPRegister.Argument3];

    /// <summary>
    /// Gets or sets the value of first return value register.
    /// </summary>
    public uint V0
    {
        get => Cpu[GPRegister.ReturnValue0];
        set => Cpu[GPRegister.ReturnValue0] = value;
    }
}
