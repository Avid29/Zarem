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
public partial class MipsRegisterTable : RegisterTable<MipsGpRegister, MipsRegisterSet>
{
    private static readonly Lazy<MipsRegisterTable> _instance = new();

    private static Dictionary<string, MipsGpRegister> GpRegisterTable { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        { "zero", MipsGpRegister.Zero }, { "at", MipsGpRegister.AssemblerTemporary },
        { "v0", MipsGpRegister.ReturnValue0 }, { "v1", MipsGpRegister.ReturnValue1 },
        { "a0", MipsGpRegister.Argument0 }, { "a1", MipsGpRegister.Argument1 },
        { "a2", MipsGpRegister.Argument2 }, { "a3", MipsGpRegister.Argument3 },
        { "t0", MipsGpRegister.Temporary0 }, { "t1", MipsGpRegister.Temporary1 },
        { "t2", MipsGpRegister.Temporary2 }, { "t3", MipsGpRegister.Temporary3 },
        { "t4", MipsGpRegister.Temporary4 }, { "t5", MipsGpRegister.Temporary5 },
        { "t6", MipsGpRegister.Temporary6 }, { "t7", MipsGpRegister.Temporary7 },
        { "s0", MipsGpRegister.Saved0 }, { "s1", MipsGpRegister.Saved1 },
        { "s2", MipsGpRegister.Saved2 }, { "s3", MipsGpRegister.Saved3 },
        { "s4", MipsGpRegister.Saved4 }, { "s5", MipsGpRegister.Saved5 },
        { "s6", MipsGpRegister.Saved6 }, { "s7", MipsGpRegister.Saved7 },
        { "t8", MipsGpRegister.Temporary8 }, { "t9", MipsGpRegister.Temporary9 },
        { "k0", MipsGpRegister.Kernel0 }, { "k1", MipsGpRegister.Kernel1 },
        { "gp", MipsGpRegister.GlobalPointer }, { "sp", MipsGpRegister.StackPointer },
        { "fp", MipsGpRegister.FramePointer }, { "ra", MipsGpRegister.ReturnAddress },
        { "hi", MipsGpRegister.High }, { "lo", MipsGpRegister.Low },
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
    protected override Dictionary<MipsRegisterSet, Dictionary<string, MipsGpRegister>> NamedRegisterTables { get; } = new()
    {
        { MipsRegisterSet.GeneralPurpose, GpRegisterTable }
    };

    /// <inheritdoc/>
    protected override Dictionary<MipsRegisterSet, Regex> NumericalSetRegexTable { get; } = new()
    {
        { MipsRegisterSet.Numbered, GetGPRegisterRegex() },
        { MipsRegisterSet.GeneralPurpose, GetGPRegisterRegex() },
        { MipsRegisterSet.FloatingPoints, GetFloatRegisterRegex() }
    };

    /// <inheritdoc/>
    protected override Dictionary<MipsRegisterSet, string> NumericalSetFormatTable { get; } = new()
    {
        { MipsRegisterSet.Numbered, "{0}" },
        { MipsRegisterSet.GeneralPurpose, "{0}" },
        { MipsRegisterSet.FloatingPoints, "f{0}" }
    };
}
