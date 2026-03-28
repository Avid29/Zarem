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
    public ulong A0 => Cpu[(int)GPRegister.Argument0];

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    public ulong A1 => Cpu[(int)GPRegister.Argument1];

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    public ulong A2 => Cpu[(int)GPRegister.Argument2];

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    public ulong A3 => Cpu[(int)GPRegister.Argument3];

    /// <summary>
    /// Gets or sets the value of first return value register.
    /// </summary>
    public ulong V0
    {
        get => Cpu[(int)GPRegister.ReturnValue0];
        set => Cpu[(int)GPRegister.ReturnValue0] = value;
    }
}
