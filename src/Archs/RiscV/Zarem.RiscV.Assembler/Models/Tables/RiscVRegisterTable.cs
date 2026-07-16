// Avishai Dernis 2025

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Zarem.Assembler.Models.Tables;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.Assembler.Helpers.Tables;

/// <summary>
/// A class containing a lookup table for RISC-V registers.
/// </summary>
public partial class RiscVRegisterTable : RegisterTable<RiscVGpRegister, RiscVRegisterSet, RiscVRegisterCategory>
{
    private static readonly Lazy<RiscVRegisterTable> _instance = new();

    /// <summary>
    /// Gets an instance of the <see cref="RiscVRegisterTable"/>.
    /// </summary>
    public static RiscVRegisterTable Instance { get; } = _instance.Value;
}
