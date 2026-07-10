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

    private static Dictionary<string, RiscVGpRegister> GPRegisterTable { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        { "zero", RiscVGpRegister.Zero }, { "ra", RiscVGpRegister.ReturnAddress },
        { "sp", RiscVGpRegister.StackPointer }, { "gp", RiscVGpRegister.GlobalPointer },
        { "tp", RiscVGpRegister.ThreadPointer }, { "t0", RiscVGpRegister.Temporary0 },
        { "t1", RiscVGpRegister.Temporary1 }, { "t2", RiscVGpRegister.Temporary2 },
        { "s0", RiscVGpRegister.Saved0 }, { "s1", RiscVGpRegister.Saved1 },
        { "a0", RiscVGpRegister.Argument0 }, { "a1", RiscVGpRegister.Argument1 },
        { "a2", RiscVGpRegister.Argument2 }, { "a3", RiscVGpRegister.Argument3 },
        { "a4", RiscVGpRegister.Argument4 }, { "a5", RiscVGpRegister.Argument5 },
        { "a6", RiscVGpRegister.Argument6 }, { "a7", RiscVGpRegister.Argument7 },
        { "s2", RiscVGpRegister.Saved2 }, { "s3", RiscVGpRegister.Saved3 },
        { "s4", RiscVGpRegister.Saved4 }, { "s5", RiscVGpRegister.Saved5 },
        { "s6", RiscVGpRegister.Saved6 }, { "s7", RiscVGpRegister.Saved7 },
        { "s8", RiscVGpRegister.Saved8 }, { "s9", RiscVGpRegister.Saved9 },
        { "s10", RiscVGpRegister.Saved10 }, { "s11", RiscVGpRegister.Saved11 },
        { "t3", RiscVGpRegister.Temporary3 }, { "t4", RiscVGpRegister.Temporary4 },
        { "t5", RiscVGpRegister.Temporary5 }, { "t6", RiscVGpRegister.Temporary6 },
        { "fp", RiscVGpRegister.FramePointer },
    };

    private static Dictionary<string, RiscVFloatRegister> FloatRegisterTable { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ft0", RiscVFloatRegister.Temporary0 }, { "ft1", RiscVFloatRegister.Temporary1 },
        { "ft2", RiscVFloatRegister.Temporary2 }, { "ft3", RiscVFloatRegister.Temporary3 },
        { "ft4", RiscVFloatRegister.Temporary4 }, { "ft5", RiscVFloatRegister.Temporary5 },
        { "ft6", RiscVFloatRegister.Temporary6 }, { "ft7", RiscVFloatRegister.Temporary7 },
        { "fs0", RiscVFloatRegister.Saved0 }, { "fs1", RiscVFloatRegister.Saved1 },
        { "fa0", RiscVFloatRegister.Argument0 }, { "fa1", RiscVFloatRegister.Argument1 },
        { "fa2", RiscVFloatRegister.Argument2 }, { "fa3", RiscVFloatRegister.Argument3 },
        { "fa4", RiscVFloatRegister.Argument4 }, { "fa5", RiscVFloatRegister.Argument5 },
        { "fa6", RiscVFloatRegister.Argument6 }, { "fa7", RiscVFloatRegister.Argument7 },
        { "fs2", RiscVFloatRegister.Saved2 }, { "fs3", RiscVFloatRegister.Saved3 },
        { "fs4", RiscVFloatRegister.Saved4 }, { "fs5", RiscVFloatRegister.Saved5 },
        { "fs6", RiscVFloatRegister.Saved6 }, { "fs7", RiscVFloatRegister.Saved7 },
        { "fs8", RiscVFloatRegister.Saved8 }, { "fs9", RiscVFloatRegister.Saved9 },
        { "fs10", RiscVFloatRegister.Saved10 }, { "fs11", RiscVFloatRegister.Saved11 },
        { "ft8", RiscVFloatRegister.Temporary8 }, { "ft9", RiscVFloatRegister.Temporary9 },
        { "ft10", RiscVFloatRegister.Temporary10 }, { "ft11", RiscVFloatRegister.Temporary11 },
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
    protected override Dictionary<RiscVRegisterSet, Dictionary<string, RiscVGpRegister>> NamedRegisterTables { get; } = new()
    {
        { RiscVRegisterSet.GeneralPurpose, GPRegisterTable },
        { RiscVRegisterSet.FloatingPoints, FloatRegisterTable.ToDictionary(x => x.Key, x => (RiscVGpRegister)x.Value) },
    };
    
    /// <inheritdoc/>
    protected override Dictionary<RiscVRegisterSet, Regex> NumericalSetRegexTable { get; } = new()
    {
        { RiscVRegisterSet.GeneralPurpose, GetGPRegisterRegex() },
        { RiscVRegisterSet.FloatingPoints, GetFloatRegisterRegex() }
    };

    /// <inheritdoc/>
    protected override Dictionary<RiscVRegisterSet, string> NumericalSetFormatTable { get; } = new()
    {
        { RiscVRegisterSet.GeneralPurpose, "x{0}" },
        { RiscVRegisterSet.FloatingPoints, "f{0}" },
        { RiscVRegisterSet.Numbered, "x{0}" }
    };

    /// <inheritdoc/>
    protected override Dictionary<RiscVRegisterSet, Dictionary<RiscVGpRegister, RiscVRegisterCategory>> RegisterCategoryTable { get; } = new()
    {
        {
            RiscVRegisterSet.GeneralPurpose, new Dictionary<RiscVGpRegister, RiscVRegisterCategory>
            {
                { RiscVGpRegister.Zero, RiscVRegisterCategory.Special },
                { RiscVGpRegister.ReturnAddress, RiscVRegisterCategory.Special },
                { RiscVGpRegister.StackPointer, RiscVRegisterCategory.Special },
                { RiscVGpRegister.GlobalPointer, RiscVRegisterCategory.Special },
                { RiscVGpRegister.ThreadPointer, RiscVRegisterCategory.Special },
                { RiscVGpRegister.Temporary0, RiscVRegisterCategory.Temporary },
                { RiscVGpRegister.Temporary1, RiscVRegisterCategory.Temporary },
                { RiscVGpRegister.Temporary2, RiscVRegisterCategory.Temporary },
                { RiscVGpRegister.Saved0, RiscVRegisterCategory.Saved },
                { RiscVGpRegister.Saved1, RiscVRegisterCategory.Saved },
                { RiscVGpRegister.Argument0, RiscVRegisterCategory.Argument },
                { RiscVGpRegister.Argument1, RiscVRegisterCategory.Argument },
                { RiscVGpRegister.Argument2, RiscVRegisterCategory.Argument },
                { RiscVGpRegister.Argument3, RiscVRegisterCategory.Argument },
                { RiscVGpRegister.Argument4, RiscVRegisterCategory.Argument },
                { RiscVGpRegister.Argument5, RiscVRegisterCategory.Argument },
                { RiscVGpRegister.Argument6, RiscVRegisterCategory.Argument },
                { RiscVGpRegister.Argument7, RiscVRegisterCategory.Argument },
                { RiscVGpRegister.Saved2, RiscVRegisterCategory.Saved },
                { RiscVGpRegister.Saved3, RiscVRegisterCategory.Saved },
                { RiscVGpRegister.Saved4, RiscVRegisterCategory.Saved },
                { RiscVGpRegister.Saved5, RiscVRegisterCategory.Saved },
                { RiscVGpRegister.Saved6, RiscVRegisterCategory.Saved },
                { RiscVGpRegister.Saved7, RiscVRegisterCategory.Saved },
                { RiscVGpRegister.Saved8, RiscVRegisterCategory.Saved },
                { RiscVGpRegister.Saved9, RiscVRegisterCategory.Saved },
                { RiscVGpRegister.Saved10, RiscVRegisterCategory.Saved },
                { RiscVGpRegister.Saved11, RiscVRegisterCategory.Saved },
                { RiscVGpRegister.Temporary3, RiscVRegisterCategory.Temporary },
                { RiscVGpRegister.Temporary4, RiscVRegisterCategory.Temporary },
                { RiscVGpRegister.Temporary5, RiscVRegisterCategory.Temporary },
                { RiscVGpRegister.Temporary6, RiscVRegisterCategory.Temporary },
            }
        },
        {
            RiscVRegisterSet.FloatingPoints, new Dictionary<RiscVGpRegister, RiscVRegisterCategory>
            {
                { (RiscVGpRegister)RiscVFloatRegister.Temporary0, RiscVRegisterCategory.Temporary },
                { (RiscVGpRegister)RiscVFloatRegister.Temporary1, RiscVRegisterCategory.Temporary },
                { (RiscVGpRegister)RiscVFloatRegister.Temporary2, RiscVRegisterCategory.Temporary },
                { (RiscVGpRegister)RiscVFloatRegister.Temporary3, RiscVRegisterCategory.Temporary },
                { (RiscVGpRegister)RiscVFloatRegister.Temporary4, RiscVRegisterCategory.Temporary },
                { (RiscVGpRegister)RiscVFloatRegister.Temporary5, RiscVRegisterCategory.Temporary },
                { (RiscVGpRegister)RiscVFloatRegister.Temporary6, RiscVRegisterCategory.Temporary },
                { (RiscVGpRegister)RiscVFloatRegister.Temporary7, RiscVRegisterCategory.Temporary },
                { (RiscVGpRegister)RiscVFloatRegister.Saved0, RiscVRegisterCategory.Saved },
                { (RiscVGpRegister)RiscVFloatRegister.Saved1, RiscVRegisterCategory.Saved },
                { (RiscVGpRegister)RiscVFloatRegister.Argument0, RiscVRegisterCategory.Argument },
                { (RiscVGpRegister)RiscVFloatRegister.Argument1, RiscVRegisterCategory.Argument },
                { (RiscVGpRegister)RiscVFloatRegister.Argument2, RiscVRegisterCategory.Argument },
                { (RiscVGpRegister)RiscVFloatRegister.Argument3, RiscVRegisterCategory.Argument },
                { (RiscVGpRegister)RiscVFloatRegister.Argument4, RiscVRegisterCategory.Argument },
                { (RiscVGpRegister)RiscVFloatRegister.Argument5, RiscVRegisterCategory.Argument },
                { (RiscVGpRegister)RiscVFloatRegister.Argument6, RiscVRegisterCategory.Argument },
                { (RiscVGpRegister)RiscVFloatRegister.Argument7, RiscVRegisterCategory.Argument },
                { (RiscVGpRegister)RiscVFloatRegister.Saved2, RiscVRegisterCategory.Saved },
                { (RiscVGpRegister)RiscVFloatRegister.Saved3, RiscVRegisterCategory.Saved },
                { (RiscVGpRegister)RiscVFloatRegister.Saved4, RiscVRegisterCategory.Saved },
                { (RiscVGpRegister)RiscVFloatRegister.Saved5, RiscVRegisterCategory.Saved },
                { (RiscVGpRegister)RiscVFloatRegister.Saved6, RiscVRegisterCategory.Saved },
                { (RiscVGpRegister)RiscVFloatRegister.Saved7, RiscVRegisterCategory.Saved },
                { (RiscVGpRegister)RiscVFloatRegister.Saved8, RiscVRegisterCategory.Saved },
                { (RiscVGpRegister)RiscVFloatRegister.Saved9, RiscVRegisterCategory.Saved },
                { (RiscVGpRegister)RiscVFloatRegister.Saved10, RiscVRegisterCategory.Saved },
                { (RiscVGpRegister)RiscVFloatRegister.Saved11, RiscVRegisterCategory.Saved },
                { (RiscVGpRegister)RiscVFloatRegister.Temporary8, RiscVRegisterCategory.Temporary},
                { (RiscVGpRegister)RiscVFloatRegister.Temporary9, RiscVRegisterCategory.Temporary},
                { (RiscVGpRegister)RiscVFloatRegister.Temporary10, RiscVRegisterCategory.Temporary},
                { (RiscVGpRegister)RiscVFloatRegister.Temporary11, RiscVRegisterCategory.Temporary},
            }
        }
    };
}
