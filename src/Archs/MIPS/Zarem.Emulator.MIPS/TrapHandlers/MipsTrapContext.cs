// Avishai Dernis 2026

using Zarem.Emulator.Machine;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// A struct for the context of a 
/// </summary>
public readonly ref struct MipsTrapContext
{
    internal MipsTrapContext(MipsComputer computer, ulong trapCode)
    {
        Computer = computer;
        TrapCode = trapCode;
    }

    /// <summary>
    /// Gets the computer that trapped.
    /// </summary>
    public MipsComputer Computer { get; }

    /// <summary>
    /// Gets the trap code.
    /// </summary>
    public ulong TrapCode { get; }

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    public uint A0 => Computer.Processor[GPRegister.Argument0];

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    public uint A1 => Computer.Processor[GPRegister.Argument1];

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    public uint A2 => Computer.Processor[GPRegister.Argument2];

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    public uint A3 => Computer.Processor[GPRegister.Argument3];

    /// <summary>
    /// Gets or sets the value of first return value register.
    /// </summary>
    public uint V0
    {
        get => Computer.Processor[GPRegister.ReturnValue0];
        set => Computer.Processor[GPRegister.ReturnValue0] = value;
    }
}
