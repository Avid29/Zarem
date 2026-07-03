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

    /// <inheritdoc/>
    protected override Dictionary<MipsRegisterSet, Dictionary<MipsGpRegister, MipsRegisterCategory>> RegisterCategoryTable { get; } = new()
    {
        {
            MipsRegisterSet.GeneralPurpose, new Dictionary<MipsGpRegister, MipsRegisterCategory>
            {
                { MipsGpRegister.Zero, MipsRegisterCategory.Special },
                { MipsGpRegister.AssemblerTemporary, MipsRegisterCategory.Special },
                { MipsGpRegister.ReturnValue0, MipsRegisterCategory.ReturnValue },
                { MipsGpRegister.ReturnValue1, MipsRegisterCategory.ReturnValue },
                { MipsGpRegister.Argument0, MipsRegisterCategory.Argument },
                { MipsGpRegister.Argument1, MipsRegisterCategory.Argument },
                { MipsGpRegister.Argument2, MipsRegisterCategory.Argument },
                { MipsGpRegister.Argument3, MipsRegisterCategory.Argument },
                { MipsGpRegister.Temporary0, MipsRegisterCategory.Temporary },
                { MipsGpRegister.Temporary1, MipsRegisterCategory.Temporary },
                { MipsGpRegister.Temporary2, MipsRegisterCategory.Temporary },
                { MipsGpRegister.Temporary3, MipsRegisterCategory.Temporary },
                { MipsGpRegister.Temporary4, MipsRegisterCategory.Temporary },
                { MipsGpRegister.Temporary5, MipsRegisterCategory.Temporary },
                { MipsGpRegister.Temporary6, MipsRegisterCategory.Temporary },
                { MipsGpRegister.Temporary7, MipsRegisterCategory.Temporary },
                { MipsGpRegister.Saved0, MipsRegisterCategory.Saved },
                { MipsGpRegister.Saved1, MipsRegisterCategory.Saved },
                { MipsGpRegister.Saved2, MipsRegisterCategory.Saved },
                { MipsGpRegister.Saved3, MipsRegisterCategory.Saved },
                { MipsGpRegister.Saved4, MipsRegisterCategory.Saved },
                { MipsGpRegister.Saved5, MipsRegisterCategory.Saved },
                { MipsGpRegister.Saved6, MipsRegisterCategory.Saved },
                { MipsGpRegister.Saved7, MipsRegisterCategory.Saved },
                { MipsGpRegister.Temporary8, MipsRegisterCategory.Temporary },
                { MipsGpRegister.Temporary9, MipsRegisterCategory.Temporary },
                { MipsGpRegister.Kernel0, MipsRegisterCategory.Kernel },
                { MipsGpRegister.Kernel1, MipsRegisterCategory.Kernel },
                { MipsGpRegister.GlobalPointer, MipsRegisterCategory.Special },
                { MipsGpRegister.StackPointer, MipsRegisterCategory.Special },
                { MipsGpRegister.FramePointer, MipsRegisterCategory.Special },
                { MipsGpRegister.ReturnAddress, MipsRegisterCategory.Special },
                { MipsGpRegister.High, MipsRegisterCategory.HighLow },
                { MipsGpRegister.Low, MipsRegisterCategory.HighLow },
            }
        },
        {
            MipsRegisterSet.FloatingPoints, new Dictionary<MipsGpRegister, MipsRegisterCategory>
            {
                { (MipsGpRegister)MipsFloatRegister.F0, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F1, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F2, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F3, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F4, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F5, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F6, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F7, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F8, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F9, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F10, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F11, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F12, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F13, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F14, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F15, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F16, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F17, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F18, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F19, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F20, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F21, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F22, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F23, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F24, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F25, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F26, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F27, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F28, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F29, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F30, MipsRegisterCategory.Temporary },
                { (MipsGpRegister)MipsFloatRegister.F31, MipsRegisterCategory.Temporary },
            }
        }
    };
}
