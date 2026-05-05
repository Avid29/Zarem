// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Config;
using Zarem.Emulator.Interpret;
using Zarem.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Functions;
using Zarem.Mips.Models.Instructions.Enums.Operations;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Extensions;
using Zarem.Mips.Models.Instructions.Enums.Functions.FloatProc;

namespace Zarem.Emulator.Models;

public unsafe partial class MipsInstructionServiceTable<T, TS>
{
    private void InitTables(MipsEmulatorConfig config)
    {
        var version = config.Version;
        InitRoot(version);
        InitSpecial(version);
        InitRegImm(version);
        InitFloat(version);
    }

    private void InitRoot(MipsVersion version)
    {
        Register(MipsOpCode.Jump, &Jump);
        Register(MipsOpCode.JumpAndLink, &JumpLink);
        Register(MipsOpCode.BranchOnEquals, &BranchOn<XeqLogic<T>>);
        Register(MipsOpCode.BranchOnNotEquals, &BranchOn<XneLogic<T>>);
        Register(MipsOpCode.BranchOnLessThanOrEqualToZero, &BranchOn<XlezLogic<T, TS>>);
        Register(MipsOpCode.BranchOnGreaterThanZero, &BranchOn<XgtzLogic<T, TS>>);
        Register(MipsOpCode.AddImmediate, &CheckedAluI<CheckedAddLogic<int>, int>);
        Register(MipsOpCode.AddImmediateUnsigned, &AluI<AddLogic<uint>, uint>);
        Register(MipsOpCode.SetLessThanImmediate, &AluISigned<SltLogic<TS>, TS>);
        Register(MipsOpCode.SetLessThanImmediateUnsigned, &AluI<SltLogic<T>, T>);
        Register(MipsOpCode.AndImmediate, &AluI<AndLogic<T>, T>);
        Register(MipsOpCode.OrImmediate, &AluI<OrLogic<T>, T>);
        Register(MipsOpCode.ExclusiveOrImmediate, &AluI<XorLogic<T>, T>);
        Register(MipsOpCode.LoadUpperImmediate, &Lui);
        Register(MipsOpCode.Coprocessor0, &CreateCoProc0Execution);
        Register(MipsOpCode.Coprocessor2, &NotImplemented); // TODO
        Register(MipsOpCode.LoadByte, &Load<sbyte>);
        Register(MipsOpCode.LoadHalfWord, &Load<short>);
        Register(MipsOpCode.LoadWord, &Load<int>);
        Register(MipsOpCode.LoadByteUnsigned, &Load<byte>);
        Register(MipsOpCode.LoadHalfWordUnsigned, &Load<ushort>);
        Register(MipsOpCode.StoreByte, &Store<sbyte>);
        Register(MipsOpCode.StoreHalfWord, &Store<short>);
        Register(MipsOpCode.StoreWord, &Store<int>);
        Register(MipsOpCode.LoadWordCoprocessor1, &NotImplemented); // TODO
        Register(MipsOpCode.StoreWordCoprocessor1, &NotImplemented); // TODO

        if (version is < MipsVersion.MipsIII)
        {
            Register(MipsOpCode.Coprocessor3, &NotImplemented); // TODO
            Register(MipsOpCode.LoadWordCoprocessor3, &NotImplemented); // TODO
            Register(MipsOpCode.StoreWordCoprocessor3, &NotImplemented); // TODO

            if (version is >= MipsVersion.MipsII)
            {
                Register(MipsOpCode.LoadDoubleWordCoprocessor3, &NotImplemented); // TODO
                Register(MipsOpCode.StoreDoubleWordCoprocessor3, &NotImplemented); // TODO
            }
        }
        
        if (version is >= MipsVersion.MipsIII && version.Is64Bit())
        {
            Register(MipsOpCode.DoubleWordAddImmediate, &CheckedAluI<CheckedAddLogic<long>, long>);
            Register(MipsOpCode.DoubleWordAddImmediateUnsigned, &AluI<AddLogic<ulong>, ulong>);
            Register(MipsOpCode.LoadDoubleWordLeft, &NotImplemented);
            Register(MipsOpCode.LoadDoubleWordRight, &NotImplemented);
            Register(MipsOpCode.LoadDoubleWord, &Load<long>);
            Register(MipsOpCode.StoreDoubleWord, &Store<long>);
        }

        if (version is < MipsVersion.Mips_R6)
        {
            Register(MipsOpCode.LoadWordLeft, &NotImplemented); // TODO
            Register(MipsOpCode.LoadWordRight, &NotImplemented);
            Register(MipsOpCode.StoreWordLeft, &NotImplemented); // TODO
            Register(MipsOpCode.StoreWordRight, &NotImplemented); // TODO
            Register(MipsOpCode.LoadWordCoprocessor2, &NotImplemented); // TODO
            Register(MipsOpCode.StoreWordCoprocessor2, &NotImplemented); // TODO
        }

        if (version is >= MipsVersion.MipsII)
        {
            Register(MipsOpCode.LoadLinkedWord, &NotImplemented); // TODO
            Register(MipsOpCode.StoreConditionalWord, &NotImplemented); // TODO
        }

        if (version is >= MipsVersion.MipsII and < MipsVersion.Mips_R6)
        {
            Register(MipsOpCode.BranchOnEqualLikely, &BranchOnLikely<XeqLogic<T>>);
            Register(MipsOpCode.BranchOnNotEqualLikely, &BranchOnLikely<XneLogic<T>>);
            Register(MipsOpCode.BranchOnLessThanOrEqualToZeroLikely, &BranchOnLikely<XlezLogic<T, TS>>);
            Register(MipsOpCode.BranchOnGreaterThanZeroLikely, &BranchOnLikely<XgtzLogic<T, TS>>);

            Register(MipsOpCode.LoadDoubleWordCoprocessor1, &NotImplemented); // TODO
            Register(MipsOpCode.LoadDoubleWordCoprocessor2, &NotImplemented); // TODO
            Register(MipsOpCode.StoreDoubleWordCoprocessor1, &NotImplemented); // TODO
            Register(MipsOpCode.StoreDoubleWordCoprocessor2, &NotImplemented); // TODO
        }

        if (version is >= MipsVersion.Mips_R1 and < MipsVersion.Mips_R6)
        {
            InitSpecial2();
        }

        if (version is >= MipsVersion.Mips_R6)
        {
            Register(MipsOpCode.BranchCompact, &NotImplemented); // TODO
            Register(MipsOpCode.BranchAndLinkCompact, &NotImplemented); // TODO
        }
    }

    private void InitSpecial(MipsVersion version)
    {
        Register(FunctionCode.ShiftLeftLogical, &Shift<SllLogic<uint>, uint>);
        Register(FunctionCode.ShiftRightLogical, &Shift<SrlLogic<uint>, uint>);
        Register(FunctionCode.ShiftRightArithmetic, &Shift<SraLogic<int>, int>);
        Register(FunctionCode.ShiftLeftLogicalVariable, &ShiftVar<SllLogic<uint>, uint>);
        Register(FunctionCode.ShiftRightLogicalVariable, &ShiftVar<SrlLogic<uint>, uint>);
        Register(FunctionCode.ShiftRightArithmeticVariable, &ShiftVar<SraLogic<int>, int>);
        Register(FunctionCode.JumpAndLinkRegister, &JumpLinkR);
        Register(FunctionCode.SystemCall, &Trap<SyscallLogic>);
        Register(FunctionCode.Break, &Trap<BreakLogic>);
        Register(FunctionCode.Add, &CheckedAluR<CheckedAddLogic<int>, int>);
        Register(FunctionCode.AddUnsigned, &AluR<AddLogic<uint>, uint>);
        Register(FunctionCode.Subtract, &CheckedAluR<CheckedSubLogic<int>, int>);
        Register(FunctionCode.SubtractUnsigned, &AluR<SubLogic<uint>, uint>);
        Register(FunctionCode.And, &AluR<AndLogic<T>, T>);
        Register(FunctionCode.Or, &AluR<OrLogic<T>, T>);
        Register(FunctionCode.ExclusiveOr, &AluR<XorLogic<T>, T>);
        Register(FunctionCode.Nor, &AluR<NorLogic<T>, T>);
        Register(FunctionCode.SetLessThan, &AluR<SltLogic<TS>, TS>);
        Register(FunctionCode.SetLessThanUnsigned, &AluR<SltLogic<T>, T>);

        if (version is >= MipsVersion.MipsII)
        {
            Register(FunctionCode.Sync, &NotImplemented); // TODO
            Register(FunctionCode.TrapOnGreaterOrEqual, &TrapOn<XgeLogic<T, TS>>);
            Register(FunctionCode.TrapOnGreaterOrEqualUnsigned, &TrapOn<XgeuLogic<T>>);
            Register(FunctionCode.TrapOnLessThan, &TrapOn<XltLogic<T, TS>>);
            Register(FunctionCode.TrapOnLessThanUnsigned, &TrapOn<XltuLogic<T>>);
            Register(FunctionCode.TrapOnEquals, &TrapOn<XeqLogic<T>>);
            Register(FunctionCode.TrapOnNotEquals, &TrapOn<XneLogic<T>>);
        }

        if (version is >= MipsVersion.MipsIII && version.Is64Bit())
        {
            Register(FunctionCode.DoubleWordShiftLeftLogicalVariable, &ShiftVar<SllLogic<ulong>, ulong>);
            Register(FunctionCode.DoubleWordShiftRightLogicalVariable, &ShiftVar<SrlLogic<ulong>, ulong>);
            Register(FunctionCode.DoubleWordShiftRightArithmeticVariable, &ShiftVar<SraLogic<long>, long>);
            Register(FunctionCode.DoubleWordMultiply, &SignedMultR<MultLogic<long, Int128>, long, Int128>);
            Register(FunctionCode.DoubleWordMultiplyUnsigned, &MultR<MultLogic<ulong, UInt128>, ulong, UInt128>);
            Register(FunctionCode.DoubleWordDivide, &SignedDivR<DivLogic<long>, long>);
            Register(FunctionCode.DoubleWordDivideUnsigned, &DivR<DivLogic<ulong>, ulong>);
            Register(FunctionCode.DoubleWordAdd, &CheckedAluR<CheckedAddLogic<long>, long>);
            Register(FunctionCode.DoubleWordAddUnsigned, &AluR<AddLogic<ulong>, ulong>);
            Register(FunctionCode.DoubleWordSubtract, &CheckedAluR<CheckedSubLogic<long>, long>);
            Register(FunctionCode.DoubleWordSubtractUnsigned, &AluR<SubLogic<ulong>, ulong>);
            Register(FunctionCode.DoubleWordShiftLeftLogical, &Shift<SllLogic<ulong>, ulong>);
            Register(FunctionCode.DoubleWordShiftRightLogical, &Shift<SrlLogic<ulong>, ulong>);
            Register(FunctionCode.DoubleWordShiftRightArithmetic, &Shift<SraLogic<long>, long>);
            Register(FunctionCode.DoubleWordShiftLeftLogicalPlus32, &ShiftPlus32<SllLogic<ulong>, ulong>);
            Register(FunctionCode.DoubleWordShiftRightLogicalPlus32, &ShiftPlus32<SrlLogic<ulong>, ulong>);
            Register(FunctionCode.DoubleWordShiftRightArithmeticPlus32, &ShiftPlus32<SraLogic<long>, long>);
        }

        if (version is >= MipsVersion.MipsIV and < MipsVersion.Mips_R6)
        {
            Register(FunctionCode.MoveOnZero, &Move<XeqzLogic<T>>);
            Register(FunctionCode.MoveOnNotZero, &Move<XnezLogic<T>>);
        }

        if (version is < MipsVersion.Mips_R6)
        {
            Register(FunctionCode.JumpRegister, &JumpR);
            Register(FunctionCode.Multiply, &SignedMultR<MultLogic<int, long>, int, long>);
            Register(FunctionCode.MultiplyUnsigned, &MultR<MultLogic<uint, ulong>, uint, ulong>);
            Register(FunctionCode.Divide, &SignedDivR<DivLogic<int>, int>);
            Register(FunctionCode.DivideUnsigned, &DivR<DivLogic<uint>, uint>);
            Register(FunctionCode.MoveFromHigh, &Mfhi);
            Register(FunctionCode.MoveToHigh, &Mthi);
            Register(FunctionCode.MoveFromLow, &Mflo);
            Register(FunctionCode.MoveToLow, &Mtlo);
        }

        if (version is >= MipsVersion.Mips_R6)
        {
            Register(FunctionCode.SelectOnEquals, &NotImplemented);
            Register(FunctionCode.SelectOnNotEquals, &NotImplemented);
        }
    }

    private void InitRegImm(MipsVersion version)
    {
        Register(RegImmFuncCode.BranchOnLessThanZero, &BranchOn<XltzLogic<T, TS>>);
        Register(RegImmFuncCode.BranchOnGreaterThanOrEqualToZero, &BranchOn<XgezLogic<T, TS>>);

        if (version is >= MipsVersion.MipsII and < MipsVersion.Mips_R6)
        {
            Register(RegImmFuncCode.TrapOnGreaterOrEqualImmediate, &TrapOnI<XgeLogic<T, TS>>);
            Register(RegImmFuncCode.TrapOnGreaterOrEqualImmediateUnsigned, &TrapOnI<XgeuLogic<T>>);
            Register(RegImmFuncCode.TrapOnLessThanImmediate, &TrapOnI<XltLogic<T, TS>>);
            Register(RegImmFuncCode.TrapOnLessThanImmediateUnsigned, &TrapOnI<XltuLogic<T>>);
            Register(RegImmFuncCode.TrapOnEqualsImmediate, &TrapOnI<XeqLogic<T>>);
            Register(RegImmFuncCode.TrapOnNotEqualsImmediate, &TrapOnI<XneLogic<T>>);

            Register(RegImmFuncCode.BranchOnLessThanZeroLikely, &BranchOnLikely<XltzLogic<T, TS>>);
            Register(RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely, &BranchOnLikely<XgezLogic<T, TS>>);
            Register(RegImmFuncCode.BranchOnLessThanZeroLikelyAndLink, &BranchLinkOnLikely<XltzLogic<T, TS>>);
            Register(RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink, &BranchLinkOnLikely<XgezLogic<T, TS>>);
        }

        if (version is < MipsVersion.Mips_R6)
        {
            Register(RegImmFuncCode.BranchOnLessThanZeroAndLink, &BranchLinkOn<XltzLogic<T, TS>>);
            Register(RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroAndLink, &BranchLinkOn<XgezLogic<T, TS>>);
        }

        if (version >= MipsVersion.Mips_R6)
        {
            Register(RegImmFuncCode.NoOpAndLink, &NotImplemented); // TODO
            Register(RegImmFuncCode.BranchAndLink, &NotImplemented); // TODO
        }
    }

    private void InitSpecial2()
    {
        Register(Func2Code.MultiplyToGPR, &SignedAluR<MulLogic<int>, int>);
        Register(Func2Code.MultiplyAndAddHiLow, &SignedMultAddR<MultAddLogic<int, long>, int, long>);
        Register(Func2Code.MultiplyAndAddHiLowUnsigned, &MultAddR<MultAddLogic<uint, ulong>, uint, ulong>);
        Register(Func2Code.MultiplyAndSubtractHiLow, &SignedMultAddR<MultSubLogic<int, long>, int, long>);
        Register(Func2Code.MultiplyAndSubtractHiLowUnsigned, &MultAddR<MultSubLogic<uint, ulong>, uint, ulong>);
        Register(Func2Code.CountLeadingZeros, &AluR<ClzLogic<uint>, uint>);
        Register(Func2Code.CountLeadingOnes, &AluR<CloLogic<uint>, uint>);
    }

    private void InitFloat(MipsVersion version)
    {
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
        Register(CoProc1RSCode.MFC1, &MFC1);
        Register(CoProc1RSCode.MTC1, &MTC1);
    }

    private void InitFloatFuncs<TFormat>(MipsVersion version)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var format = GetFloatFuncTableIndex<TFormat>();
        Register(format, MipsFloatFuncCode.Add, &FloatAlu<AddLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.Subtract, &FloatAlu<SubLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.Multiply, &FloatAlu<MulLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.Divide, &FloatAlu<DivLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.SquareRoot, &FloatFAlu<SqrtLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.AbsoluteValue, &FloatFAlu<AbsLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.Move, &FloatFAlu<MovLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.Negate, &FloatFAlu<NegLogic<TFormat>, TFormat>);

        Register(format, MipsFloatFuncCode.Round_W, &FloatRound<RoundLogic<TFormat>, TFormat, int>);
        Register(format, MipsFloatFuncCode.Truncate_W, &FloatRound<TruncLogic<TFormat>, TFormat, int>);
        Register(format, MipsFloatFuncCode.Ceiling_W, &FloatRound<CeilingLogic<TFormat>, TFormat, int>);
        Register(format, MipsFloatFuncCode.Floor_W, &FloatRound<FloorLogic<TFormat>, TFormat, int>);

        if (version.Is64Bit())
        {
            Register(format, MipsFloatFuncCode.Round_L, &FloatRound<RoundLogic<TFormat>, TFormat, long>);
            Register(format, MipsFloatFuncCode.Truncate_L, &FloatRound<TruncLogic<TFormat>, TFormat, long>);
            Register(format, MipsFloatFuncCode.Ceiling_L, &FloatRound<CeilingLogic<TFormat>, TFormat, long>);
            Register(format, MipsFloatFuncCode.Floor_L, &FloatRound<FloorLogic<TFormat>, TFormat, long>);
        }

        if (version >= MipsVersion.MipsIV)
        {
            Register(format, MipsFloatFuncCode.Reciprical, &FloatFAlu<RecipLogic<TFormat>, TFormat>);
        }

        if (version >= MipsVersion.Mips_R2)
        {
            Register(format, MipsFloatFuncCode.RecipricalSquareRoot, &FloatFAlu<RSqrtLogic<TFormat>, TFormat>);
        }
    }

    private void InitConvertFuncs<TFormat>(MipsVersion version)
        where TFormat : unmanaged, INumber<TFormat>
    {
        var format = GetFloatFuncTableIndex<TFormat>();
        InitConvertFunc<TFormat, float>(format, MipsFloatFuncCode.ConvertToSingle);
        InitConvertFunc<TFormat, double>(format, MipsFloatFuncCode.ConvertToDouble);
        InitConvertFunc<TFormat, int>(format, MipsFloatFuncCode.ConvertToWord);

        if (version.Is64Bit() && typeof(TFormat) != typeof(long))
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

        Register(format, code, &FloatConvert<TFrom, TTo>);
    }

    private void Register(MipsOpCode opCode, delegate*<MipsInterpretCpu<T>, MipsInstruction, out MipsExecution<T>, MipsTrap> func)
        => _instructionTable.Register(opCode, (IntPtr)func);

    private void Register(FunctionCode funcCode, delegate*<MipsInterpretCpu<T>, MipsInstruction, out MipsExecution<T>, MipsTrap> func)
        => _instructionTable.Register(funcCode, (IntPtr)func);

    private void Register(Func2Code funcCode, delegate*<MipsInterpretCpu<T>, MipsInstruction, out MipsExecution<T>, MipsTrap> func)
        => _instructionTable.Register(funcCode, (IntPtr)func);

    private void Register(RegImmFuncCode funcCode, delegate*<MipsInterpretCpu<T>, MipsInstruction, out MipsExecution<T>, MipsTrap> func)
        => _instructionTable.Register(funcCode, (IntPtr)func);

    private void Register(CoProc1RSCode funcCode, delegate*<MipsInterpretCpu<T>, MipsFloatInstruction, out MipsExecution<T>, MipsTrap> func)
        => _instructionTable.Register(funcCode, (IntPtr)func);

    private void Register(MipsFloatFormat format, MipsFloatFuncCode funcCode, delegate*<MipsInterpretCpu<T>, MipsFloatInstruction, out MipsExecution<T>, MipsTrap> func)
        => _instructionTable.Register(format, funcCode, (IntPtr)func);

    private static IntPtr GetFunctionPtrValue(delegate*<MipsInterpretCpu<T>, MipsInstruction, out MipsExecution<T>, MipsTrap> func)
        => (IntPtr)func;
}
