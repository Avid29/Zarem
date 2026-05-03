// Avishai Dernis 2024

using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Functions;
using Zarem.Models.Instructions.Enums.Functions.CoProc0;
using Zarem.Models.Instructions.Enums.Functions.FloatProc;

namespace Zarem.Helpers.Instructions;

/// <summary>
/// A static class containing helper methods related to instruction types.
/// </summary>
public static class InstructionTypeHelper
{
    /// <summary>
    /// Gets the <see cref="MipsInstructionType"/> of an <see cref="MipsInstruction"/>.
    /// </summary>
    /// <param name="opCode">The instruction to get the type of.</param>
    /// <param name="rtFuncCode">The rtFunction of the instruction.</param>
    /// <param name="rsFuncCode">The rsFuncCode of the instruction.</param>
    /// <returns>The <see cref="MipsInstructionType"/> associated to an <see cref="MipsInstruction"/>.</returns>
    public static MipsInstructionType GetInstructionType(MipsOpCode? opCode, RegImmFuncCode? rtFuncCode = null, CoProc0RSCode? rsFuncCode = null)
    {
        if (!opCode.HasValue)
            return MipsInstructionType.Pseudo;

        return opCode switch
        {
            // R Type 
            MipsOpCode.Special => MipsInstructionType.BasicR,
            MipsOpCode.Special2 => MipsInstructionType.Special2R,
            MipsOpCode.Special3 => MipsInstructionType.Special3R,

            MipsOpCode.RegisterImmediate
                when rtFuncCode is <= RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely or
                >= RegImmFuncCode.BranchOnLessThanZeroAndLink => MipsInstructionType.RegisterImmediateBranch,
            MipsOpCode.RegisterImmediate => MipsInstructionType.RegisterImmediateTrap,
            
            // J Type
            MipsOpCode.Jump or MipsOpCode.JumpAndLink or
            MipsOpCode.JumpAndLinkX => MipsInstructionType.BasicJ,
            
            // CoProc0
            MipsOpCode.Coprocessor0 => MipsInstructionType.Coproc0,

            // CoProc1
            MipsOpCode.Coprocessor1
                => (CoProc1RSCode?)rsFuncCode switch
                {
                    null or (>= CoProc1RSCode.Single and <= CoProc1RSCode.PairedSingle) => MipsInstructionType.Float,
                    _ => MipsInstructionType.Coproc1,
                },

            // I Type is the default
            _ => MipsInstructionType.BasicI,
        };
    }

    /// <summary>
    /// Gets the <see cref="MipsInstructionPattern"/> of an <see cref="MipsInstruction"/>.
    /// </summary>
    /// <param name="opCode">The instruction to get the type of.</param>
    /// <returns>The <see cref="MipsInstructionPattern"/> associated to an <see cref="MipsInstruction"/>.</returns>
    public static MipsInstructionPattern GetInstructionPattern(MipsOpCode? opCode)
    {
        if (!opCode.HasValue)
            return MipsInstructionPattern.Other;

        return opCode switch
        {
            MipsOpCode.Special => MipsInstructionPattern.R,
            MipsOpCode.RegisterImmediate => MipsInstructionPattern.R,
            MipsOpCode.Special2 => MipsInstructionPattern.R,
            MipsOpCode.Special3 => MipsInstructionPattern.R,
            MipsOpCode.Jump or
            MipsOpCode.JumpAndLink => MipsInstructionPattern.J,
            MipsOpCode.Coprocessor0 => MipsInstructionPattern.Other,
            _ => MipsInstructionPattern.I,
        };
    }
}
