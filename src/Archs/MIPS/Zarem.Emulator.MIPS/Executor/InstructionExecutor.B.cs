// Avishai Dernis 2026

using System;
using Zarem.Emulator.Executor.Enum;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Emulator.Executor;

/// <summary>
/// A class which handles converting decoded instructions into <see cref="Execution"/> models.
/// </summary>
public partial class InstructionExecutor
{
    private Execution CreateRegImmExecution()
    {
        return Instruction.RTFuncCode switch
        {
            // Branch
            RegImmFuncCode.BranchOnLessThanZero or
            RegImmFuncCode.BranchOnLessThanZeroLikely => Branch((rs, _) => (int)rs < 0),
            RegImmFuncCode.BranchOnGreaterThanOrEqualToZero or
            RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely => Branch((rs, _) => (int)rs >= 0),

            // Trap
            RegImmFuncCode.TrapOnGreaterOrEqualImmediate => TrapI((rs, imm) => (int)rs >= imm),
            RegImmFuncCode.TrapOnGreaterOrEqualImmediateUnisigned => TrapI((rs, imm) => rs >= (ushort)imm),
            RegImmFuncCode.TrapOnLessThanImmediate => TrapI((rs, imm) => (int)rs < imm),
            RegImmFuncCode.TrapOnLessThanImmediateUnsigned => TrapI((rs, imm) => rs < (ushort)imm),
            RegImmFuncCode.TrapOnEqualsImmediate => TrapI((rs, imm) => rs == imm),
            RegImmFuncCode.TrapOnNotEqualsImmediate => TrapI((rs, imm) => rs != imm),

            _ => throw new NotImplementedException()
        };
    }

    private Execution Branch(BranchDelegate func)
    {
        if (!func(RS, RT))
        {
            return default;
        }

        return Execution.CreateBranch(Instruction.Offset);
    }

    private Execution TrapI(TrapIDelegate func)
    {
        if (func(RS, Instruction.ImmediateValue))
            Trap = MIPSTrap.Trap;

        return default;
    }
}
