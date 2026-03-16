// Avishai Dernis 2026

using System.Collections.Generic;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Emulator.Machine.Registers;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Debugger.Viewer;

/// <summary>
/// A class wrapping a <see cref="MipsRegisterFile"/> as an <see cref="IRegisterGroup"/>.
/// </summary>
public class MipsRegisterViewer : IRegisterGroup
{
    private MipsRegisterFile _registers;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsRegisterViewer"/> class.
    /// </summary>
    public MipsRegisterViewer(MipsRegisterFile registerFile)
    {
        _registers = registerFile;
    }

    /// <inheritdoc/>
    public ulong? this[string registerName]
    {
        get => RegistersTable.TryGetRegister(registerName, out var reg, out var set) && set == _registers.RegisterSet ? _registers[reg] : null;
        set
        {
            if (value.HasValue && RegistersTable.TryGetRegister(registerName, out var reg, out var set) && set == _registers.RegisterSet)
                _registers[reg] = (uint)value.Value;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> RegisterNames
    {
        get
        {
            for (var i = 0; i < _registers.Count; i++)
            {
                yield return RegistersTable.GetRegisterString((GPRegister)i, _registers.RegisterSet);
            }
        }
    }
}
