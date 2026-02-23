// Adam Dernis 2024

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
    public readonly MIPSInstruction[] Expand()
    {
        return PseudoOp switch
        {
            PseudoOp.NoOperation =>
            [
                MIPSInstruction.Create(FunctionCode.ShiftLeftLogical, GPRegister.Zero, GPRegister.Zero, GPRegister.Zero, 0),
            ],
            PseudoOp.SuperScalarNoOperation =>
            [
                MIPSInstruction.Create(FunctionCode.ShiftLeftLogical, GPRegister.Zero, GPRegister.Zero, GPRegister.Zero, 1),
            ],
            PseudoOp.UnconditionalBranch =>
            [
                MIPSInstruction.Create(OperationCode.BranchOnEquals, GPRegister.Zero, GPRegister.Zero, (short)Immediate),
            ],
            PseudoOp.BranchOnLessThan =>
            [
                MIPSInstruction.Create(FunctionCode.SetLessThan, RS, RT, GPRegister.AssemblerTemporary),
                MIPSInstruction.Create(OperationCode.BranchOnNotEquals, GPRegister.AssemblerTemporary, GPRegister.Zero, (short)Immediate)
            ],
            PseudoOp.LoadImmediate =>
            [
                MIPSInstruction.Create(OperationCode.LoadUpperImmediate, RT, (short)(Immediate >> 16)),
                MIPSInstruction.Create(OperationCode.OrImmediate, RT, RT, (short)Immediate)
            ],
            PseudoOp.AbsoluteValue =>
            [
                MIPSInstruction.Create(FunctionCode.AddUnsigned, RS, GPRegister.Zero, RT),
                MIPSInstruction.Create(RegImmFuncCode.BranchOnGreaterThanOrEqualToZero, RS, 8),
                MIPSInstruction.Create(FunctionCode.Subtract, GPRegister.Zero, RS, RT),
            ],
            PseudoOp.Move =>
            [
                MIPSInstruction.Create(FunctionCode.Add, RS, GPRegister.Zero, RT),
            ],
            PseudoOp.LoadAddress =>
            [
                MIPSInstruction.Create(OperationCode.LoadUpperImmediate, RT, (short)(Immediate >> 16)),
                MIPSInstruction.Create(OperationCode.OrImmediate, RT, RT, (short)Immediate)
            ],
            PseudoOp.SetGreaterThanOrEqual =>
            [
                MIPSInstruction.Create(OperationCode.AddImmediateUnsigned, RT, RT, (short)-1),
                MIPSInstruction.Create(FunctionCode.SetLessThan, RS, RT, RD),
            ],
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<MIPSInstruction[]>(),
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
