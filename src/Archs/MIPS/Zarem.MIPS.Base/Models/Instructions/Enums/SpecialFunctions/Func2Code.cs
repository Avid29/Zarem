// Avishai Dernis 2024

using Zarem.Models.Instructions.Enums.Operations;

namespace Zarem.Models.Instructions.Enums.SpecialFunctions;

/// <summary>
/// An enum for <see cref="OperationCode.Special2"/> instruction function codes.
/// </summary>
public enum Func2Code
{
    /// <summary>
    /// Marks that there is no function2 code.
    /// </summary>
    /// <remarks>
    /// This value is too large to encode in a real instruction. If by accident this were encoded into
    /// an <see cref="MIPSInstruction"/> struct, it would become <see cref="MultiplyAndAddHiLow"/>.
    /// </remarks>
    None = 0x40,

#pragma warning disable CS1591

    MultiplyAndAddHiLow = 0x0,
    MultiplyAndAddHiLowUnsigned = 0x1,

    MultiplyToGPR = 0x2,

    MultiplyAndSubtractHiLow = 0x4,
    MultiplyAndSubtractHiLowUnsigned = 0x5,

    CountLeadingZeros = 0x20,
    CountLeadingOnes = 0x21,

    SoftwareDebugBreakpoint = 0x3f,

#pragma warning restore CS1591
}
