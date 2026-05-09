// Avishai Dernis 2026

using System.Collections.Generic;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Debugger.Viewer;
using Zarem.Emulator.Machine.Registers;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Debugger.Viewer;

/// <summary>
/// A class wrapping an <see cref="IRegisterFile"/> as an <see cref="IRegisterGroup"/>.
/// </summary>
public class MipsRegisterViewer : IRegisterGroup
{
    private readonly IRegisterFile _registers;
    private readonly MipsRegisterSet _set;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsRegisterViewer"/> class.
    /// </summary>
    public MipsRegisterViewer(IRegisterFile registerFile, MipsRegisterSet set)
    {
        _registers = registerFile;
        _set = set;
    }

    /// <inheritdoc/>
    public ulong? this[string registerName]
    {
        get => MipsRegisterTable.Instance.TryGetRegister(registerName, out var reg, out var set, out _) && set == _set ? _registers[(int)reg] : null;
        set
        {
            if (value.HasValue && MipsRegisterTable.Instance.TryGetRegister(registerName, out var reg, out var set, out _) && set == _set)
                _registers[(int)reg] = (uint)value.Value;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> RegisterNames
    {
        get
        {
            for (var i = 0; i < _registers.Count; i++)
            {
                yield return MipsRegisterTable.Instance.GetRegisterString((MipsGpRegister)i, MipsRegisterSet.GeneralPurpose);
            }
        }
    }
}
