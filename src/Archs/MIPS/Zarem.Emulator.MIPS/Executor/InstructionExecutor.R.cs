// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Executor.Enum;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Emulator.Executor;

/// <summary>
/// A class which handles converting decoded instructions into <see cref="Execution"/> models.
/// </summary>
public partial class InstructionExecutor
{
    private Execution CreateRTypeExecution()
    {
        return Instruction.OpCode switch
        {
            // Special (R-Type)
            OperationCode.Special => Instruction.FuncCode switch
            {
                // Shift
                FunctionCode.ShiftLeftLogical => ShiftR((rt, shift) => rt << shift),
                FunctionCode.ShiftRightLogical => ShiftR((rt, shift) => rt >> shift),
                FunctionCode.ShiftRightArithmetic => ShiftR((rt, shift) => (uint)((int)rt >> shift)),
                FunctionCode.ShiftLeftLogicalVariable => BasicR((rs, rt) => rt << (int)rs),
                FunctionCode.ShiftRightLogicalVariable => BasicR((rs, rt) => rt >> (int)rs),
                FunctionCode.ShiftRightArithmeticVariable => BasicR((rs, rt) => (uint)((int)rt >> (int)rs)),

                // Arithmetic
                FunctionCode.Add => BasicR(
                    (rs, rt) => (uint)((int)rs + (int)rt),
                    (a, b, r) => ((a ^ r) & (b ^ r)) < 0),
                FunctionCode.AddUnsigned => BasicR((rs, rt) => rs + rt),
                FunctionCode.Subtract => BasicR(
                    (rs, rt) => (uint)((int)rs - (int)rt),
                    (a, b, r) => ((a ^ b) & (a ^ r)) < 0),
                FunctionCode.SubtractUnsigned => BasicR((rs, rt) => rs - rt),
                FunctionCode.Multiply => MultR((rs, rt) => (ulong)((long)(int)rs * (int)rt)),
                FunctionCode.MultiplyUnsigned => MultR((rs, rt) => (ulong)rs * rt),
                FunctionCode.Divide => DivR(
                    (rs, rt) => rt is not 0 ? (uint)((int)rs / (int)rt) : 0,
                    (rs, rt) => rt is not 0 ? (uint)((int)rs % (int)rt) : rs),
                FunctionCode.DivideUnsigned => DivR(
                    (rs, rt) => rt is not 0 ? rs / rt : 0,
                    (rs, rt) => rt is not 0 ? rs % rt : rs),

                // Logical
                FunctionCode.And => BasicR((rs, rt) => rs & rt),
                FunctionCode.Or => BasicR((rs, rt) => rs | rt),
                FunctionCode.ExclusiveOr => BasicR((rs, rt) => rs ^ rt),
                FunctionCode.Nor => BasicR((rs, rt) => ~(rs | rt)),

                // Compare
                FunctionCode.SetLessThan => BasicR((rs, rt) => (uint)((int)rs < (int)rt ? 1 : 0)),
                FunctionCode.SetLessThanUnsigned => BasicR((rs, rt) => (uint)(rs < rt ? 1 : 0)),

                // Jump register
                FunctionCode.JumpRegister => JumpR(),
                FunctionCode.JumpAndLinkRegister => JumpR(Instruction.RD),

                // System
                FunctionCode.SystemCall => CreateTrap(MIPSTrap.Syscall),
                FunctionCode.Break => CreateTrap(MIPSTrap.Breakpoint),
                FunctionCode.Sync => throw new NotImplementedException(),

                FunctionCode.MoveFromHigh => new Execution(Instruction.RD, Processor.High),
                FunctionCode.MoveToHigh => new Execution
                {
                    High = RS,
                },
                FunctionCode.MoveFromLow => new Execution(Instruction.RD, Processor.Low),
                FunctionCode.MoveToLow => new Execution
                {
                    Low = RS,
                },

                // Trap
                FunctionCode.TrapOnGreaterOrEqual => TrapR((rs, rt) => (int)rs >= (int)rt),
                FunctionCode.TrapOnGreaterOrEqualUnsigned => TrapR((rs, rt) => rs >= rt),
                FunctionCode.TrapOnLessThan => TrapR((rs, rt) => (int)rs < (int)rt),
                FunctionCode.TrapOnLessThanUnsigned => TrapR((rs, rt) => rs < rt),
                FunctionCode.TrapOnEquals => TrapR((rs, rt) => rs == rt),
                FunctionCode.TrapOnNotEquals => TrapR((rs, rt) => rs != rt),

                _ => throw new NotImplementedException(),
            },

            // Special 2 (R-Type)
            OperationCode.Special2 when Config.MipsVersion is <= MipsVersion.MipsV => Instruction.Func2Code switch
            {
                // Multiply
                Func2Code.MultiplyToGPR => BasicR((rs, rt) => (uint)((long)(int)rs * (int)rt)),
                Func2Code.MultiplyAndAddHiLow => MultR((rs, rt) =>
                {
                    long sum = ((long)(int)Processor.High << 32) + (int)Processor.Low;
                    sum += (long)(int)rs * (int)rt;
                    return (ulong)sum;
                }),
                Func2Code.MultiplyAndAddHiLowUnsigned => MultR((rs, rt) =>
                {
                    ulong sum = ((ulong)Processor.High << 32) + Processor.Low;
                    sum += (ulong)rs * rt;
                    return sum;
                }),
                Func2Code.MultiplyAndSubtractHiLow => MultR((rs, rt) =>
                {
                    long diff = ((long)(int)Processor.High << 32) + (int)Processor.Low;
                    diff -= (long)(int)rs * (int)rt;
                    return (ulong)diff;
                }),
                Func2Code.MultiplyAndSubtractHiLowUnsigned => MultR((rs, rt) =>
                {
                    ulong diff = ((ulong)Processor.High << 32) + Processor.Low;
                    diff -= (ulong)rs * rt;
                    return diff;
                }),

                // Bit counting
                Func2Code.CountLeadingZeros => BasicR((rs, _) => (uint)BitOperations.LeadingZeroCount(rs)),
                Func2Code.CountLeadingOnes => BasicR((rs, _) => (uint)BitOperations.LeadingZeroCount(~rs)),
                _ => throw new NotImplementedException(),
            },

            _ => throw new NotImplementedException()
        };
    }

    private Execution BasicR(BasicRDelegate func, OverflowCheckDelegate? checkFunc = null)
    {
        // Determine the destination register and
        // compute the result using the provided function
        uint value = func(RS, RT);

        // Check for overflow if a check function is provided
        // Return a trap execution if overflow is detected
        if (checkFunc is not null &&
            checkFunc((int)RS, (int)RT, (int)value))
        {
            Trap = MIPSTrap.ArithmeticOverflow;
            return default;
        }

        // No overflow detected
        // Return the execution with the computed value and destination
        return new Execution(Instruction.RD, value);
    }

    private Execution ShiftR(ShiftRDelegate func)
    {
        uint value = func(RT, Instruction.ShiftAmount);
        return new Execution(Instruction.RD, value);
    }

    private Execution MultR(MultRDelegate func)
    {
        ulong value = func(RS, RT);
        return new Execution(value);
    }

    private Execution DivR(BasicRDelegate divFunc, BasicRDelegate remFunc)
    {
        uint div = divFunc(RS, RT);
        uint rem = remFunc(RS, RT);
        return new Execution((rem, div));
    }

    private Execution TrapR(BranchDelegate func)
        => func(RS, RT) ? CreateTrap(MIPSTrap.Trap) : default;

    private Execution JumpR(GPRegister? link = null)
    {
        if (link is null)
        {
            // No link register specified, just jump to the target address
            return new Execution(RS);
        }

        // A link register was provided
        // Write the return address to the specified link register
        // and set the program counter to the jump address
        return new Execution(link.Value, Processor.ProgramCounter + 4)
        {
            ProgramCounter = RS,
        };
    }
}
