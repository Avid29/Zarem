// Avishai Dernis 2024

using CommunityToolkit.Diagnostics;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Models.Instructions;

/// <summary>
/// A struct representing a pseudo instruction.
/// </summary>
public readonly struct PseudoInstruction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PseudoInstruction"/> struct.
    /// </summary>
    /// <param name="op"></param>
    public PseudoInstruction(PseudoOp op)
    {
        PseudoOp = op;
    }

    /// <summary>
    /// Gets or sets the psudo operation code
    /// </summary>
    public PseudoOp PseudoOp { get; init; }

    /// <summary>
    /// Gets or sets the pseudo-instruction rs register.
    /// </summary>
    public GPRegister RS { get; init; }

    /// <summary>
    /// Gets or sets the pseudo-instruction rt register.
    /// </summary>
    public GPRegister RT { get; init; }

    /// <summary>
    /// Gets or sets the pseudo-instruction rd register.
    /// </summary>
    public GPRegister RD { get; init; }

    /// <summary>
    /// Gets or sets the pseudo-instruction immediate value.
    /// </summary>
    public int Immediate { get; init; }

    /// <summary>
    /// Gets or sets the pseudo-instruction address.
    /// </summary>
    public uint Address { get; init; }

    /// <summary>
    /// Expands the pseudo-instruction into an array of real instructions.
    /// </summary>
    public readonly MipsInstruction[] Expand()
    {
        return PseudoOp switch
        {
            PseudoOp.NoOperation =>
            [
                // nop: sll zero, zero, 0
                MipsInstruction.Create(FunctionCode.ShiftLeftLogical, GPRegister.Zero, GPRegister.Zero, GPRegister.Zero, 0),
            ],
            PseudoOp.SuperScalarNoOperation =>
            [
                // ssnop: sll zero, zero, 1
                MipsInstruction.Create(FunctionCode.ShiftLeftLogical, GPRegister.Zero, GPRegister.Zero, GPRegister.Zero, 1),
            ],
            PseudoOp.UnconditionalBranch =>
            [
                // b offset: beq zero, zero, offset
                MipsInstruction.Create(OperationCode.BranchOnEquals, GPRegister.Zero, GPRegister.Zero, (short)Immediate),
            ],
            PseudoOp.BranchOnLessThan =>
            [
                // blt rs, rt, offset: slt at, rs, rt; bne at, zero, offset
                MipsInstruction.Create(FunctionCode.SetLessThan, RS, RT, GPRegister.AssemblerTemporary),
                MipsInstruction.Create(OperationCode.BranchOnNotEquals, GPRegister.AssemblerTemporary, GPRegister.Zero, (short)Immediate)
            ],
            PseudoOp.LoadImmediate =>
            [
                // li rt, imm: lui rt, upper; ori rt, rt, lower
                MipsInstruction.Create(OperationCode.LoadUpperImmediate, RT, (short)(Immediate >> 16)),
                MipsInstruction.Create(OperationCode.OrImmediate, RT, RT, (short)Immediate)
            ],
            PseudoOp.AbsoluteValue =>
            [
                // abs rd, rs: 
                // 1. move rd, rs 
                // 2. bgez rs, 8 (skip next) 
                // 3. sub rd, zero, rs
                MipsInstruction.Create(FunctionCode.AddUnsigned, RS, GPRegister.Zero, RD),
                MipsInstruction.Create(RegImmFuncCode.BranchOnGreaterThanOrEqualToZero, RS, 2), // Offset is usually instruction count (2 instructions)
                MipsInstruction.Create(FunctionCode.Subtract, GPRegister.Zero, RS, RD),
            ],
            PseudoOp.Move =>
            [
                // move rd, rs: addu rd, rs, zero
                MipsInstruction.Create(FunctionCode.AddUnsigned, RS, GPRegister.Zero, RD),
            ],
            PseudoOp.LoadAddress =>
            [
                // la rd, addr: lui rd, upper; ori rd, rd, lower
                MipsInstruction.Create(OperationCode.LoadUpperImmediate, RD, (short)(Immediate >> 16)),
                MipsInstruction.Create(OperationCode.OrImmediate, RD, RD, (short)Immediate)
            ],
            PseudoOp.SetGreaterThanOrEqual =>
            [
                // sge rd, rs, rt: slt rd, rs, rt; xori rd, rd, 1
                MipsInstruction.Create(FunctionCode.SetLessThan, RS, RT, RD),
                MipsInstruction.Create(OperationCode.ExclusiveOrImmediate, RD, RD, (short)1),
            ],
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<MipsInstruction[]>(),
        };
    }
}
