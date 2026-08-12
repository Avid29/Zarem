// Avishai Dernis 2025

using Zarem.Attributes.Register;

namespace Zarem.Mips.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum for register sets.
/// </summary>
public enum MipsRegisterSet
{
    /// <summary>
    /// Do not writeback to any register from any register set.
    /// </summary>
    /// <remarks>
    /// This has an equivelant value to <see cref="Numbered"/>, except that is used in the assembler.
    /// </remarks>
    None,

    /// <summary>
    /// The register is encoding as a number, and the instruction does not specify which register set it belongs to.
    /// </summary>
    /// <remarks>
    /// This has an equivelant value to <see cref="None"/>, except that is used in the interpreter.
    /// </remarks>
    [RegisterSet("{0}")] Numbered = None,

#pragma warning disable CS1591

    [RegisterSet("{0}", 32, typeof(MipsGpRegister), @"^\$?([0-9]+)$")] GeneralPurpose,
    [RegisterSet("f{0}", 32, typeof(MipsFloatRegister), @"^\$?f([0-9]+)$")] FloatingPoints,
    
    CoProc0,
    FCSR,

#pragma warning restore CS1591
}
