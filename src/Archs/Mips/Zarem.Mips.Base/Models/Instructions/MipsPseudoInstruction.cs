// Avishai Dernis 2024

using CommunityToolkit.Diagnostics;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Models.Instructions;

/// <summary>
/// A struct representing a pseudo instruction.
/// </summary>
public readonly struct MipsPseudoInstruction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsPseudoInstruction"/> struct.
    /// </summary>
    /// <param name="op"></param>
    public MipsPseudoInstruction(MipsPseudoOp op)
    {
        PseudoOp = op;
    }

    /// <summary>
    /// Gets or sets the psudo operation code
    /// </summary>
    public MipsPseudoOp PseudoOp { get; init; }

    /// <summary>
    /// Gets or sets the pseudo-instruction rs register.
    /// </summary>
    public MipsGpRegister RS { get; init; }

    /// <summary>
    /// Gets or sets the pseudo-instruction rt register.
    /// </summary>
    public MipsGpRegister RT { get; init; }

    /// <summary>
    /// Gets or sets the pseudo-instruction rd register.
    /// </summary>
    public MipsGpRegister RD { get; init; }

    /// <summary>
    /// Gets or sets the pseudo-instruction immediate value.
    /// </summary>
    public int Immediate { get; init; }

    /// <summary>
    /// Expands the pseudo-instruction into an array of real instructions.
    /// </summary>
    public readonly MipsInstruction[] Expand()
    {
        return PseudoOp switch
        {
            MipsPseudoOp.NoOperation =>
            [
                // nop: sll zero, zero, 0
                MipsInstruction.CreateR(FunctionCode.ShiftLeftLogical, MipsGpRegister.Zero, MipsGpRegister.Zero, MipsGpRegister.Zero),
            ],
            MipsPseudoOp.SuperScalarNoOperation =>
            [
                // ssnop: sll zero, zero, 1
                MipsInstruction.CreateR(FunctionCode.ShiftLeftLogical, MipsGpRegister.Zero, MipsGpRegister.Zero, MipsGpRegister.Zero, 1),
            ],
            MipsPseudoOp.UnconditionalBranch =>
            [
                // b offset: beq zero, zero, offset
                MipsInstruction.CreateBranch(MipsOpCode.BranchOnEquals, MipsGpRegister.Zero, MipsGpRegister.Zero, Immediate),
            ],
            MipsPseudoOp.BranchOnLessThan =>
            [
                // blt rs, rt, offset: slt at, rs, rt; bne at, zero, offset
                MipsInstruction.CreateR(FunctionCode.SetLessThan, RS, RT, MipsGpRegister.AssemblerTemporary),
                MipsInstruction.CreateBranch(MipsOpCode.BranchOnNotEquals, MipsGpRegister.AssemblerTemporary, MipsGpRegister.Zero, Immediate)
            ],
            MipsPseudoOp.LoadImmediate =>
            [
                // li rd, imm: lui rd, upper; ori rd, rd, lower
                MipsInstruction.CreateI(MipsOpCode.LoadUpperImmediate, MipsGpRegister.Zero, RT, (short)(Immediate >> 16)),
                MipsInstruction.CreateI(MipsOpCode.OrImmediate, RT, RT, (short)Immediate)
            ],
            MipsPseudoOp.AbsoluteValue =>
            [
                // abs rd, rs: 
                // 1. move rd, rs 
                // 2. bgez rs, 8 (skip next) 
                // 3. sub rd, zero, rs
                MipsInstruction.CreateR(FunctionCode.AddUnsigned, RS, MipsGpRegister.Zero, RD),
                MipsInstruction.CreateBranch(RegImmFuncCode.BranchOnGreaterThanOrEqualToZero, RS, 2), // Offset is usually instruction count (2 instructions)
                MipsInstruction.CreateR(FunctionCode.Subtract, MipsGpRegister.Zero, RS, RD),
            ],
            MipsPseudoOp.Move =>
            [
                // move rd, rs: addu rd, rs, zero
                MipsInstruction.CreateR(FunctionCode.AddUnsigned, RS, MipsGpRegister.Zero, RD),
            ],
            MipsPseudoOp.LoadAddress =>
            [
                // la rd, addr: lui rd, upper; ori rd, rd, lower
                MipsInstruction.CreateI(MipsOpCode.LoadUpperImmediate, MipsGpRegister.Zero, RD, (short)(Immediate >> 16)),
                MipsInstruction.CreateI(MipsOpCode.OrImmediate, RD, RD, (short)Immediate)
            ],
            MipsPseudoOp.SetGreaterThanOrEqual =>
            [
                // sge rd, rs, rt: slt rd, rs, rt; xori rd, rd, 1
                MipsInstruction.CreateR(FunctionCode.SetLessThan, RS, RT, RD),
                MipsInstruction.CreateI(MipsOpCode.ExclusiveOrImmediate, RD, RD, 1),
            ],
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<MipsInstruction[]>(),
        };
    }
}
