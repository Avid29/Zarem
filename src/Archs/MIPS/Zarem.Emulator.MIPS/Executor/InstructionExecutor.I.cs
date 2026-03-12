// Avishai Dernis 2026

using System;
using Zarem.Emulator.Executor.Enum;
using Zarem.Models.Instructions.Enums.Operations;

namespace Zarem.Emulator.Executor;

/// <summary>
/// A class which handles converting decoded instructions into <see cref="Execution"/> models.
/// </summary>
public partial class InstructionExecutor
{
    private Execution CreateITypeExecution()
    {
        return Instruction.OpCode switch
        {
            // Branch
            OperationCode.BranchOnEquals or
            OperationCode.BranchOnEqualLikely => Branch((rs, rt) => rs == rt),
            OperationCode.BranchOnNotEquals or
            OperationCode.BranchOnNotEqualLikely => Branch((rs, rt) => rs != rt),
            OperationCode.BranchOnLessThanOrEqualToZero or
            OperationCode.BranchOnLessThanOrEqualToZeroLikely => Branch((rs, _) => (int)rs <= 0),
            OperationCode.BranchOnGreaterThanZero or
            OperationCode.BranchOnGreaterThanZeroLikely => Branch((rs, _) => (int)rs > 0),

            // Arithmetic
            OperationCode.AddImmediate => BasicI(
                (rs, imm) => (uint)((int)rs + imm),
                (a, b, r) => ((a ^ r) & (b ^ r)) < 0),
            OperationCode.AddImmediateUnsigned => BasicI((rs, imm) => rs + (ushort)imm),

            // Compare
            OperationCode.SetLessThanImmediate => BasicI((rs, imm) => (uint)((int)rs < imm ? 1 : 0)),
            OperationCode.SetLessThanImmediateUnsigned => BasicI((rs, imm) => (uint)(rs < imm ? 1 : 0)),

            // Logical
            OperationCode.AndImmediate => BasicI((rs, imm) => rs & (ushort)imm),
            OperationCode.OrImmediate => BasicI((rs, imm) => rs | (ushort)imm),
            OperationCode.ExclusiveOrImmediate => BasicI((rs, imm) => rs ^ (ushort)imm),

            OperationCode.LoadUpperImmediate => BasicI((rs, imm) => (uint)((ushort)imm << 16)),

            _ => throw new NotImplementedException()
        };
    }

    private Execution BasicI(BasicIDelegate func, OverflowCheckDelegate? checkFunc = null)
    {
        // Determine the destination register and
        // compute the result using the provided function
        var dest = Instruction.RT;
        uint value = func(RS, Instruction.ImmediateValue);

        // Check for overflow if a check function is provided
        // Return a trap execution if overflow is detected
        if (checkFunc is not null &&
            checkFunc((int)RS, Instruction.ImmediateValue, (int)value))
        {
            return CreateTrap(MipsTrap.ArithmeticOverflow);
        }

        // No overflow detected
        // Return the execution with the computed value and destination
        return Execution.CreateWriteback(dest, value);
    }
}
