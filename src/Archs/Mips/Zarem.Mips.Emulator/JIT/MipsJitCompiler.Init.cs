// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Functions;
using Zarem.Mips.Models.Instructions.Enums.Functions.FloatProc;
using Zarem.Mips.Models.Instructions.Enums.Operations;
using Zarem.Mips.Models.Instructions.Enums.Registers;
using Zarem.Mips.Models.Versioning;
using Zarem.Mips.Models.Versioning.Enums;

namespace Zarem.Emulator.Models.JIT;

public partial class MipsJitCompiler<T, TFloat>
{
    private void InitTables(MipsEmulatorConfig config)
    {
        var versionInfo = config.VersionInfo;
        InitRoot(versionInfo);
        InitSpecial(versionInfo);
        InitRegImm(versionInfo);
        InitFloat(versionInfo);
    }

    private void InitRoot(MipsVersionInfo versionInfo)
    {
        _instructionTable.Register(MipsOpCode.Jump, (il, inst, pc) => Jump(il, inst, pc));
        _instructionTable.Register(MipsOpCode.JumpAndLink, (il, inst, pc) => Jump(il, inst, pc, link: true));
        _instructionTable.Register(MipsOpCode.BranchOnEquals, (il, inst, pc) => BranchCompareReg(il, inst, pc, OpCodes.Beq));
        _instructionTable.Register(MipsOpCode.BranchOnNotEquals, (il, inst, pc) => BranchCompareReg(il, inst, pc, OpCodes.Bne_Un));
        _instructionTable.Register(MipsOpCode.BranchOnLessThanOrEqualToZero, (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Ble));
        _instructionTable.Register(MipsOpCode.BranchOnGreaterThanZero, (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Bgt));
        _instructionTable.Register(MipsOpCode.AddImmediate, (il, inst, pc) => CheckedAluI<int>(il, inst, pc, OpCodes.Add));
        _instructionTable.Register(MipsOpCode.AddImmediateUnsigned, (il, inst, pc) => AluI<int>(il, inst, OpCodes.Add, signExtend: true));
        _instructionTable.Register(MipsOpCode.SetLessThanImmediate, (il, inst, pc) => AluI<T>(il, inst, OpCodes.Clt, signExtend: true));
        _instructionTable.Register(MipsOpCode.SetLessThanImmediateUnsigned, (il, inst, pc) => AluI<T>(il, inst, OpCodes.Clt_Un, signExtend: true));
        _instructionTable.Register(MipsOpCode.AndImmediate, (il, inst, pc) => AluI<T>(il, inst, OpCodes.And));
        _instructionTable.Register(MipsOpCode.OrImmediate, (il, inst, pc) => AluI<T>(il, inst, OpCodes.Or));
        _instructionTable.Register(MipsOpCode.ExclusiveOrImmediate, (il, inst, pc) => AluI<T>(il, inst, OpCodes.Xor));
        _instructionTable.Register(MipsOpCode.LoadUpperImmediate, Lui);
        _instructionTable.Register(MipsOpCode.LoadByte, Load<sbyte>);
        _instructionTable.Register(MipsOpCode.LoadHalfWord, Load<short>);
        _instructionTable.Register(MipsOpCode.LoadWord, Load<int>);
        _instructionTable.Register(MipsOpCode.LoadByteUnsigned, Load<byte>);
        _instructionTable.Register(MipsOpCode.LoadHalfWordUnsigned, Load<ushort>);
        _instructionTable.Register(MipsOpCode.StoreByte, Store<sbyte>);
        _instructionTable.Register(MipsOpCode.StoreHalfWord, Store<short>);
        _instructionTable.Register(MipsOpCode.StoreWord, Store<int>);

        if (versionInfo.Generation is >= MipsGeneration.MipsIII && versionInfo.Is64Bit)
        {
            _instructionTable.Register(MipsOpCode.DoubleWordAddImmediate, (il, inst, pc) => CheckedAluI<long>(il, inst, pc, OpCodes.Add));
            _instructionTable.Register(MipsOpCode.DoubleWordAddImmediateUnsigned, (il, inst, pc) => AluI<long>(il, inst, OpCodes.Add, signExtend: true));
            //_opCodeTable[(int)MipsOpCode.LoadDoubleWordLeft] =
            //_opCodeTable[(int)MipsOpCode.LoadDoubleWordRight] = 
            _instructionTable.Register(MipsOpCode.LoadDoubleWord, Load<long>);
            _instructionTable.Register(MipsOpCode.StoreDoubleWord, Store<long>);
        }

        if (versionInfo.Generation is >= MipsGeneration.MipsII and < MipsGeneration.R6)
        {
            _instructionTable.Register(MipsOpCode.BranchOnEqualLikely, (il, inst, pc) => BranchCompareReg(il, inst, pc, OpCodes.Beq, true));
            _instructionTable.Register(MipsOpCode.BranchOnNotEqualLikely, (il, inst, pc) => BranchCompareReg(il, inst, pc, OpCodes.Bne_Un, true));
            _instructionTable.Register(MipsOpCode.BranchOnLessThanOrEqualToZeroLikely, (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Ble, true));
            _instructionTable.Register(MipsOpCode.BranchOnGreaterThanZeroLikely, (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Bgt, true));

            //_opCodeTable[(int)MipsOpCode.LoadDoubleWordCoprocessor1] = &NotImplemented; // TODO
            //_opCodeTable[(int)MipsOpCode.LoadDoubleWordCoprocessor2] = &NotImplemented; // TODO
            //_opCodeTable[(int)MipsOpCode.StoreDoubleWordCoprocessor1] = &NotImplemented; // TODO
            //_opCodeTable[(int)MipsOpCode.StoreDoubleWordCoprocessor2] = &NotImplemented; // TODO
        }
    }

    private void InitSpecial(MipsVersionInfo versionInfo)
    {
        _instructionTable.Register(FunctionCode.ShiftLeftLogical, (il, inst, pc) => Shift<int>(il, inst, OpCodes.Shl));
        _instructionTable.Register(FunctionCode.ShiftRightLogical, (il, inst, pc) => Shift<int>(il, inst, OpCodes.Shr_Un));
        _instructionTable.Register(FunctionCode.ShiftRightArithmetic, (il, inst, pc) => Shift<int>(il, inst, OpCodes.Shr));
        _instructionTable.Register(FunctionCode.ShiftLeftLogicalVariable, (il, inst, pc) => ShiftVar<int>(il, inst, OpCodes.Shl));
        _instructionTable.Register(FunctionCode.ShiftRightLogicalVariable, (il, inst, pc) => ShiftVar<int>(il, inst, OpCodes.Shr_Un));
        _instructionTable.Register(FunctionCode.ShiftRightArithmeticVariable, (il, inst, pc) => ShiftVar<int>(il, inst, OpCodes.Shr));
        _instructionTable.Register(FunctionCode.JumpAndLinkRegister, (il, inst, pc) => JumpR(il, inst, pc, link: true));
        _instructionTable.Register(FunctionCode.SystemCall, (il, inst, pc) => EmitTrapRet(il, MipsTrap.Syscall, pc));
        _instructionTable.Register(FunctionCode.Break, (il, inst, pc) => EmitTrapRet(il, MipsTrap.Breakpoint, pc));
        _instructionTable.Register(FunctionCode.Add, (il, inst, pc) => CheckedAluR<int>(il, inst, pc, OpCodes.Add, false));
        _instructionTable.Register(FunctionCode.AddUnsigned, (il, inst, pc) => AluR<uint>(il, inst, OpCodes.Add));
        _instructionTable.Register(FunctionCode.Subtract, (il, inst, pc) => CheckedAluR<int>(il, inst, pc, OpCodes.Sub, true));
        _instructionTable.Register(FunctionCode.SubtractUnsigned, (il, inst, pc) => AluR<uint>(il, inst, OpCodes.Sub));
        _instructionTable.Register(FunctionCode.And, (il, inst, pc) => AluR<T>(il, inst, OpCodes.And));
        _instructionTable.Register(FunctionCode.Or, (il, inst, pc) => AluR<T>(il, inst, OpCodes.Or));
        _instructionTable.Register(FunctionCode.ExclusiveOr, (il, inst, pc) => AluR<T>(il, inst, OpCodes.Xor));
        _instructionTable.Register(FunctionCode.Nor, (il, inst, pc) => AluR<T>(il, inst, OpCodes.Or, followUp: OpCodes.Not));
        _instructionTable.Register(FunctionCode.SetLessThan, (il, inst, pc) => AluR<T>(il, inst, OpCodes.Clt));
        _instructionTable.Register(FunctionCode.SetLessThanUnsigned, (il, inst, pc) => AluR<T>(il, inst, OpCodes.Clt_Un));

        if (versionInfo.Generation is >= MipsGeneration.MipsII)
        {
            // NOTE: Traps use inverted branchs for opcodes
            _instructionTable.Register(FunctionCode.TrapOnGreaterOrEqual, (il, inst, pc) => TrapCompareReg(il, inst, pc, OpCodes.Blt));
            _instructionTable.Register(FunctionCode.TrapOnGreaterOrEqualUnsigned, (il, inst, pc) => TrapCompareReg(il, inst, pc, OpCodes.Blt_Un));
            _instructionTable.Register(FunctionCode.TrapOnLessThan, (il, inst, pc) => TrapCompareReg(il, inst, pc, OpCodes.Bge));
            _instructionTable.Register(FunctionCode.TrapOnLessThanUnsigned, (il, inst, pc) => TrapCompareReg(il, inst, pc, OpCodes.Bge_Un));
            _instructionTable.Register(FunctionCode.TrapOnEquals, (il, inst, pc) => TrapCompareReg(il, inst, pc, OpCodes.Bne_Un));
            _instructionTable.Register(FunctionCode.TrapOnNotEquals, (il, inst, pc) => TrapCompareReg(il, inst, pc, OpCodes.Beq));
        }

        if (versionInfo.Generation is >= MipsGeneration.MipsIII && versionInfo.Is64Bit)
        {
            _instructionTable.Register(FunctionCode.DoubleWordShiftLeftLogicalVariable, (il, inst, pc) => ShiftVar<long>(il, inst, OpCodes.Shl));
            _instructionTable.Register(FunctionCode.DoubleWordShiftRightLogicalVariable, (il, inst, pc) => ShiftVar<long>(il, inst, OpCodes.Shr_Un));
            _instructionTable.Register(FunctionCode.DoubleWordShiftRightArithmeticVariable, (il, inst, pc) => ShiftVar<long>(il, inst, OpCodes.Shr));
            _instructionTable.Register(FunctionCode.DoubleWordMultiply, (il, inst, pc) => MultR<long, Int128>(il, inst));
            _instructionTable.Register(FunctionCode.DoubleWordMultiplyUnsigned, (il, inst, pc) => MultR<ulong, UInt128>(il, inst));
            _instructionTable.Register(FunctionCode.DoubleWordDivide, (il, inst, pc) => DivR<long>(il, inst, true));
            _instructionTable.Register(FunctionCode.DoubleWordDivideUnsigned, (il, inst, pc) => DivR<long>(il, inst, false));
            _instructionTable.Register(FunctionCode.DoubleWordAdd, (il, inst, pc) => CheckedAluR<long>(il, inst, pc, OpCodes.Add, false));
            _instructionTable.Register(FunctionCode.DoubleWordAddUnsigned, (il, inst, pc) => AluR<ulong>(il, inst, OpCodes.Add));
            _instructionTable.Register(FunctionCode.DoubleWordSubtract, (il, inst, pc) => CheckedAluR<long>(il, inst, pc, OpCodes.Sub, true));
            _instructionTable.Register(FunctionCode.DoubleWordSubtractUnsigned, (il, inst, pc) => AluR<ulong>(il, inst, OpCodes.Sub));
            _instructionTable.Register(FunctionCode.DoubleWordShiftLeftLogical, (il, inst, pc) => Shift<long>(il, inst, OpCodes.Shl));
            _instructionTable.Register(FunctionCode.DoubleWordShiftRightLogical, (il, inst, pc) => Shift<long>(il, inst, OpCodes.Shr_Un));
            _instructionTable.Register(FunctionCode.DoubleWordShiftRightArithmetic, (il, inst, pc) => Shift<long>(il, inst, OpCodes.Shr));
            _instructionTable.Register(FunctionCode.DoubleWordShiftLeftLogicalPlus32, (il, inst, pc) => ShiftPlus32<long>(il, inst, OpCodes.Shl));
            _instructionTable.Register(FunctionCode.DoubleWordShiftRightLogicalPlus32, (il, inst, pc) => ShiftPlus32<long>(il, inst, OpCodes.Shr_Un));
            _instructionTable.Register(FunctionCode.DoubleWordShiftRightArithmeticPlus32, (il, inst, pc) => ShiftPlus32<long>(il, inst, OpCodes.Shr));
        }

        if (versionInfo.Generation is >= MipsGeneration.MipsIV and < MipsGeneration.R6)
        {
            _instructionTable.Register(FunctionCode.MoveOnZero, (il, inst, pc) => Move(il, inst, OpCodes.Brtrue));
            _instructionTable.Register(FunctionCode.MoveOnNotZero, (il, inst, pc) => Move(il, inst, OpCodes.Brfalse));
        }

        if (versionInfo.Generation is >= MipsGeneration.R1 and < MipsGeneration.R6)
        {
            InitSpecial2();
        }

        if (versionInfo.Generation is < MipsGeneration.R6)
        {
            _instructionTable.Register(FunctionCode.JumpRegister, (il, inst, pc) => JumpR(il, inst, pc));
            _instructionTable.Register(FunctionCode.Multiply, (il, inst, pc) => MultR<int, long>(il, inst));
            _instructionTable.Register(FunctionCode.MultiplyUnsigned, (il, inst, pc) => MultR<uint, ulong>(il, inst));
            _instructionTable.Register(FunctionCode.Divide, (il, inst, pc) => DivR<int>(il, inst, true));
            _instructionTable.Register(FunctionCode.DivideUnsigned, (il, inst, pc) => DivR<uint>(il, inst, false));
            _instructionTable.Register(FunctionCode.MoveFromHigh, (il, inst, pc) => MoveFromTo(il, MipsGpRegister.High, inst.RD));
            _instructionTable.Register(FunctionCode.MoveToHigh, (il, inst, pc) => MoveFromTo(il, inst.RS, MipsGpRegister.High));
            _instructionTable.Register(FunctionCode.MoveFromLow, (il, inst, pc) => MoveFromTo(il, MipsGpRegister.Low, inst.RD));
            _instructionTable.Register(FunctionCode.MoveToLow, (il, inst, pc) => MoveFromTo(il, inst.RS, MipsGpRegister.Low));
        }
    }

    private void InitRegImm(MipsVersionInfo versionInfo)
    {
        _instructionTable.Register(RegImmFuncCode.BranchOnLessThanZero, (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Blt));
        _instructionTable.Register(RegImmFuncCode.BranchOnGreaterThanOrEqualToZero, (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Bge));

        if (versionInfo.Generation is >= MipsGeneration.MipsII and < MipsGeneration.R6)
        {
            // NOTE: Traps use inverted branchs for opcodes
            _instructionTable.Register(RegImmFuncCode.TrapOnGreaterOrEqualImmediate, (il, inst, pc) => TrapCompareImmediate(il, inst, pc, OpCodes.Blt));
            _instructionTable.Register(RegImmFuncCode.TrapOnGreaterOrEqualImmediateUnsigned, (il, inst, pc) => TrapCompareImmediate(il, inst, pc, OpCodes.Blt_Un));
            _instructionTable.Register(RegImmFuncCode.TrapOnLessThanImmediate, (il, inst, pc) => TrapCompareImmediate(il, inst, pc, OpCodes.Bge));
            _instructionTable.Register(RegImmFuncCode.TrapOnLessThanImmediateUnsigned, (il, inst, pc) => TrapCompareImmediate(il, inst, pc, OpCodes.Bge_Un));
            _instructionTable.Register(RegImmFuncCode.TrapOnEqualsImmediate, (il, inst, pc) => TrapCompareImmediate(il, inst, pc, OpCodes.Bne_Un));
            _instructionTable.Register(RegImmFuncCode.TrapOnNotEqualsImmediate, (il, inst, pc) => TrapCompareImmediate(il, inst, pc, OpCodes.Beq));
            _instructionTable.Register(RegImmFuncCode.BranchOnLessThanZeroLikely, (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Blt, true));
            _instructionTable.Register(RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely, (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Bge, true));
            _instructionTable.Register(RegImmFuncCode.BranchOnLessThanZeroLikelyAndLink, (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Beq, true));
            _instructionTable.Register(RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink, (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Beq, true));
        }
    }

    private void InitSpecial2()
    {
        _instructionTable.Register(Func2Code.MultiplyToGPR, (il, inst, pc) => AluR<int>(il, inst, OpCodes.Mul));
        _instructionTable.Register(Func2Code.MultiplyAndAddHiLow, (il, inst, pc) => MultR<int, long>(il, inst, 1));
        _instructionTable.Register(Func2Code.MultiplyAndAddHiLowUnsigned, (il, inst, pc) => MultR<uint, ulong>(il, inst, 1));
        _instructionTable.Register(Func2Code.MultiplyAndSubtractHiLow, (il, inst, pc) => MultR<int, long>(il, inst, -1));
        _instructionTable.Register(Func2Code.MultiplyAndSubtractHiLowUnsigned, (il, inst, pc) => MultR<int, long>(il, inst, -1));
        _instructionTable.Register(Func2Code.CountLeadingZeros, (il, inst, pc) => MethodUnary<uint>(il, inst, il => il.Emit(OpCodes.Call, _clzMethod)));
        _instructionTable.Register(Func2Code.CountLeadingOnes, (il, inst, pc) => MethodUnary<uint>(il, inst, il =>
        {
            il.Emit(OpCodes.Not);
            il.Emit(OpCodes.Call, _clzMethod);
        }));
    }

    private void InitFloat(MipsVersionInfo versionInfo)
    {
        InitFloatRoot(versionInfo);
        InitFloatFuncs<float>(versionInfo);
        InitFloatFuncs<double>(versionInfo);

        InitConvertFuncs<float>(versionInfo);
        InitConvertFuncs<double>(versionInfo);
        InitConvertFuncs<int>(versionInfo);

        if (versionInfo.Is64Bit)
        {
            InitConvertFuncs<long>(versionInfo);
        }
    }

    private void InitFloatRoot(MipsVersionInfo versionInfo)
    {
        _instructionTable.Register(CoProc1RSCode.MFC1, (il, inst, pc) => MoveFromFloat(il, inst));
        _instructionTable.Register(CoProc1RSCode.MTC1, (il, inst, pc) => MoveToFloat(il, inst));
    }

    private void InitFloatFuncs<TFormat>(MipsVersionInfo versionInfo)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var format = MipsInstructionDecodeTable<T>.GetFloatFuncTableIndex<TFormat>();
        _instructionTable.Register(format, MipsFloatFuncCode.Add, (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Add));
        _instructionTable.Register(format, MipsFloatFuncCode.Subtract, (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Sub));
        _instructionTable.Register(format, MipsFloatFuncCode.Multiply, (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Mul));
        _instructionTable.Register(format, MipsFloatFuncCode.Divide, (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Div));
        _instructionTable.Register(format, MipsFloatFuncCode.SquareRoot, (il, inst, pc) => FloatUnary<TFormat>(il, inst, nameof(Math.Sqrt)));
        _instructionTable.Register(format, MipsFloatFuncCode.AbsoluteValue, (il, inst, pc) => FloatUnary<TFormat>(il, inst, nameof(Math.Abs)));
        _instructionTable.Register(format, MipsFloatFuncCode.Move, (il, inst, pc) => MoveFloat<TFormat>(il, ((MipsFloatInstruction)inst).FS, ((MipsFloatInstruction)inst).FD));
        _instructionTable.Register(format, MipsFloatFuncCode.Negate, (il, inst, pc) => FloatUnary<TFormat>(il, inst, OpCodes.Neg));

        _instructionTable.Register(format, MipsFloatFuncCode.Round_W, (il, inst, pc) => FloatRound<TFormat, int>(il, inst, nameof(Math.Round)));
        _instructionTable.Register(format, MipsFloatFuncCode.Truncate_W, (il, inst, pc) => FloatRound<TFormat, int>(il, inst, nameof(Math.Truncate)));
        _instructionTable.Register(format, MipsFloatFuncCode.Ceiling_W, (il, inst, pc) => FloatRound<TFormat, int>(il, inst, nameof(Math.Ceiling)));
        _instructionTable.Register(format, MipsFloatFuncCode.Floor_W, (il, inst, pc) => FloatRound<TFormat, int>(il, inst, nameof(Math.Floor)));

        if (versionInfo.Is64Bit)
        {
            _instructionTable.Register(format, MipsFloatFuncCode.Round_L, (il, inst, pc) => FloatRound<TFormat, long>(il, inst, nameof(Math.Round)));
            _instructionTable.Register(format, MipsFloatFuncCode.Truncate_L, (il, inst, pc) => FloatRound<TFormat, long>(il, inst, nameof(Math.Truncate)));
            _instructionTable.Register(format, MipsFloatFuncCode.Ceiling_L, (il, inst, pc) => FloatRound<TFormat, long>(il, inst, nameof(Math.Ceiling)));
            _instructionTable.Register(format, MipsFloatFuncCode.Floor_L, (il, inst, pc) => FloatRound<TFormat, long>(il, inst, nameof(Math.Floor)));
        }

        if (versionInfo.Generation >= MipsGeneration.MipsIV)
        {
            _instructionTable.Register(format, MipsFloatFuncCode.Reciprical, (il, inst, pc) => FloatUnary<TFormat>(il, inst, nameof(Math.ReciprocalEstimate)));
        }

        if (versionInfo.Generation >= MipsGeneration.R2)
        {
            _instructionTable.Register(format, MipsFloatFuncCode.RecipricalSquareRoot, (il, inst, pc) => FloatUnary<TFormat>(il, inst, nameof(Math.ReciprocalSqrtEstimate)));
        }
    }

    private void InitConvertFuncs<TFormat>(MipsVersionInfo versionInfo)
        where TFormat : unmanaged, INumber<TFormat>
    {
        var format = MipsInstructionDecodeTable<T>.GetFloatFuncTableIndex<TFormat>();
        InitConvertFunc<TFormat, float>(format, MipsFloatFuncCode.ConvertToSingle);
        InitConvertFunc<TFormat, double>(format, MipsFloatFuncCode.ConvertToDouble);
        InitConvertFunc<TFormat, int>(format, MipsFloatFuncCode.ConvertToWord);

        if (versionInfo.Is64Bit && typeof(TFormat) != typeof(long))
        {
            InitConvertFunc<TFormat, long>(format, MipsFloatFuncCode.ConvertToLong);
        }
    }

    private void InitConvertFunc<TFrom, TTo>(MipsFloatFormat format, MipsFloatFuncCode code)
        where TFrom : unmanaged, INumber<TFrom>
        where TTo : unmanaged, INumber<TTo>
    {
        if (typeof(TFrom) == typeof(TTo))
            return;

        _instructionTable.Register(format, code, (il, inst, pc) => FloatConvert<TFrom, TTo>(il, ((MipsFloatInstruction)inst).FS, ((MipsFloatInstruction)inst).FD));
    }
}
