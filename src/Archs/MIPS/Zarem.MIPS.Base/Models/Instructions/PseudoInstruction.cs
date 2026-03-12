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
                MipsInstruction.Create(FunctionCode.ShiftLeftLogical, GPRegister.Zero, GPRegister.Zero, GPRegister.Zero, 0),
            ],
            PseudoOp.SuperScalarNoOperation =>
            [
                MipsInstruction.Create(FunctionCode.ShiftLeftLogical, GPRegister.Zero, GPRegister.Zero, GPRegister.Zero, 1),
            ],
            PseudoOp.UnconditionalBranch =>
            [
                MipsInstruction.Create(OperationCode.BranchOnEquals, GPRegister.Zero, GPRegister.Zero, (short)Immediate),
            ],
            PseudoOp.BranchOnLessThan =>
            [
                MipsInstruction.Create(FunctionCode.SetLessThan, RS, RT, GPRegister.AssemblerTemporary),
                MipsInstruction.Create(OperationCode.BranchOnNotEquals, GPRegister.AssemblerTemporary, GPRegister.Zero, (short)Immediate)
            ],
            PseudoOp.LoadImmediate =>
            [
                MipsInstruction.Create(OperationCode.LoadUpperImmediate, RT, (short)(Immediate >> 16)),
                MipsInstruction.Create(OperationCode.OrImmediate, RT, RT, (short)Immediate)
            ],
            PseudoOp.AbsoluteValue =>
            [
                MipsInstruction.Create(FunctionCode.AddUnsigned, RS, GPRegister.Zero, RT),
                MipsInstruction.Create(RegImmFuncCode.BranchOnGreaterThanOrEqualToZero, RS, 8),
                MipsInstruction.Create(FunctionCode.Subtract, GPRegister.Zero, RS, RT),
            ],
            PseudoOp.Move =>
            [
                MipsInstruction.Create(FunctionCode.Add, RS, GPRegister.Zero, RT),
            ],
            PseudoOp.LoadAddress =>
            [
                MipsInstruction.Create(OperationCode.LoadUpperImmediate, RT, (short)(Immediate >> 16)),
                MipsInstruction.Create(OperationCode.OrImmediate, RT, RT, (short)Immediate)
            ],
            PseudoOp.SetGreaterThanOrEqual =>
            [
                MipsInstruction.Create(OperationCode.AddImmediateUnsigned, RT, RT, (short)-1),
                MipsInstruction.Create(FunctionCode.SetLessThan, RS, RT, RD),
            ],
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<MipsInstruction[]>(),
        };
    }

    /// <summary>
    /// Gets the number of real instructions required to implement the peudo instruction.
    /// </summary>
    public readonly int RealInstructionCount =>
        PseudoOp switch
        {
            PseudoOp.Move => 1,
            PseudoOp.BranchOnLessThan or PseudoOp.LoadImmediate or
            PseudoOp.LoadAddress or PseudoOp.SetGreaterThanOrEqual => 2,
            PseudoOp.AbsoluteValue => 3,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<int>(),
        };
}
