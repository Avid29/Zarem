// Avishai Dernis 2025

using Zarem.Attributes.Register;

namespace Zarem.RiscV.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum for register sets.
/// </summary>
public enum RiscVRegisterSet
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
    [RegisterSet("x{0}")] Numbered = None,

#pragma warning disable CS1591

    [RegisterSet("x{0}", typeof(RiscVGpRegister), @"^x([0-9]+)$")] GeneralPurpose,
    [RegisterSet("f{0}", typeof(RiscVFloatRegister), @"^f([0-9]+)$")] FloatingPoints,

#pragma warning restore CS1591
}
