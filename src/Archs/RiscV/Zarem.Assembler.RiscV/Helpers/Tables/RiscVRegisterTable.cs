// Avishai Dernis 2025

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Zarem.Assembler.Models;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Assembler.Helpers.Tables;

/// <summary>
/// A class containing a lookup table for RISC-V registers.
/// </summary>
public partial class RiscVRegisterTable : RegisterTable<GPRegister, RegisterSet>
{
    private static readonly Lazy<RiscVRegisterTable> _instance = new();

    private static Dictionary<string, GPRegister> GPRegisterTable { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        { "zero", GPRegister.Zero }, { "ra", GPRegister.ReturnAddress },
        { "sp", GPRegister.StackPointer }, { "gp", GPRegister.GlobalPointer },
        { "tp", GPRegister.ThreadPointer }, { "t0", GPRegister.Temporary0 },
        { "t1", GPRegister.Temporary1 }, { "t2", GPRegister.Temporary2 },
        { "s0", GPRegister.Saved0 }, { "s1", GPRegister.Saved1 },
        { "a0", GPRegister.Argument0 }, { "a1", GPRegister.Argument1 },
        { "a2", GPRegister.Argument2 }, { "a3", GPRegister.Argument3 },
        { "a4", GPRegister.Argument4 }, { "a5", GPRegister.Argument5 },
        { "a6", GPRegister.Argument6 }, { "a7", GPRegister.Argument7 },
        { "s2", GPRegister.Saved2 }, { "s3", GPRegister.Saved3 },
        { "s4", GPRegister.Saved4 }, { "s5", GPRegister.Saved5 },
        { "s6", GPRegister.Saved6 }, { "s7", GPRegister.Saved7 },
        { "s8", GPRegister.Saved8 }, { "s9", GPRegister.Saved9 },
        { "s10", GPRegister.Saved10 }, { "s11", GPRegister.Saved11 },
        { "t3", GPRegister.Temporary3 }, { "t4", GPRegister.Temporary4 },
        { "t5", GPRegister.Temporary5 }, { "t6", GPRegister.Temporary6 },
        { "fp", GPRegister.FramePointer },
    };

    private static Dictionary<string, FloatRegister> FloatRegisterTable { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ft0", FloatRegister.Temporary0 }, { "ft1", FloatRegister.Temporary1 },
        { "ft2", FloatRegister.Temporary2 }, { "ft3", FloatRegister.Temporary3 },
        { "ft4", FloatRegister.Temporary4 }, { "ft5", FloatRegister.Temporary5 },
        { "ft6", FloatRegister.Temporary6 }, { "ft7", FloatRegister.Temporary7 },
        { "fs0", FloatRegister.Saved0 }, { "fs1", FloatRegister.Saved1 },
        { "fa0", FloatRegister.Argument0 }, { "fa1", FloatRegister.Argument1 },
        { "fa2", FloatRegister.Argument2 }, { "fa3", FloatRegister.Argument3 },
        { "fa4", FloatRegister.Argument4 }, { "fa5", FloatRegister.Argument5 },
        { "fa6", FloatRegister.Argument6 }, { "fa7", FloatRegister.Argument7 },
        { "fs2", FloatRegister.Saved2 }, { "fs3", FloatRegister.Saved3 },
        { "fs4", FloatRegister.Saved4 }, { "fs5", FloatRegister.Saved5 },
        { "fs6", FloatRegister.Saved6 }, { "fs7", FloatRegister.Saved7 },
        { "fs8", FloatRegister.Saved8 }, { "fs9", FloatRegister.Saved9 },
        { "fs10", FloatRegister.Saved10 }, { "fs11", FloatRegister.Saved11 },
        { "ft8", FloatRegister.Temporary8 }, { "ft9", FloatRegister.Temporary9 },
        { "ft10", FloatRegister.Temporary10 }, { "ft11", FloatRegister.Temporary11 },
    };

    [GeneratedRegex(@"^x([0-9]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetGPRegisterRegex();

    [GeneratedRegex(@"^f([0-9]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetFloatRegisterRegex();

    /// <summary>
    /// Gets an instance of the <see cref="RiscVRegisterTable"/>.
    /// </summary>
    public static RiscVRegisterTable Instance { get; } = _instance.Value;

    /// <inheritdoc/>
    protected override Dictionary<RegisterSet, Dictionary<string, GPRegister>> NamedRegisterTables { get; } = new()
    {
        { RegisterSet.GeneralPurpose, GPRegisterTable },
        { RegisterSet.FloatingPoints, FloatRegisterTable.ToDictionary(x => x.Key, x => (GPRegister)x.Value) },
    };
    
    /// <inheritdoc/>
    protected override Dictionary<RegisterSet, Regex> NumericalSetRegexTable { get; } = new()
    {
        { RegisterSet.GeneralPurpose, GetGPRegisterRegex() },
        { RegisterSet.FloatingPoints, GetFloatRegisterRegex() }
    };

    /// <inheritdoc/>
    protected override Dictionary<RegisterSet, string> NumericalSetFormatTable { get; } = new()
    {
        { RegisterSet.GeneralPurpose, "x{0}" },
        { RegisterSet.FloatingPoints, "f{0}" },
        { RegisterSet.Numbered, "x{0}" }
    };
}
