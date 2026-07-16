// Avishai Dernis 2025

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Zarem.Assembler.Models.Tables;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Assembler.Helpers.Tables;

/// <summary>
/// A class containing a lookup table for MIPS registers.
/// </summary>
public partial class MipsRegisterTable : RegisterTable<MipsGpRegister, MipsRegisterSet, MipsRegisterCategory>
{
    private static readonly Lazy<MipsRegisterTable> _instance = new();

    /// <summary>
    /// Gets an instance of the <see cref="MipsRegisterTable"/>.
    /// </summary>
    public static MipsRegisterTable Instance { get; } = _instance.Value;
}
