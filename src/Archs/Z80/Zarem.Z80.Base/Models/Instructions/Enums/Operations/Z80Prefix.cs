// Avishai Dernis 2026

namespace Zarem.Z80.Models.Instructions.Enums.Operations;

/// <summary>
/// A prefix for a Z80 instruction.
/// </summary>
public enum Z80Prefix : ushort
{
#pragma warning disable CS1591

    None = 0x00,

    // Single byte prefixes
    BitManipulationPage = 0xCB,
    IndexRegisterXModifier = 0xDD,
    ExtendedInstructionPage = 0xED,
    IndexRegisterYModifier = 0xFD,

    // Double byte prefixes
    IndexXBitManipulationPage = 0xDDCB,
    IndexYBitManipulationPage = 0xFDCB,

#pragma warning restore CS1591
}
