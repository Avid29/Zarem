// Avishai Dernis 2024

using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Models.Instructions.Enums.SpecialFunctions;

/// <summary>
/// These values go in the <see cref="Argument.RT"/> field of instructions with <see cref="OperationCode.RegisterImmediate"/>.
/// </summary>
public enum RegImmFuncCode : byte
{
    /// <summary>
    /// Marks that there is no function code.
    /// </summary>
    /// <remarks>
    /// This value is too large to encode in a real instruction. If by accident
    /// this were encoded into an <see cref="MIPSInstruction"/> struct, it would become 
    /// <see cref="BranchOnLessThanZero"/> (or <see cref="GPRegister.Zero"/>) upon unencoding.
    /// </remarks>
    None = 0x20,

#pragma warning disable CS1591

    BranchOnLessThanZero = 0x00,
    BranchOnGreaterThanOrEqualToZero = 0x01,
    BranchOnLessThanZeroLikely = 0x02,
    BranchOnGreaterThanOrEqualToZeroLikely = 0x03,

    TrapOnGreaterOrEqualImmediate = 0x08,
    TrapOnGreaterOrEqualImmediateUnisigned = 0x09,
    TrapOnLessThanImmediate = 0x0a,
    TrapOnLessThanImmediateUnsigned = 0x0b,
    TrapOnEqualsImmediate = 0x0c,
    TrapOnNotEqualsImmediate = 0x0e,

    BranchOnLessThanZeroAndLink = 0x10,
    BranchOnGreaterThanOrEqualToZeroAndLink = 0x11,
    BranchOnLessThanZeroLikelyAndLink = 0x12,
    BranchOnGreaterThanOrEqualToZeroLikelyAndLink = 0x13,

    // MIPS Revision 6
    NoOpAndLink = 0x10,
    BranchAndLink = 0x11

#pragma warning restore CS1591
}
