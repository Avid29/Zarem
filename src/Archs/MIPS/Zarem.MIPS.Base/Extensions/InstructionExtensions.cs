// Avishai Dernis 2024

using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Extensions;

/// <summary>
/// A static class containing instruction extensions.
/// </summary>
public static class InstructionExtensions
{
    /// <summary>
    /// Gets the register an instruction writes back to.
    /// </summary>
    /// <param name="instruction">The instruction.</param>
    /// <returns>Which register the instruction writes back to.</returns>
    public static MipsGpRegister? GetWritebackRegister(this MipsInstruction instruction)
    {
        var arg = instruction.GetWritebackArgument();

        return arg switch
        {
            MipsArgument.RD => instruction.RD,
            MipsArgument.RT => instruction.RT,
            _ => null,
        };
    }

    /// <summary>
    /// Gets the argument register an instruction writes back to.
    /// </summary>
    /// <param name="instruction">The instruction.</param>
    /// <returns>Which argument register the instruction writes back to.</returns>
    public static MipsArgument? GetWritebackArgument(this MipsInstruction instruction)
    {
        if (instruction.Type is MipsInstructionType.BasicR)
        {
            return instruction.FuncCode switch
            {
                // All these instructions write back to $rd.
                FunctionCode.ShiftLeftLogical or FunctionCode.ShiftRightLogical or FunctionCode.ShiftRightArithmetic or                         // Shift
                FunctionCode.ShiftLeftLogicalVariable or FunctionCode.ShiftRightLogicalVariable or FunctionCode.ShiftRightArithmeticVariable or // Shift Variable
                FunctionCode.MoveFromHigh or FunctionCode.MoveFromLow or                                                                        // Move
                FunctionCode.Add or FunctionCode.AddUnsigned or FunctionCode.Subtract or FunctionCode.SubtractUnsigned or                       // Arithmetic
                FunctionCode.And or FunctionCode.Or or FunctionCode.ExclusiveOr or FunctionCode.Nor or                                          // Logical
                FunctionCode.SetLessThan or FunctionCode.SetLessThanUnsigned => MipsArgument.RD,                                                    // Sets
                _ => null,
            };
        }

        return instruction.OpCode switch
        {
            // All these instructions write back to $rt.
            MipsOpCode.AddImmediate or MipsOpCode.AddImmediateUnsigned or                                                             // Arithmetic
            MipsOpCode.SetLessThanImmediate or MipsOpCode.SetLessThanImmediateUnsigned or                                             // Sets
            MipsOpCode.AndImmediate or MipsOpCode.OrImmediate or MipsOpCode.ExclusiveOrImmediate or                                // Logical
            MipsOpCode.LoadByte or MipsOpCode.LoadHalfWord or MipsOpCode.LoadWordLeft or MipsOpCode.LoadWord or                 // Loads
            MipsOpCode.LoadByteUnsigned or MipsOpCode.LoadHalfWordUnsigned or MipsOpCode.LoadWordRight => MipsArgument.RT,             // Loads (continued)
            _ => null,
        };
    }

    /// <summary>
    /// Gets whether or not the mips version uses 64-bit registers.
    /// </summary>
    public static bool Is64Bit(this MipsVersion version)
        => version switch
        {
            MipsVersion.MipsIII or MipsVersion.MipsIV or MipsVersion.MipsV or
            MipsVersion.Mips64R1 or MipsVersion.Mips64R2 or
            MipsVersion.Mips64R3 or MipsVersion.Mips64R5 or
            MipsVersion.Mips64R6 => true,
            _ => false
        };
}
