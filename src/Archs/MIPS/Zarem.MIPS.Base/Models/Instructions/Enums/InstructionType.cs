// Avishai Dernis 2024

namespace Zarem.Models.Instructions.Enums;

/// <summary>
/// An enum for the mips instruction types.
/// </summary>
public enum InstructionType
{
#pragma warning disable CS1591

    BasicR,
    BasicI,
    IBranch,
    BasicJ,
    RegisterImmediateBranch,
    RegisterImmediateTrap,
    Special2R,
    Special3R,
    Coproc0,
    Coproc1,
    Float,
    Pseudo,

#pragma warning restore CS1591
}
