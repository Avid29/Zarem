// Avishai Dernis 2024

using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.SpecialFunctions;
using Zarem.Models.Instructions.Enums.SpecialFunctions.CoProc0;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;

namespace Zarem.Helpers.Instructions;

/// <summary>
/// A static class containing helper methods related to instruction types.
/// </summary>
public static class InstructionTypeHelper
{
    /// <summary>
    /// Gets the <see cref="InstructionType"/> of an <see cref="MipsInstruction"/>.
    /// </summary>
    /// <param name="opCode">The instruction to get the type of.</param>
    /// <param name="rtFuncCode">The rtFunction of the instruction.</param>
    /// <param name="rsFuncCode">The rsFuncCode of the instruction.</param>
    /// <returns>The <see cref="InstructionType"/> associated to an <see cref="MipsInstruction"/>.</returns>
    public static InstructionType GetInstructionType(OperationCode? opCode, RegImmFuncCode? rtFuncCode = null, CoProc0RSCode? rsFuncCode = null)
    {
        if (!opCode.HasValue)
            return InstructionType.Pseudo;

        return opCode switch
        {
            // R Type 
            OperationCode.Special => InstructionType.BasicR,
            OperationCode.Special2 => InstructionType.Special2R,
            OperationCode.Special3 => InstructionType.Special3R,

            OperationCode.RegisterImmediate
                when rtFuncCode is <= RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely or
                >= RegImmFuncCode.BranchOnLessThanZeroAndLink => InstructionType.RegisterImmediateBranch,
            OperationCode.RegisterImmediate => InstructionType.RegisterImmediate,
            
            // J Type
            OperationCode.Jump or OperationCode.JumpAndLink or
            OperationCode.JumpAndLinkX => InstructionType.BasicJ,
            
            // CoProc0
            OperationCode.Coprocessor0 => InstructionType.Coproc0,

            // CoProc1
            OperationCode.Coprocessor1
                => (CoProc1RSCode?)rsFuncCode switch
                {
                    null or (>= CoProc1RSCode.Single and <= CoProc1RSCode.PairedSingle) => InstructionType.Float,
                    _ => InstructionType.Coproc1,
                },

            // I Type is the default
            _ => InstructionType.BasicI,
        };
    }

    /// <summary>
    /// Gets the <see cref="InstructionPattern"/> of an <see cref="MipsInstruction"/>.
    /// </summary>
    /// <param name="opCode">The instruction to get the type of.</param>
    /// <returns>The <see cref="InstructionPattern"/> associated to an <see cref="MipsInstruction"/>.</returns>
    public static InstructionPattern GetInstructionPattern(OperationCode? opCode)
    {
        if (!opCode.HasValue)
            return InstructionPattern.Other;

        return opCode switch
        {
            OperationCode.Special => InstructionPattern.R,
            OperationCode.RegisterImmediate => InstructionPattern.R,
            OperationCode.Special2 => InstructionPattern.R,
            OperationCode.Special3 => InstructionPattern.R,
            OperationCode.Jump or
            OperationCode.JumpAndLink => InstructionPattern.J,
            OperationCode.Coprocessor0 => InstructionPattern.Other,
            _ => InstructionPattern.I,
        };
    }
}
