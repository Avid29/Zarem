// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine.Enums;
using Zarem.Extensions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;

namespace Zarem.Emulator.Models.JIT;

public partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private void InitTables(MipsEmulatorConfig config)
    {
        var version = config.Version;

        // Set default behavior to reserve instruction trap
        for (int i = 0; i < 64; i++)
        {
            _opCodeTable[i] = ReservedInstruction;
            _specialTable[i] = ReservedInstruction;
            _special2Table[i] = ReservedInstruction;
        }

        for (int i = 0; i < 32; i++)
        {
            _regImmTable[i] = ReservedInstruction;
        }

        // Populate tables
        InitRoot(version);
        InitSpecial(version);
        InitRegImm(version);
        InitFloat(version);
    }

    private void InitRoot(MipsVersion version)
    {
        _opCodeTable[(int)MipsOpCode.Special] = DispatchSpecial;
        _opCodeTable[(int)MipsOpCode.RegisterImmediate] = DispatchRegImm;
        _opCodeTable[(int)MipsOpCode.Jump] = (il, inst, pc) => Jump(il, inst, pc);
        _opCodeTable[(int)MipsOpCode.JumpAndLink] = (il, inst, pc) => Jump(il, inst, pc, link: true);
        _opCodeTable[(int)MipsOpCode.BranchOnEquals] = (il, inst, pc) => BranchCompareReg(il, inst, pc, OpCodes.Beq);
        _opCodeTable[(int)MipsOpCode.BranchOnNotEquals] = (il, inst, pc) => BranchCompareReg(il, inst, pc, OpCodes.Bne_Un);
        _opCodeTable[(int)MipsOpCode.BranchOnLessThanOrEqualToZero] = (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Ble);
        _opCodeTable[(int)MipsOpCode.BranchOnGreaterThanZero] = (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Bgt);
        _opCodeTable[(int)MipsOpCode.AddImmediate] = (il, inst, pc) => CheckedAluI<int>(il, inst, pc, OpCodes.Add);
        _opCodeTable[(int)MipsOpCode.AddImmediateUnsigned] = (il, inst, pc) => AluI<int>(il, inst, OpCodes.Add, signExtend: true);
        _opCodeTable[(int)MipsOpCode.SetLessThanImmediate] = (il, inst, pc) => AluI<T>(il, inst, OpCodes.Clt, signExtend: true);
        _opCodeTable[(int)MipsOpCode.SetLessThanImmediateUnsigned] = (il, inst, pc) => AluI<T>(il, inst, OpCodes.Clt_Un, signExtend: true);
        _opCodeTable[(int)MipsOpCode.AndImmediate] = (il, inst, pc) => AluI<T>(il, inst, OpCodes.And);
        _opCodeTable[(int)MipsOpCode.OrImmediate] = (il, inst, pc) => AluI<T>(il, inst, OpCodes.Or);
        _opCodeTable[(int)MipsOpCode.ExclusiveOrImmediate] = (il, inst, pc) => AluI<T>(il, inst, OpCodes.Xor);
        _opCodeTable[(int)MipsOpCode.LoadUpperImmediate] = Lui;
        _opCodeTable[(int)MipsOpCode.Coprocessor1] = (il, inst, pc) => DispatchCoProc1(il, inst, pc);
        _opCodeTable[(int)MipsOpCode.LoadByte] = Load<sbyte>;
        _opCodeTable[(int)MipsOpCode.LoadHalfWord] = Load<short>;
        _opCodeTable[(int)MipsOpCode.LoadWord] = Load<int>;
        _opCodeTable[(int)MipsOpCode.LoadByteUnsigned] = Load<byte>;
        _opCodeTable[(int)MipsOpCode.LoadHalfWordUnsigned] = Load<ushort>;
        _opCodeTable[(int)MipsOpCode.StoreByte] = Store<sbyte>;
        _opCodeTable[(int)MipsOpCode.StoreHalfWord] = Store<short>;
        _opCodeTable[(int)MipsOpCode.StoreWord] = Store<int>;

        if (version is >= MipsVersion.MipsIII && version.Is64Bit())
        {
            _opCodeTable[(int)MipsOpCode.DoubleWordAddImmediate] = (il, inst, pc) => CheckedAluI<long>(il, inst, pc, OpCodes.Add);
            _opCodeTable[(int)MipsOpCode.DoubleWordAddImmediateUnsigned] = (il, inst, pc) => AluI<long>(il, inst, OpCodes.Add, signExtend: true);
            //_opCodeTable[(int)MipsOpCode.LoadDoubleWordLeft] =
            //_opCodeTable[(int)MipsOpCode.LoadDoubleWordRight] = 
            _opCodeTable[(int)MipsOpCode.LoadDoubleWord] = Load<long>;
            _opCodeTable[(int)MipsOpCode.StoreDoubleWord] = Store<long>;
        }

        if (version is >= MipsVersion.MipsII and < MipsVersion.Mips_R6)
        {
            _opCodeTable[(int)MipsOpCode.BranchOnEqualLikely] = (il, inst, pc) => BranchCompareReg(il, inst, pc, OpCodes.Beq, true);
            _opCodeTable[(int)MipsOpCode.BranchOnNotEqualLikely] = (il, inst, pc) => BranchCompareReg(il, inst, pc, OpCodes.Bne_Un, true);
            _opCodeTable[(int)MipsOpCode.BranchOnLessThanOrEqualToZeroLikely] = (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Ble, true);
            _opCodeTable[(int)MipsOpCode.BranchOnGreaterThanZeroLikely] = (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Bgt, true);

            //_opCodeTable[(int)MipsOpCode.LoadDoubleWordCoprocessor1] = &NotImplemented; // TODO
            //_opCodeTable[(int)MipsOpCode.LoadDoubleWordCoprocessor2] = &NotImplemented; // TODO
            //_opCodeTable[(int)MipsOpCode.StoreDoubleWordCoprocessor1] = &NotImplemented; // TODO
            //_opCodeTable[(int)MipsOpCode.StoreDoubleWordCoprocessor2] = &NotImplemented; // TODO
        }
    }

    private void InitSpecial(MipsVersion version)
    {
        _specialTable[(int)FunctionCode.ShiftLeftLogical] = (il, inst, pc) => Shift<int>(il, inst, OpCodes.Shl);
        _specialTable[(int)FunctionCode.ShiftRightLogical] = (il, inst, pc) => Shift<int>(il, inst, OpCodes.Shr_Un);
        _specialTable[(int)FunctionCode.ShiftRightArithmetic] = (il, inst, pc) => Shift<int>(il, inst, OpCodes.Shr);
        _specialTable[(int)FunctionCode.ShiftLeftLogicalVariable] = (il, inst, pc) => ShiftVar<int>(il, inst, OpCodes.Shl);
        _specialTable[(int)FunctionCode.ShiftRightLogicalVariable] = (il, inst, pc) => ShiftVar<int>(il, inst, OpCodes.Shr_Un);
        _specialTable[(int)FunctionCode.ShiftRightArithmeticVariable] = (il, inst, pc) => ShiftVar<int>(il, inst, OpCodes.Shr);
        _specialTable[(int)FunctionCode.JumpAndLinkRegister] = (il, inst, pc) => JumpR(il, inst, pc, link: true);
        _specialTable[(int)FunctionCode.SystemCall] = (il, inst, pc) => EmitTrapRet(il, MipsTrap.Syscall, pc);
        _specialTable[(int)FunctionCode.Break] = (il, inst, pc) => EmitTrapRet(il, MipsTrap.Breakpoint, pc);
        _specialTable[(int)FunctionCode.Add] = (il, inst, pc) => CheckedAluR<int>(il, inst, pc, OpCodes.Add, false);
        _specialTable[(int)FunctionCode.AddUnsigned] = (il, inst, pc) => AluR<uint>(il, inst, OpCodes.Add);
        _specialTable[(int)FunctionCode.Subtract] = (il, inst, pc) => CheckedAluR<int>(il, inst, pc, OpCodes.Sub, true);
        _specialTable[(int)FunctionCode.SubtractUnsigned] = (il, inst, pc) => AluR<uint>(il, inst, OpCodes.Sub);
        _specialTable[(int)FunctionCode.And] = (il, inst, pc) => AluR<T>(il, inst, OpCodes.And);
        _specialTable[(int)FunctionCode.Or] = (il, inst, pc) => AluR<T>(il, inst, OpCodes.Or);
        _specialTable[(int)FunctionCode.ExclusiveOr] = (il, inst, pc) => AluR<T>(il, inst, OpCodes.Xor);
        _specialTable[(int)FunctionCode.Nor] = (il, inst, pc) => AluR<T>(il, inst, OpCodes.Or, followUp: OpCodes.Not);
        _specialTable[(int)FunctionCode.SetLessThan] = (il, inst, pc) => AluR<T>(il, inst, OpCodes.Clt);
        _specialTable[(int)FunctionCode.SetLessThanUnsigned] = (il, inst, pc) => AluR<T>(il, inst, OpCodes.Clt_Un);

        if (version is >= MipsVersion.MipsII)
        {
            // NOTE: Traps use inverted branchs for opcodes
            _specialTable[(int)FunctionCode.TrapOnGreaterOrEqual] = (il, inst, pc) => TrapCompareReg(il, inst, pc, OpCodes.Blt);
            _specialTable[(int)FunctionCode.TrapOnGreaterOrEqualUnsigned] = (il, inst, pc) => TrapCompareReg(il, inst, pc, OpCodes.Blt_Un);
            _specialTable[(int)FunctionCode.TrapOnLessThan] = (il, inst, pc) => TrapCompareReg(il, inst, pc, OpCodes.Bge);
            _specialTable[(int)FunctionCode.TrapOnLessThanUnsigned] = (il, inst, pc) => TrapCompareReg(il, inst, pc, OpCodes.Bge_Un);
            _specialTable[(int)FunctionCode.TrapOnEquals] = (il, inst, pc) => TrapCompareReg(il, inst, pc, OpCodes.Bne_Un);
            _specialTable[(int)FunctionCode.TrapOnNotEquals] = (il, inst, pc) => TrapCompareReg(il, inst, pc, OpCodes.Beq);
        }

        if (version is >= MipsVersion.MipsIII && version.Is64Bit())
        {
            _specialTable[(int)FunctionCode.DoubleWordShiftLeftLogicalVariable] = (il, inst, pc) => ShiftVar<long>(il, inst, OpCodes.Shl);
            _specialTable[(int)FunctionCode.DoubleWordShiftRightLogicalVariable] = (il, inst, pc) => ShiftVar<long>(il, inst, OpCodes.Shr_Un);
            _specialTable[(int)FunctionCode.DoubleWordShiftRightArithmeticVariable] = (il, inst, pc) => ShiftVar<long>(il, inst, OpCodes.Shr);
            _specialTable[(int)FunctionCode.DoubleWordMultiply] = (il, inst, pc) => MultR<long, Int128>(il, inst);
            _specialTable[(int)FunctionCode.DoubleWordMultiplyUnsigned] = (il, inst, pc) => MultR<ulong, UInt128>(il, inst);
            _specialTable[(int)FunctionCode.DoubleWordDivide] = (il, inst, pc) => DivR<long>(il, inst, true);
            _specialTable[(int)FunctionCode.DoubleWordDivideUnsigned] = (il, inst, pc) => DivR<long>(il, inst, false);
            _specialTable[(int)FunctionCode.DoubleWordAdd] = (il, inst, pc) => CheckedAluR<long>(il, inst, pc, OpCodes.Add, false);
            _specialTable[(int)FunctionCode.DoubleWordAddUnsigned] = (il, inst, pc) => AluR<ulong>(il, inst, OpCodes.Add);
            _specialTable[(int)FunctionCode.DoubleWordSubtract] = (il, inst, pc) => CheckedAluR<long>(il, inst, pc, OpCodes.Sub, true);
            _specialTable[(int)FunctionCode.DoubleWordSubtractUnsigned] = (il, inst, pc) => AluR<ulong>(il, inst, OpCodes.Sub);
            _specialTable[(int)FunctionCode.DoubleWordShiftLeftLogical] = (il, inst, pc) => Shift<long>(il, inst, OpCodes.Shl);
            _specialTable[(int)FunctionCode.DoubleWordShiftRightLogical] = (il, inst, pc) => Shift<long>(il, inst, OpCodes.Shr_Un);
            _specialTable[(int)FunctionCode.DoubleWordShiftRightArithmetic] = (il, inst, pc) => Shift<long>(il, inst, OpCodes.Shr);
            _specialTable[(int)FunctionCode.DoubleWordShiftLeftLogicalPlus32] = (il, inst, pc) => ShiftPlus32<long>(il, inst, OpCodes.Shl);
            _specialTable[(int)FunctionCode.DoubleWordShiftRightLogicalPlus32] = (il, inst, pc) => ShiftPlus32<long>(il, inst, OpCodes.Shr_Un);
            _specialTable[(int)FunctionCode.DoubleWordShiftRightArithmeticPlus32] = (il, inst, pc) => ShiftPlus32<long>(il, inst, OpCodes.Shr);
        }

        if (version is >= MipsVersion.MipsIV and < MipsVersion.Mips_R6)
        {
            _specialTable[(int)FunctionCode.MoveOnZero] = (il, inst, pc) => Move(il, inst, OpCodes.Brtrue);
            _specialTable[(int)FunctionCode.MoveOnNotZero] = (il, inst, pc) => Move(il, inst, OpCodes.Brfalse);
        }

        if (version is >= MipsVersion.Mips_R1 and < MipsVersion.Mips_R6)
        {
            InitSpecial2();
            _opCodeTable[(int)MipsOpCode.Special2] = DispatchSpecial2;
        }

        if (version is < MipsVersion.Mips_R6)
        {
            _specialTable[(int)FunctionCode.JumpRegister] = (il, inst, pc) => JumpR(il, inst, pc);
            _specialTable[(int)FunctionCode.Multiply] = (il, inst, pc) => MultR<int, long>(il, inst);
            _specialTable[(int)FunctionCode.MultiplyUnsigned] = (il, inst, pc) => MultR<uint, ulong>(il, inst);
            _specialTable[(int)FunctionCode.Divide] = (il, inst, pc) => DivR<int>(il, inst, true);
            _specialTable[(int)FunctionCode.DivideUnsigned] = (il, inst, pc) => DivR<uint>(il, inst, false);
            _specialTable[(int)FunctionCode.MoveFromHigh] = (il, inst, pc) => MoveFromTo(il, MipsGpRegister.High, inst.RD);
            _specialTable[(int)FunctionCode.MoveToHigh] = (il, inst, pc) => MoveFromTo(il, inst.RS, MipsGpRegister.High);
            _specialTable[(int)FunctionCode.MoveFromLow] = (il, inst, pc) => MoveFromTo(il, MipsGpRegister.Low, inst.RD);
            _specialTable[(int)FunctionCode.MoveToLow] = (il, inst, pc) => MoveFromTo(il, inst.RS, MipsGpRegister.Low);
        }
    }

    private void InitRegImm(MipsVersion version)
    {
        _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZero] = (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Blt);
        _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZero] = (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Bge);

        if (version is >= MipsVersion.MipsII and < MipsVersion.Mips_R6)
        {
            // NOTE: Traps use inverted branchs for opcodes
            _regImmTable[(int)RegImmFuncCode.TrapOnGreaterOrEqualImmediate] = (il, inst, pc) => TrapCompareImmediate(il, inst, pc, OpCodes.Blt);
            _regImmTable[(int)RegImmFuncCode.TrapOnGreaterOrEqualImmediateUnsigned] = (il, inst, pc) => TrapCompareImmediate(il, inst, pc, OpCodes.Blt_Un);
            _regImmTable[(int)RegImmFuncCode.TrapOnLessThanImmediate] = (il, inst, pc) => TrapCompareImmediate(il, inst, pc, OpCodes.Bge);
            _regImmTable[(int)RegImmFuncCode.TrapOnLessThanImmediateUnsigned] = (il, inst, pc) => TrapCompareImmediate(il, inst, pc, OpCodes.Bge_Un);
            _regImmTable[(int)RegImmFuncCode.TrapOnEqualsImmediate] = (il, inst, pc) => TrapCompareImmediate(il, inst, pc, OpCodes.Bne_Un);
            _regImmTable[(int)RegImmFuncCode.TrapOnNotEqualsImmediate] = (il, inst, pc) => TrapCompareImmediate(il, inst, pc, OpCodes.Beq);
            _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZeroLikely] = (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Blt, true);
            _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely] = (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Bge, true);
            _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZeroLikelyAndLink] = (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Beq, true);
            _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink] = (il, inst, pc) => BranchCompareZero(il, inst, pc, OpCodes.Beq, true);
        }
    }

    private void InitSpecial2()
    {
        _special2Table[(int)Func2Code.MultiplyToGPR] = (il, inst, pc) => AluR<int>(il, inst, OpCodes.Mul);
        _special2Table[(int)Func2Code.MultiplyAndAddHiLow] = (il, inst, pc) => MultR<int, long>(il, inst, 1);
        _special2Table[(int)Func2Code.MultiplyAndAddHiLowUnsigned] = (il, inst, pc) => MultR<uint, ulong>(il, inst, 1);
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLow] = (il, inst, pc) => MultR<int, long>(il, inst, -1);
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLowUnsigned] = (il, inst, pc) => MultR<int, long>(il, inst, -1);
        _special2Table[(int)Func2Code.CountLeadingZeros] = (il, inst, pc) => MethodUnary<uint>(il, inst, il => il.Emit(OpCodes.Call, _clzMethod));
        _special2Table[(int)Func2Code.CountLeadingOnes] = (il, inst, pc) => MethodUnary<uint>(il, inst, il =>
        {
            il.Emit(OpCodes.Not);
            il.Emit(OpCodes.Call, _clzMethod);
        });
    }

    private void InitFloat(MipsVersion version)
    {
        for (int i = 0; i < 32; i++)
        {
            _coProc1RSTable[i] = ReservedInstruction;
        }

        for (int i = 0; i < _floatFuncTables.Length; i++)
        {
            _floatFuncTables[i] = new MipsFloatEmitter[64];

            for (int j = 0; j < 64; j++)
            {
                _floatFuncTables[i][j] = ReservedInstruction;
            }
        }

        InitFloatRoot(version);
        InitFloatFuncs<float>(version);
        InitFloatFuncs<double>(version);

        InitConvertFuncs<float>(version);
        InitConvertFuncs<double>(version);
        InitConvertFuncs<int>(version);

        if (version.Is64Bit())
        {
            InitConvertFuncs<long>(version);
        }
    }

    private void InitFloatRoot(MipsVersion version)
    {
        _coProc1RSTable[(int)CoProc1RSCode.MFC1] = (il, inst, pc) => MoveFromFloat(il, inst);
        _coProc1RSTable[(int)CoProc1RSCode.MTC1] = (il, inst, pc) => MoveToFloat(il, inst);
        _coProc1RSTable[(int)CoProc1RSCode.Single] = DispatchFloatFunc<float>;
        _coProc1RSTable[(int)CoProc1RSCode.Double] = DispatchFloatFunc<double>;
        _coProc1RSTable[(int)CoProc1RSCode.Word] = DispatchFloatFunc<int>;

        if (version.Is64Bit())
        {
            _coProc1RSTable[(int)CoProc1RSCode.Long] = DispatchFloatFunc<long>;
        }
    }

    private void InitFloatFuncs<TFormat>(MipsVersion version)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        int index = GetFloatFuncTableIndex<TFormat>();
        _floatFuncTables[index][(int)FloatFuncCode.Add] = (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Add);
        _floatFuncTables[index][(int)FloatFuncCode.Subtract] = (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Sub);
        _floatFuncTables[index][(int)FloatFuncCode.Multiply] = (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Mul);
        _floatFuncTables[index][(int)FloatFuncCode.Divide] = (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Div);
        _floatFuncTables[index][(int)FloatFuncCode.SquareRoot] = (il, inst, pc) => FloatUnary<TFormat>(il, inst, nameof(Math.Sqrt));
        _floatFuncTables[index][(int)FloatFuncCode.AbsoluteValue] = (il, inst, pc) => FloatUnary<TFormat>(il, inst, nameof(Math.Abs));
        _floatFuncTables[index][(int)FloatFuncCode.Move] = (il, inst, pc) => MoveFloat<TFormat>(il, inst.FS, inst.FD);
        _floatFuncTables[index][(int)FloatFuncCode.Negate] = (il, inst, pc) => FloatUnary<TFormat>(il, inst, OpCodes.Neg);

        _floatFuncTables[index][(int)FloatFuncCode.Round_W] = (il, inst, pc) => FloatRound<TFormat, int>(il, inst, nameof(Math.Round));
        _floatFuncTables[index][(int)FloatFuncCode.Truncate_W] = (il, inst, pc) => FloatRound<TFormat, int>(il, inst, nameof(Math.Truncate));
        _floatFuncTables[index][(int)FloatFuncCode.Ceiling_W] = (il, inst, pc) => FloatRound<TFormat, int>(il, inst, nameof(Math.Ceiling));
        _floatFuncTables[index][(int)FloatFuncCode.Floor_W] = (il, inst, pc) => FloatRound<TFormat, int>(il, inst, nameof(Math.Floor));

        if (version.Is64Bit())
        {
            _floatFuncTables[index][(int)FloatFuncCode.Round_L] = (il, inst, pc) => FloatRound<TFormat, long>(il, inst, nameof(Math.Round));
            _floatFuncTables[index][(int)FloatFuncCode.Truncate_L] = (il, inst, pc) => FloatRound<TFormat, long>(il, inst, nameof(Math.Truncate));
            _floatFuncTables[index][(int)FloatFuncCode.Ceiling_L] = (il, inst, pc) => FloatRound<TFormat, long>(il, inst, nameof(Math.Ceiling));
            _floatFuncTables[index][(int)FloatFuncCode.Floor_L] = (il, inst, pc) => FloatRound<TFormat, long>(il, inst, nameof(Math.Floor));
        }

        if (version >= MipsVersion.MipsIV)
        {
            _floatFuncTables[index][(int)FloatFuncCode.Reciprical] = (il, inst, pc) => FloatUnary<TFormat>(il, inst, nameof(Math.ReciprocalEstimate));
        }

        if (version >= MipsVersion.Mips_R2)
        {
            _floatFuncTables[index][(int)FloatFuncCode.RecipricalSquareRoot] = (il, inst, pc) => FloatUnary<TFormat>(il, inst, nameof(Math.ReciprocalSqrtEstimate));
        }
    }

    private void InitConvertFuncs<TFormat>(MipsVersion version)
        where TFormat : unmanaged, INumber<TFormat>
    {
        int index = GetFloatFuncTableIndex<TFormat>();
        InitConvertFunc<TFormat, float>(index, FloatFuncCode.ConvertToSingle);
        InitConvertFunc<TFormat, double>(index, FloatFuncCode.ConvertToDouble);
        InitConvertFunc<TFormat, int>(index, FloatFuncCode.ConvertToWord);

        if (version.Is64Bit() && typeof(TFormat) != typeof(long))
        {
            InitConvertFunc<TFormat, long>(index, FloatFuncCode.ConvertToLong);
        }
    }

    private void InitConvertFunc<TFrom, TTo>(int index, FloatFuncCode code)
        where TFrom : unmanaged, INumber<TFrom>
        where TTo : unmanaged, INumber<TTo>
    {
        if (typeof(TFrom) == typeof(TTo))
            return;

        _floatFuncTables[index][(int)code] = (il, inst, pc) => FloatConvert<TFrom, TTo>(il, inst.FS, inst.FD);
    }
}
