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
    public static GPRegister? GetWritebackRegister(this MipsInstruction instruction)
    {
        var arg = instruction.GetWritebackArgument();

        return arg switch
        {
            Argument.RD => instruction.RD,
            Argument.RT => instruction.RT,
            _ => null,
        };
    }

    /// <summary>
    /// Gets the argument register an instruction writes back to.
    /// </summary>
    /// <param name="instruction">The instruction.</param>
    /// <returns>Which argument register the instruction writes back to.</returns>
    public static Argument? GetWritebackArgument(this MipsInstruction instruction)
    {
        if (instruction.Type is InstructionType.BasicR)
        {
            return instruction.FuncCode switch
            {
                // All these instructions write back to $rd.
                FunctionCode.ShiftLeftLogical or FunctionCode.ShiftRightLogical or FunctionCode.ShiftRightArithmetic or                         // Shift
                FunctionCode.ShiftLeftLogicalVariable or FunctionCode.ShiftRightLogicalVariable or FunctionCode.ShiftRightArithmeticVariable or // Shift Variable
                FunctionCode.MoveFromHigh or FunctionCode.MoveFromLow or                                                                        // Move
                FunctionCode.Add or FunctionCode.AddUnsigned or FunctionCode.Subtract or FunctionCode.SubtractUnsigned or                       // Arithmetic
                FunctionCode.And or FunctionCode.Or or FunctionCode.ExclusiveOr or FunctionCode.Nor or                                          // Logical
                FunctionCode.SetLessThan or FunctionCode.SetLessThanUnsigned => Argument.RD,                                                    // Sets
                _ => null,
            };
        }

        return instruction.OpCode switch
        {
            // All these instructions write back to $rt.
            OperationCode.AddImmediate or OperationCode.AddImmediateUnsigned or                                                             // Arithmetic
            OperationCode.SetLessThanImmediate or OperationCode.SetLessThanImmediateUnsigned or                                             // Sets
            OperationCode.AndImmediate or OperationCode.OrImmediate or OperationCode.ExclusiveOrImmediate or                                // Logical
            OperationCode.LoadByte or OperationCode.LoadHalfWord or OperationCode.LoadWordLeft or OperationCode.LoadWord or                 // Loads
            OperationCode.LoadByteUnsigned or OperationCode.LoadHalfWordUnsigned or OperationCode.LoadWordRight => Argument.RT,             // Loads (continued)
            _ => null,
        };
    }
}
