// Avishai Dernis 2025

using System;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine.Registers;

/// <summary>
/// A class representing a register file.
/// </summary>
public class MipsRegisterFile
{
    private readonly uint[] _registers;

    /// <summary>
    /// An event invoked when a register is changed.
    /// </summary>
    public event EventHandler<GPRegister>? RegisterChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsRegisterFile"/> class.
    /// </summary>
    public MipsRegisterFile(RegisterSet set, int count = 32)
    {
        RegisterSet = set;
        _registers = new uint[count];
    }

    /// <summary>
    /// Gets the register set type.
    /// </summary>
    public RegisterSet RegisterSet { get; }

    /// <summary>
    /// Gets the number of registers in the register file.
    /// </summary>
    public int Count => _registers.Length;

    /// <summary>
    /// Gets or sets the value in a register.
    /// </summary>
    public uint this[int register]
    {
        get => _registers[register];
        set
        {
            // Cannot set the 0 GPR register
            if (register is 0 && RegisterSet == RegisterSet.GeneralPurpose)
                return;

            // Register is out of the indexable bounds. Do nothing.
            if (register < 0 || register >= _registers.Length)
                return;

            _registers[register] = value;
            RegisterChanged?.Invoke(this, (GPRegister)register);
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
