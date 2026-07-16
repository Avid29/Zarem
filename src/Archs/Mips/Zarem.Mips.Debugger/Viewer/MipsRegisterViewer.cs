// Avishai Dernis 2026

using System.Collections.Generic;
using Zarem.Assembler.Models.Tables;
using Zarem.Debugger.Models;
using Zarem.Debugger.Viewer;
using Zarem.Emulator.Machine.Registers;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Debugger.Viewer;

/// <summary>
/// A class wrapping an <see cref="IRegisterFile"/> as an <see cref="IRegisterViewer"/>.
/// </summary>
public class MipsRegisterViewer : IRegisterViewer
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
        get => RegisterTable<MipsGpRegister, MipsRegisterSet>.TryGetRegister(registerName, out var reg, out var set, out _) && set == _set ? _registers[(int)reg] : null;
        set
        {
            if (value.HasValue && RegisterTable<MipsGpRegister, MipsRegisterSet>.TryGetRegister(registerName, out var reg, out var set, out _) && set == _set)
                _registers[(int)reg] = (uint)value.Value;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<RegisterMeta> Registers
    {
        get
        {
            for (var i = 0; i < _registers.Count; i++)
            {
                var name = RegisterTable<MipsGpRegister, MipsRegisterSet>.GetRegisterString((MipsGpRegister)i, MipsRegisterSet.GeneralPurpose);
                var category = RegisterTable<MipsGpRegister, MipsRegisterSet, MipsRegisterCategory>.GetRegisterCategory((MipsGpRegister)i, MipsRegisterSet.GeneralPurpose);

                // TODO: Localize category

                yield return new RegisterMeta(name, $"{category}");
            }
        }
    }
}
