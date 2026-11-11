// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum for functions code stored in the RS2 field of RISC-V instructions.
/// </summary>
public enum FunctRS2Code : byte
{
#pragma warning disable CS1591

    CountLeadingZeros = 0x00,
    CountTrailingZeros = 0x01,
    PopulationCount = 0x02,
    SignExtendByte = 0x04,
    SignExtendHalfword = 0x05,

    OrcB = 0x07,
    Rev8_32 = 0x18,

#pragma warning restore CS1591
}
