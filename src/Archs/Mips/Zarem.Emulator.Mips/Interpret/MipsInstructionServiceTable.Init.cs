// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Config;
using Zarem.Emulator.Interpret;
using Zarem.Emulator.Machine.Enums;
using Zarem.Extensions;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Functions;
using Zarem.Models.Instructions.Enums.Functions.FloatProc;

namespace Zarem.Emulator.Models;

public unsafe partial class MipsInstructionServiceTable<T, TS>
{
    private void InitTables(MipsEmulatorConfig config)
    {
        var version = config.Version;

        // Set default behavior to reserve instruction trap
        for (int i = 0; i < 64; i++)
        {
            _opCodeTable[i] = &ReservedInstruction;
            _specialTable[i] = &ReservedInstruction;
            _special2Table[i] = &ReservedInstruction;
        }

        for (int i = 0; i < 32; i++)
        {
            _regImmTable[i] = &ReservedInstruction;
        }

        // Populate tables
        InitRoot(version);
        InitSpecial(version);
        InitRegImm(version);
        InitFloat(version);
    }

    private void InitRoot(MipsVersion version)
    {
        _opCodeTable[(int)MipsOpCode.Special] = &DispatchSpecial;
        _opCodeTable[(int)MipsOpCode.RegisterImmediate] = &DispatchRegImm;
        _opCodeTable[(int)MipsOpCode.Jump] = &Jump;
        _opCodeTable[(int)MipsOpCode.JumpAndLink] = &JumpLink;
        _opCodeTable[(int)MipsOpCode.BranchOnEquals] = &BranchOn<XeqLogic<T>>;
        _opCodeTable[(int)MipsOpCode.BranchOnNotEquals] = &BranchOn<XneLogic<T>>;
        _opCodeTable[(int)MipsOpCode.BranchOnLessThanOrEqualToZero] = &BranchOn<XlezLogic<T, TS>>;
        _opCodeTable[(int)MipsOpCode.BranchOnGreaterThanZero] = &BranchOn<XgtzLogic<T, TS>>;
        _opCodeTable[(int)MipsOpCode.AddImmediate] = &CheckedAluI<CheckedAddLogic<int>, int>;
        _opCodeTable[(int)MipsOpCode.AddImmediateUnsigned] = &AluI<AddLogic<uint>, uint>;
        _opCodeTable[(int)MipsOpCode.SetLessThanImmediate] = &AluISigned<SltLogic<TS>, TS>;
        _opCodeTable[(int)MipsOpCode.SetLessThanImmediateUnsigned] = &AluI<SltLogic<T>, T>;
        _opCodeTable[(int)MipsOpCode.AndImmediate] = &AluI<AndLogic<T>, T>;
        _opCodeTable[(int)MipsOpCode.OrImmediate] = &AluI<OrLogic<T>, T>;
        _opCodeTable[(int)MipsOpCode.ExclusiveOrImmediate] = &AluI<XorLogic<T>, T>;
        _opCodeTable[(int)MipsOpCode.LoadUpperImmediate] = &Lui;
        _opCodeTable[(int)MipsOpCode.Coprocessor0] = &CreateCoProc0Execution;
        _opCodeTable[(int)MipsOpCode.Coprocessor1] = &DispatchCoProc1;
        _opCodeTable[(int)MipsOpCode.Coprocessor2] = &NotImplemented; // TODO
        _opCodeTable[(int)MipsOpCode.LoadByte] = &Load<sbyte>;
        _opCodeTable[(int)MipsOpCode.LoadHalfWord] = &Load<short>;
        _opCodeTable[(int)MipsOpCode.LoadWord] = &Load<int>;
        _opCodeTable[(int)MipsOpCode.LoadByteUnsigned] = &Load<byte>;
        _opCodeTable[(int)MipsOpCode.LoadHalfWordUnsigned] = &Load<ushort>;
        _opCodeTable[(int)MipsOpCode.StoreByte] = &Store<sbyte>;
        _opCodeTable[(int)MipsOpCode.StoreHalfWord] = &Store<short>;
        _opCodeTable[(int)MipsOpCode.StoreWord] = &Store<int>;
        _opCodeTable[(int)MipsOpCode.LoadWordCoprocessor1] = &NotImplemented; // TODO
        _opCodeTable[(int)MipsOpCode.StoreWordCoprocessor1] = &NotImplemented; // TODO

        if (version is < MipsVersion.MipsIII)
        {
            _opCodeTable[(int)MipsOpCode.Coprocessor3] = &NotImplemented; // TODO
            _opCodeTable[(int)MipsOpCode.LoadWordCoprocessor3] = &NotImplemented; // TODO
            _opCodeTable[(int)MipsOpCode.StoreWordCoprocessor3] = &NotImplemented; // TODO

            if (version is >= MipsVersion.MipsII)
            {
                _opCodeTable[(int)MipsOpCode.LoadDoubleWordCoprocessor3] = &NotImplemented; // TODO
                _opCodeTable[(int)MipsOpCode.StoreDoubleWordCoprocessor3] = &NotImplemented; // TODO
            }
        }
        
        if (version is >= MipsVersion.MipsIII && version.Is64Bit())
        {
            _opCodeTable[(int)MipsOpCode.DoubleWordAddImmediate] = &CheckedAluI<CheckedAddLogic<long>, long>;
            _opCodeTable[(int)MipsOpCode.DoubleWordAddImmediateUnsigned] = &AluI<AddLogic<ulong>, ulong>;
            _opCodeTable[(int)MipsOpCode.LoadDoubleWordLeft] = &NotImplemented;
            _opCodeTable[(int)MipsOpCode.LoadDoubleWordRight] = &NotImplemented;
            _opCodeTable[(int)MipsOpCode.LoadDoubleWord] = &Load<long>;
            _opCodeTable[(int)MipsOpCode.StoreDoubleWord] = &Store<long>;
        }

        if (version is < MipsVersion.Mips_R6)
        {
            _opCodeTable[(int)MipsOpCode.LoadWordLeft] = &NotImplemented; // TODO
            _opCodeTable[(int)MipsOpCode.LoadWordRight] = &NotImplemented;
            _opCodeTable[(int)MipsOpCode.StoreWordLeft] = &NotImplemented; // TODO
            _opCodeTable[(int)MipsOpCode.StoreWordRight] = &NotImplemented; // TODO
            _opCodeTable[(int)MipsOpCode.LoadWordCoprocessor2] = &NotImplemented; // TODO
            _opCodeTable[(int)MipsOpCode.StoreWordCoprocessor2] = &NotImplemented; // TODO
        }

        if (version is >= MipsVersion.MipsII)
        {
            _opCodeTable[(int)MipsOpCode.LoadLinkedWord] = &NotImplemented; // TODO
            _opCodeTable[(int)MipsOpCode.StoreConditionalWord] = &NotImplemented; // TODO
        }

        if (version is >= MipsVersion.MipsII and < MipsVersion.Mips_R6)
        {
            _opCodeTable[(int)MipsOpCode.BranchOnEqualLikely] = &BranchOnLikely<XeqLogic<T>>;
            _opCodeTable[(int)MipsOpCode.BranchOnNotEqualLikely] = &BranchOnLikely<XneLogic<T>>;
            _opCodeTable[(int)MipsOpCode.BranchOnLessThanOrEqualToZeroLikely] = &BranchOnLikely<XlezLogic<T, TS>>;
            _opCodeTable[(int)MipsOpCode.BranchOnGreaterThanZeroLikely] = &BranchOnLikely<XgtzLogic<T, TS>>;

            _opCodeTable[(int)MipsOpCode.LoadDoubleWordCoprocessor1] = &NotImplemented; // TODO
            _opCodeTable[(int)MipsOpCode.LoadDoubleWordCoprocessor2] = &NotImplemented; // TODO
            _opCodeTable[(int)MipsOpCode.StoreDoubleWordCoprocessor1] = &NotImplemented; // TODO
            _opCodeTable[(int)MipsOpCode.StoreDoubleWordCoprocessor2] = &NotImplemented; // TODO
        }

        if (version is >= MipsVersion.Mips_R1 and < MipsVersion.Mips_R6)
        {
            InitSpecial2();
            _opCodeTable[(int)MipsOpCode.Special2] = &DispatchSpecial2;
        }

        if (version is >= MipsVersion.Mips_R6)
        {
            _opCodeTable[(int)MipsOpCode.BranchCompact] = &NotImplemented; // TODO
            _opCodeTable[(int)MipsOpCode.BranchAndLinkCompact] = &NotImplemented; // TODO
        }
    }

    private void InitSpecial(MipsVersion version)
    {
        _specialTable[(int)FunctionCode.ShiftLeftLogical] = &Shift<SllLogic<uint>, uint>;
        _specialTable[(int)FunctionCode.ShiftRightLogical] = &Shift<SrlLogic<uint>, uint>;
        _specialTable[(int)FunctionCode.ShiftRightArithmetic] = &Shift<SraLogic<int>, int>;
        _specialTable[(int)FunctionCode.ShiftLeftLogicalVariable] = &ShiftVar<SllLogic<uint>, uint>;
        _specialTable[(int)FunctionCode.ShiftRightLogicalVariable] = &ShiftVar<SrlLogic<uint>, uint>;
        _specialTable[(int)FunctionCode.ShiftRightArithmeticVariable] = &ShiftVar<SraLogic<int>, int>;
        _specialTable[(int)FunctionCode.JumpAndLinkRegister] = &JumpLinkR;
        _specialTable[(int)FunctionCode.SystemCall] = &Trap<SyscallLogic>;
        _specialTable[(int)FunctionCode.Break] = &Trap<BreakLogic>;
        _specialTable[(int)FunctionCode.Add] = &CheckedAluR<CheckedAddLogic<int>, int>;
        _specialTable[(int)FunctionCode.AddUnsigned] = &AluR<AddLogic<uint>, uint>;
        _specialTable[(int)FunctionCode.Subtract] = &CheckedAluR<CheckedSubLogic<int>, int>;
        _specialTable[(int)FunctionCode.SubtractUnsigned] = &AluR<SubLogic<uint>, uint>;
        _specialTable[(int)FunctionCode.And] = &AluR<AndLogic<T>, T>;
        _specialTable[(int)FunctionCode.Or] = &AluR<OrLogic<T>, T>;
        _specialTable[(int)FunctionCode.ExclusiveOr] = &AluR<XorLogic<T>, T>;
        _specialTable[(int)FunctionCode.Nor] = &AluR<NorLogic<T>, T>;
        _specialTable[(int)FunctionCode.SetLessThan] = &AluR<SltLogic<TS>, TS>;
        _specialTable[(int)FunctionCode.SetLessThanUnsigned] = &AluR<SltLogic<T>, T>;

        if (version is >= MipsVersion.MipsII)
        {
            _specialTable[(int)FunctionCode.Sync] = &NotImplemented; // TODO
            _specialTable[(int)FunctionCode.TrapOnGreaterOrEqual] = &TrapOn<XgeLogic<T, TS>>;
            _specialTable[(int)FunctionCode.TrapOnGreaterOrEqualUnsigned] = &TrapOn<XgeuLogic<T>>;
            _specialTable[(int)FunctionCode.TrapOnLessThan] = &TrapOn<XltLogic<T, TS>>;
            _specialTable[(int)FunctionCode.TrapOnLessThanUnsigned] = &TrapOn<XltuLogic<T>>;
            _specialTable[(int)FunctionCode.TrapOnEquals] = &TrapOn<XeqLogic<T>>;
            _specialTable[(int)FunctionCode.TrapOnNotEquals] = &TrapOn<XneLogic<T>>;
        }

        if (version is >= MipsVersion.MipsIII && version.Is64Bit())
        {
            _specialTable[(int)FunctionCode.DoubleWordShiftLeftLogicalVariable] = &ShiftVar<SllLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightLogicalVariable] = &ShiftVar<SrlLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightArithmeticVariable] = &ShiftVar<SraLogic<long>, long>;
            _specialTable[(int)FunctionCode.DoubleWordMultiply] = &SignedMultR<MultLogic<long, Int128>, long, Int128>;
            _specialTable[(int)FunctionCode.DoubleWordMultiplyUnsigned] = &MultR<MultLogic<ulong, UInt128>, ulong, UInt128>;
            _specialTable[(int)FunctionCode.DoubleWordDivide] = &SignedDivR<DivLogic<long>, long>;
            _specialTable[(int)FunctionCode.DoubleWordDivideUnsigned] = &DivR<DivLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordAdd] = &CheckedAluR<CheckedAddLogic<long>, long>;
            _specialTable[(int)FunctionCode.DoubleWordAddUnsigned] = &AluR<AddLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordSubtract] = &CheckedAluR<CheckedSubLogic<long>, long>;
            _specialTable[(int)FunctionCode.DoubleWordSubtractUnsigned] = &AluR<SubLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftLeftLogical] = &Shift<SllLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightLogical] = &Shift<SrlLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightArithmetic] = &Shift<SraLogic<long>, long>;
            _specialTable[(int)FunctionCode.DoubleWordShiftLeftLogicalPlus32] = &ShiftPlus32<SllLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightLogicalPlus32] = &ShiftPlus32<SrlLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightArithmeticPlus32] = &ShiftPlus32<SraLogic<long>, long>;
        }

        if (version is >= MipsVersion.MipsIV and < MipsVersion.Mips_R6)
        {
            _specialTable[(int)FunctionCode.MoveOnZero] = &Move<XeqzLogic<T>>;
            _specialTable[(int)FunctionCode.MoveOnNotZero] = &Move<XnezLogic<T>>;
        }

        if (version is < MipsVersion.Mips_R6)
        {
            _specialTable[(int)FunctionCode.JumpRegister] = &JumpR;
            _specialTable[(int)FunctionCode.Multiply] = &SignedMultR<MultLogic<int, long>, int, long>;
            _specialTable[(int)FunctionCode.MultiplyUnsigned] = &MultR<MultLogic<uint, ulong>, uint, ulong>;
            _specialTable[(int)FunctionCode.Divide] = &SignedDivR<DivLogic<int>, int>;
            _specialTable[(int)FunctionCode.DivideUnsigned] = &DivR<DivLogic<uint>, uint>;
            _specialTable[(int)FunctionCode.MoveFromHigh] = &Mfhi;
            _specialTable[(int)FunctionCode.MoveToHigh] = &Mthi;
            _specialTable[(int)FunctionCode.MoveFromLow] = &Mflo;
            _specialTable[(int)FunctionCode.MoveToLow] = &Mtlo;
        }

        if (version is >= MipsVersion.Mips_R6)
        {
            _specialTable[(int)FunctionCode.SelectOnEquals] = &NotImplemented;
            _specialTable[(int)FunctionCode.SelectOnNotEquals] = &NotImplemented;
        }
    }

    private void InitRegImm(MipsVersion version)
    {
        _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZero] = &BranchOn<XltzLogic<T, TS>>;
        _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZero] = &BranchOn<XgezLogic<T, TS>>;

        if (version is >= MipsVersion.MipsII and < MipsVersion.Mips_R6)
        {
            _regImmTable[(int)RegImmFuncCode.TrapOnGreaterOrEqualImmediate] = &TrapOnI<XgeLogic<T, TS>>;
            _regImmTable[(int)RegImmFuncCode.TrapOnGreaterOrEqualImmediateUnsigned] = &TrapOnI<XgeuLogic<T>>;
            _regImmTable[(int)RegImmFuncCode.TrapOnLessThanImmediate] = &TrapOnI<XltLogic<T, TS>>;
            _regImmTable[(int)RegImmFuncCode.TrapOnLessThanImmediateUnsigned] = &TrapOnI<XltuLogic<T>>;
            _regImmTable[(int)RegImmFuncCode.TrapOnEqualsImmediate] = &TrapOnI<XeqLogic<T>>;
            _regImmTable[(int)RegImmFuncCode.TrapOnNotEqualsImmediate] = &TrapOnI<XneLogic<T>>;

            _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZeroLikely] = &BranchOnLikely<XltzLogic<T, TS>>;
            _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely] = &BranchOnLikely<XgezLogic<T, TS>>;
            _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZeroLikelyAndLink] = &BranchLinkOnLikely<XltzLogic<T, TS>>;
            _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink] = &BranchLinkOnLikely<XgezLogic<T, TS>>;
        }

        if (version is < MipsVersion.Mips_R6)
        {
            _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZeroAndLink] = &BranchLinkOn<XltzLogic<T, TS>>;
            _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroAndLink] = &BranchLinkOn<XgezLogic<T, TS>>;
        }

        if (version >= MipsVersion.Mips_R6)
        {
            _regImmTable[(int)RegImmFuncCode.NoOpAndLink] = &NotImplemented; // TODO
            _regImmTable[(int)RegImmFuncCode.BranchAndLink] = &NotImplemented; // TODO
        }
    }

    private void InitSpecial2()
    {
        _special2Table[(int)Func2Code.MultiplyToGPR] = &SignedAluR<MulLogic<int>, int>;
        _special2Table[(int)Func2Code.MultiplyAndAddHiLow] = &SignedMultAddR<MultAddLogic<int, long>, int, long>;
        _special2Table[(int)Func2Code.MultiplyAndAddHiLowUnsigned] = &MultAddR<MultAddLogic<uint, ulong>, uint, ulong>;
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLow] = &SignedMultAddR<MultSubLogic<int, long>, int, long>;
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLowUnsigned] = &MultAddR<MultSubLogic<uint, ulong>, uint, ulong>;
        _special2Table[(int)Func2Code.CountLeadingZeros] = &AluR<ClzLogic<uint>, uint>;
        _special2Table[(int)Func2Code.CountLeadingOnes] = &AluR<CloLogic<uint>, uint>;
    }

    private void InitFloat(MipsVersion version)
    {
        for (int i = 0; i < 32; i++)
        {
            _coProc1RSTable[i] = &ReservedInstruction;
        }

        for (int i = 0; i < _floatFuncTables.Length; i++)
        {
            _floatFuncTables[i] = new delegate*<MipsInstructionServiceTable<T, TS>, MipsFloatInstruction, out MipsExecution<T>, MipsTrap>[64];

            for (int j = 0; j < 64; j++)
            {
                _floatFuncTables[i][j] = &ReservedInstruction;
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
        _coProc1RSTable[(int)CoProc1RSCode.MFC1] = &MFC1;
        _coProc1RSTable[(int)CoProc1RSCode.MTC1] = &MTC1;
        _coProc1RSTable[(int)CoProc1RSCode.Single] = &DispatchFloatFunc<float>;
        _coProc1RSTable[(int)CoProc1RSCode.Double] = &DispatchFloatFunc<double>;
        _coProc1RSTable[(int)CoProc1RSCode.Word] = &DispatchFloatFunc<int>;

        if (version.Is64Bit())
        {
            _coProc1RSTable[(int)CoProc1RSCode.Long] = &DispatchFloatFunc<long>;
        }
    }

    private void InitFloatFuncs<TFormat>(MipsVersion version)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        int index = GetFloatFuncTableIndex<TFormat>();
        _floatFuncTables[index][(int)FloatFuncCode.Add] = &FloatAlu<AddLogic<TFormat>, TFormat>;
        _floatFuncTables[index][(int)FloatFuncCode.Subtract] = &FloatAlu<SubLogic<TFormat>, TFormat>;
        _floatFuncTables[index][(int)FloatFuncCode.Multiply] = &FloatAlu<MulLogic<TFormat>, TFormat>;
        _floatFuncTables[index][(int)FloatFuncCode.Divide] = &FloatAlu<DivLogic<TFormat>, TFormat>;
        _floatFuncTables[index][(int)FloatFuncCode.SquareRoot] = &FloatFAlu<SqrtLogic<TFormat>, TFormat>;
        _floatFuncTables[index][(int)FloatFuncCode.AbsoluteValue] = &FloatFAlu<AbsLogic<TFormat>, TFormat>;
        _floatFuncTables[index][(int)FloatFuncCode.Move] = &FloatFAlu<MovLogic<TFormat>, TFormat>;
        _floatFuncTables[index][(int)FloatFuncCode.Negate] = &FloatFAlu<NegLogic<TFormat>, TFormat>;

        _floatFuncTables[index][(int)FloatFuncCode.Round_W] = &FloatRound<RoundLogic<TFormat>, TFormat, int>;
        _floatFuncTables[index][(int)FloatFuncCode.Truncate_W] = &FloatRound<TruncLogic<TFormat>, TFormat, int>;
        _floatFuncTables[index][(int)FloatFuncCode.Ceiling_W] = &FloatRound<CeilingLogic<TFormat>, TFormat, int>;
        _floatFuncTables[index][(int)FloatFuncCode.Floor_W] = &FloatRound<FloorLogic<TFormat>, TFormat, int>;

        if (version.Is64Bit())
        {
            _floatFuncTables[index][(int)FloatFuncCode.Round_L] = &FloatRound<RoundLogic<TFormat>, TFormat, long>;
            _floatFuncTables[index][(int)FloatFuncCode.Truncate_L] = &FloatRound<TruncLogic<TFormat>, TFormat, long>;
            _floatFuncTables[index][(int)FloatFuncCode.Ceiling_L] = &FloatRound<CeilingLogic<TFormat>, TFormat, long>;
            _floatFuncTables[index][(int)FloatFuncCode.Floor_L] = &FloatRound<FloorLogic<TFormat>, TFormat, long>;
        }

        if (version >= MipsVersion.MipsIV)
        {
            _floatFuncTables[index][(int)FloatFuncCode.Reciprical] = &FloatFAlu<RecipLogic<TFormat>, TFormat>;
        }

        if (version >= MipsVersion.Mips_R2)
        {
            _floatFuncTables[index][(int)FloatFuncCode.RecipricalSquareRoot] = &FloatFAlu<RSqrtLogic<TFormat>, TFormat>;
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

        _floatFuncTables[index][(int)code] = &FloatConvert<TFrom, TTo>;
    }
}
