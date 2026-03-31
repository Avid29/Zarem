// Avishai Dernis 2025

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Zarem.Assembler.Models;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Assembler.Helpers.Tables;

/// <summary>
/// A class containing a lookup table for MIPS registers.
/// </summary>
public partial class MipsRegisterTable : RegisterTable<GPRegister, RegisterSet>
{
    private static readonly Lazy<MipsRegisterTable> _instance = new();

    private static Dictionary<string, GPRegister> GpRegisterTable { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        { "zero", GPRegister.Zero }, { "at", GPRegister.AssemblerTemporary },
        { "v0", GPRegister.ReturnValue0 }, { "v1", GPRegister.ReturnValue1 },
        { "a0", GPRegister.Argument0 }, { "a1", GPRegister.Argument1 },
        { "a2", GPRegister.Argument2 }, { "a3", GPRegister.Argument3 },
        { "t0", GPRegister.Temporary0 }, { "t1", GPRegister.Temporary1 },
        { "t2", GPRegister.Temporary2 }, { "t3", GPRegister.Temporary3 },
        { "t4", GPRegister.Temporary4 }, { "t5", GPRegister.Temporary5 },
        { "t6", GPRegister.Temporary6 }, { "t7", GPRegister.Temporary7 },
        { "s0", GPRegister.Saved0 }, { "s1", GPRegister.Saved1 },
        { "s2", GPRegister.Saved2 }, { "s3", GPRegister.Saved3 },
        { "s4", GPRegister.Saved4 }, { "s5", GPRegister.Saved5 },
        { "s6", GPRegister.Saved6 }, { "s7", GPRegister.Saved7 },
        { "t8", GPRegister.Temporary8 }, { "t9", GPRegister.Temporary9 },
        { "k0", GPRegister.Kernel0 }, { "k1", GPRegister.Kernel1 },
        { "gp", GPRegister.GlobalPointer }, { "sp", GPRegister.StackPointer },
        { "fp", GPRegister.FramePointer }, { "ra", GPRegister.ReturnAddress },
        { "hi", GPRegister.High }, { "lo", GPRegister.Low },
    };

    [GeneratedRegex(@"^\$?([0-9]+)$", RegexOptions.Compiled)]
    private static partial Regex GetGPRegisterRegex();

    [GeneratedRegex(@"^\$?f([0-9]+)$", RegexOptions.Compiled)]
    private static partial Regex GetFloatRegisterRegex();

    /// <summary>
    /// Gets an instance of the <see cref="MipsRegisterTable"/>.
    /// </summary>
    public static MipsRegisterTable Instance { get; } = _instance.Value;

    /// <inheritdoc/>
    protected override Dictionary<RegisterSet, Dictionary<string, GPRegister>> NamedRegisterTables { get; } = new()
    {
        { RegisterSet.GeneralPurpose, GpRegisterTable }
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
        { RegisterSet.GeneralPurpose, "{0}" },
        { RegisterSet.FloatingPoints, "f{0}" }
    };
}
