// Avishai Dernis 2026

using System.Collections.Generic;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Debugger.Models;
using Zarem.Debugger.Viewer;
using Zarem.Emulator.Machine.Registers;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Debugger.Viewer;

/// <summary>
/// A class wrapping an <see cref="IRegisterFile"/> as an <see cref="IRegisterViewer"/>.
/// </summary>
public class RiscVRegisterViewer : IRegisterViewer
{
    private readonly IRegisterFile _registers;
    private readonly RiscVRegisterSet _set;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVRegisterViewer"/> class.
    /// </summary>
    public RiscVRegisterViewer(IRegisterFile registerFile, RiscVRegisterSet set)
    {
        _registers = registerFile;
        _set = set;
    }

    /// <inheritdoc/>
    public ulong? this[string registerName]
    {
        get => RiscVRegisterTable.Instance.TryGetRegister(registerName, out var reg, out var set, out _) && set == _set ? _registers[(int)reg] : null;
        set
        {
            if (value.HasValue && RiscVRegisterTable.Instance.TryGetRegister(registerName, out var reg, out var set, out _) && set == _set)
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
                var name = RiscVRegisterTable.Instance.GetRegisterString((RiscVGpRegister)i, RiscVRegisterSet.GeneralPurpose);
                var category = RiscVRegisterTable.Instance.GetRegisterCategory((RiscVGpRegister)i, RiscVRegisterSet.GeneralPurpose);

                // TODO: Localize category

                yield return new RegisterMeta(name, $"{category}");
            }
        }
    }
}
