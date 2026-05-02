// Avishai Dernis 2024

using CommunityToolkit.Diagnostics;
using Zarem.Models.Instructions.Enums.Functions;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Models.Instructions;

/// <summary>
/// A struct representing a pseudo instruction.
/// </summary>
public readonly struct RiscVPseudoInstruction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVPseudoInstruction"/> struct.
    /// </summary>
    public RiscVPseudoInstruction(RiscVPseudoOp op)
    {
        PseudoOp = op;
    }

    /// <summary>
    /// Gets the psudo operation code
    /// </summary>
    public RiscVPseudoOp PseudoOp { get; init; }

    /// <summary>
    /// Gets the pseudo-instruction rs1 register.
    /// </summary>
    public RiscVGpRegister RS1 { get; init; }

    /// <summary>
    /// Gets the pseudo-instruction rs2 register.
    /// </summary>
    public RiscVGpRegister RS2 { get; init; }

    /// <summary>
    /// Gets the pseudo-instruction rd register.
    /// </summary>
    public RiscVGpRegister RD { get; init; }

    /// <summary>
    /// Gets the pseudo-instruction immediate value.
    /// </summary>
    public int Immediate { get; init; }

    /// <summary>
    /// Expands the pseudo-instruction into an array of real instructions.
    /// </summary>
    public readonly RiscVInstruction[] Expand()
    {


        return PseudoOp switch
        {
            RiscVPseudoOp.NoOperation =>
            [
                // nop: add zero zero zero
                RiscVInstruction.CreateR(RiscVOpCode.Alu, Funct3Code.Arithmetic, Funct7Code.Base, 0, 0, 0),
            ],
            RiscVPseudoOp.Return =>
            [
                // ret: jalr zero 0(ra)
                RiscVInstruction.CreateR(RiscVOpCode.Alu, Funct3Code.Arithmetic, Funct7Code.Base, 0, 0, 0),
            ],
            RiscVPseudoOp.LoadImmediate =>
            [
                // li rd, imm: lui rd, upper; addi rd, rd, lower
                RiscVInstruction.CreateI(RiscVOpCode.LoadUpperImmediate, 0, RD, 0, (short)(Immediate >> 20)),
                RiscVInstruction.CreateI(RiscVOpCode.AluImmediate, Funct3Code.Arithmetic, RD, RD, (short)(Immediate & 0xFFF)),
            ],
            RiscVPseudoOp.Move =>
            [
                // move rd, rs: addu rd, rs, zero
                RiscVInstruction.CreateR(RiscVOpCode.Alu, Funct3Code.Arithmetic, Funct7Code.Base, RD, RS1, RS2),
            ],
            RiscVPseudoOp.LoadAddress =>
            [
                // la rd, imm: lui rd, upper; addi rd, rd, lower
                RiscVInstruction.CreateI(RiscVOpCode.LoadUpperImmediate, 0, RD, 0, (short)(Immediate >> 20)),
                RiscVInstruction.CreateI(RiscVOpCode.AluImmediate, Funct3Code.Arithmetic, RD, RD, (short)(Immediate & 0xFFF)),
            ],
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<RiscVInstruction[]>(),
        };
    }
}
