// Avishai Dernis 2025

using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine.Registers;

/// <summary>
/// A class representing a register file.
/// </summary>
public class RegisterFile
{
    private readonly uint[] _registers;
    private readonly bool _gpr;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterFile"/> class.
    /// </summary>
    public RegisterFile(bool gpr = false, int count = 32)
    {
        _gpr = gpr;
        _registers = new uint[count];
    }
    
    /// <summary>
    /// Gets or sets the value in a register.
    /// </summary>
    public uint this[int register]
    {
        get => _registers[register];
        set
        {
            // Cannot set the 0 GPR register
            if (register is 0 && _gpr)
                return;

            // Register is out of the indexable bounds. Do nothing.
            if (register < 0 || register >= _registers.Length)
                return;

            _registers[register] = value;
        }
    }

    /// <summary>
    /// Gets or sets the value in a register.
    /// </summary>
    public uint this[GPRegister register]
    {
        get => this[(int)register];
        set => this[(int)register] = value;
    }

    /// <inheritdoc cref="this[GPRegister]"/>
    public uint this[CP0Registers register]
    {
        get => this[(int)register];
        set => this[(int)register] = value;
    }

    /// <inheritdoc cref="this[GPRegister]"/>
    public uint this[FloatRegister register]
    {
        get => this[(int)register];
        set => this[(int)register] = value;
    }
}
