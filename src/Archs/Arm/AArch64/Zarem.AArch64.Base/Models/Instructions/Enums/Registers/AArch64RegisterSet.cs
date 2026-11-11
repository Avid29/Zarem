// Avishai Dernis 2026

using Zarem.Attributes.Register;

namespace Zarem.AArch64.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum for AArch64 register sets.
/// </summary>
public enum AArch64RegisterSet
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

    [RegisterSet("{0}", typeof(AArch64GpRegister), @"^(?:x([0-9]+))$")] GeneralPurpose64,
    [RegisterSet("w{0}", typeof(AArch64GpRegister), @"^(?:w([0-9]+)$")] GeneralPurpose32,

#pragma warning restore CS1591
}
