// Avishai Dernis 2026

using System;
using Zarem.Emulator.Config;
using Zarem.Extensions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.SpecialFunctions;
using static Zarem.Emulator.Models.LogicTable;

namespace Zarem.Emulator.Models;

public unsafe partial class MipsInstructionServiceTable<T, TS>
{
    private void InitTables(MIPSEmulatorConfig config)
    {
        var version = config.MipsVersion;

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
    }

    private void InitRoot(MipsVersion version)
    {
        _opCodeTable[(int)OperationCode.Special] = &DispatchSpecial;
        _opCodeTable[(int)OperationCode.RegisterImmediate] = &DispatchRegImm;
        _opCodeTable[(int)OperationCode.Jump] = &Jump;
        _opCodeTable[(int)OperationCode.JumpAndLink] = &JumpLink;
        _opCodeTable[(int)OperationCode.BranchOnEquals] = &BranchOn<XeqLogic<T>>;
        _opCodeTable[(int)OperationCode.BranchOnNotEquals] = &BranchOn<XneLogic<T>>;
        _opCodeTable[(int)OperationCode.BranchOnLessThanOrEqualToZero] = &BranchOn<XlezLogic<T, TS>>;
        _opCodeTable[(int)OperationCode.BranchOnGreaterThanZero] = &BranchOn<XgtzLogic<T, TS>>;
        _opCodeTable[(int)OperationCode.AddImmediate] = &CheckedAluI<AddLogic<uint, int>, uint, int>;
        _opCodeTable[(int)OperationCode.AddImmediateUnsigned] = &AluI<AdduLogic<uint>, uint>;
        _opCodeTable[(int)OperationCode.SetLessThanImmediate] = &AluISigned<SltLogic<T, TS>, T, TS>;
        _opCodeTable[(int)OperationCode.SetLessThanImmediateUnsigned] = &AluI<SltuLogic<T>, T>;
        _opCodeTable[(int)OperationCode.AndImmediate] = &AluI<AndLogic<T>, T>;
        _opCodeTable[(int)OperationCode.OrImmediate] = &AluI<OrLogic<T>, T>;
        _opCodeTable[(int)OperationCode.ExclusiveOrImmediate] = &AluI<XorLogic<T>, T>;
        _opCodeTable[(int)OperationCode.LoadUpperImmediate] = &Lui;
        _opCodeTable[(int)OperationCode.Coprocessor0] = &CreateCoProc0Execution;
        _opCodeTable[(int)OperationCode.Coprocessor1] = &CreateCoProc1Execution;
        _opCodeTable[(int)OperationCode.Coprocessor2] = &NotImplemented; // TODO
        _opCodeTable[(int)OperationCode.LoadByte] = &Load<sbyte>;
        _opCodeTable[(int)OperationCode.LoadHalfWord] = &Load<short>;
        _opCodeTable[(int)OperationCode.LoadWord] = &Load<int>;
        _opCodeTable[(int)OperationCode.LoadByteUnsigned] = &Load<byte>;
        _opCodeTable[(int)OperationCode.LoadHalfWordUnsigned] = &Load<ushort>;
        _opCodeTable[(int)OperationCode.StoreByte] = &Store<sbyte>;
        _opCodeTable[(int)OperationCode.StoreHalfWord] = &Store<short>;
        _opCodeTable[(int)OperationCode.StoreWord] = &Store<int>;
        _opCodeTable[(int)OperationCode.LoadWordCoprocessor1] = &NotImplemented; // TODO
        _opCodeTable[(int)OperationCode.StoreWordCoprocessor1] = &NotImplemented; // TODO

        if (version is < MipsVersion.MipsIII)
        {
            _opCodeTable[(int)OperationCode.Coprocessor3] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.LoadWordCoprocessor3] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.StoreWordCoprocessor3] = &NotImplemented; // TODO

            if (version is >= MipsVersion.MipsII)
            {
                _opCodeTable[(int)OperationCode.LoadDoubleWordCoprocessor3] = &NotImplemented; // TODO
                _opCodeTable[(int)OperationCode.StoreDoubleWordCoprocessor3] = &NotImplemented; // TODO
            }
        }
        
        if (version is >= MipsVersion.MipsIII && version.Is64Bit())
        {
            _opCodeTable[(int)OperationCode.DoubleWordAddImmediate] = &CheckedAluI<AddLogic<ulong, long>, ulong, long>;
            _opCodeTable[(int)OperationCode.DoubleWordAddImmediateUnsigned] = &AluI<AdduLogic<ulong>, ulong>;
            _opCodeTable[(int)OperationCode.LoadDoubleWordLeft] = &NotImplemented;
            _opCodeTable[(int)OperationCode.LoadDoubleWordRight] = &NotImplemented;
            _opCodeTable[(int)OperationCode.LoadDoubleWord] = &Load<long>;
            _opCodeTable[(int)OperationCode.StoreDoubleWord] = &Store<long>;
        }

        if (version is < MipsVersion.Mips_R6)
        {
            _opCodeTable[(int)OperationCode.LoadWordLeft] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.LoadWordRight] = &NotImplemented;
            _opCodeTable[(int)OperationCode.StoreWordLeft] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.StoreWordRight] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.LoadWordCoprocessor2] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.StoreWordCoprocessor2] = &NotImplemented; // TODO
        }

        if (version is >= MipsVersion.MipsII)
        {
            _opCodeTable[(int)OperationCode.LoadLinkedWord] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.StoreConditionalWord] = &NotImplemented; // TODO
        }

        if (version is >= MipsVersion.MipsII and < MipsVersion.Mips_R6)
        {
            _opCodeTable[(int)OperationCode.BranchOnEqualLikely] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.BranchOnNotEqualLikely] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.BranchOnLessThanOrEqualToZeroLikely] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.BranchOnGreaterThanZeroLikely] = &NotImplemented; // TODO

            _opCodeTable[(int)OperationCode.LoadDoubleWordCoprocessor1] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.LoadDoubleWordCoprocessor2] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.StoreDoubleWordCoprocessor1] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.StoreDoubleWordCoprocessor2] = &NotImplemented; // TODO
        }

        if (version is >= MipsVersion.Mips_R1 and < MipsVersion.Mips_R6)
        {
            InitSpecial2();
            _opCodeTable[(int)OperationCode.Special2] = &DispatchSpecial2;
        }

        if (version is >= MipsVersion.Mips_R6)
        {
            _opCodeTable[(int)OperationCode.BranchCompact] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.BranchAndLinkCompact] = &NotImplemented; // TODO
        }
    }

    private void InitSpecial(MipsVersion version)
    {
        _specialTable[(int)FunctionCode.ShiftLeftLogical] = &Shift<SllLogic<uint>, uint>;
        _specialTable[(int)FunctionCode.ShiftRightLogical] = &Shift<SrlLogic<uint>, uint>;
        _specialTable[(int)FunctionCode.ShiftRightArithmetic] = &Shift<SraLogic<uint, int>, uint>;
        _specialTable[(int)FunctionCode.ShiftLeftLogicalVariable] = &ShiftVar<SllLogic<uint>, uint>;
        _specialTable[(int)FunctionCode.ShiftRightLogicalVariable] = &ShiftVar<SrlLogic<uint>, uint>;
        _specialTable[(int)FunctionCode.ShiftRightArithmeticVariable] = &ShiftVar<SraLogic<uint, int>, uint>;
        _specialTable[(int)FunctionCode.JumpAndLinkRegister] = &JumpLinkR;
        _specialTable[(int)FunctionCode.SystemCall] = &Trap<SyscallLogic>;
        _specialTable[(int)FunctionCode.Break] = &Trap<BreakLogic>;
        _specialTable[(int)FunctionCode.Add] = &CheckedAluR<AddLogic<uint, int>, uint, int>;
        _specialTable[(int)FunctionCode.AddUnsigned] = &AluR<AdduLogic<uint>, uint>;
        _specialTable[(int)FunctionCode.Subtract] = &CheckedAluR<SubLogic<uint, int>, uint, int>;
        _specialTable[(int)FunctionCode.SubtractUnsigned] = &AluR<SubuLogic<uint>, uint>;
        _specialTable[(int)FunctionCode.And] = &AluR<AndLogic<T>, T>;
        _specialTable[(int)FunctionCode.Or] = &AluR<OrLogic<T>, T>;
        _specialTable[(int)FunctionCode.ExclusiveOr] = &AluR<XorLogic<T>, T>;
        _specialTable[(int)FunctionCode.Nor] = &AluR<NorLogic<T>, T>;
        _specialTable[(int)FunctionCode.SetLessThan] = &AluR<SltLogic<T, TS>, T>;
        _specialTable[(int)FunctionCode.SetLessThanUnsigned] = &AluR<SltuLogic<T>, T>;

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
            _specialTable[(int)FunctionCode.DoubleWordShiftRightArithmeticVariable] = &ShiftVar<SraLogic<ulong, long>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordMultiply] = &MultR<MultLogic<ulong, long, UInt128>, ulong, UInt128>;
            _specialTable[(int)FunctionCode.DoubleWordMultiplyUnsigned] = &MultR<MultuLogic<ulong, UInt128>, ulong, UInt128>;
            _specialTable[(int)FunctionCode.DoubleWordDivide] = &DivR<DivLogic<ulong, long>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordDivideUnsigned] = &DivR<DivuLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordAdd] = &AluR<AddLogic<ulong, long>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordAddUnsigned] = &AluR<AdduLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordSubtract] = &AluR<SubLogic<ulong, long>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordSubtractUnsigned] = &AluR<SubuLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftLeftLogical] = &Shift<SllLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightLogical] = &Shift<SrlLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightArithmetic] = &Shift<SraLogic<ulong, long>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftLeftLogicalPlus32] = &ShiftPlus32<SllLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightLogicalPlus32] = &ShiftPlus32<SrlLogic<ulong>, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightArithmeticPlus32] = &ShiftPlus32<SraLogic<ulong, long>, ulong>;
        }

        if (version is >= MipsVersion.MipsIV and < MipsVersion.Mips_R6)
        {
            _specialTable[(int)FunctionCode.MoveOnZero] = &Move<Xeqz<T>>;
            _specialTable[(int)FunctionCode.MoveOnNotZero] = &Move<Xnez<T>>;
        }

        if (version is < MipsVersion.Mips_R6)
        {
            _specialTable[(int)FunctionCode.JumpRegister] = &JumpR;
            _specialTable[(int)FunctionCode.Multiply] = &MultR<MultLogic<uint, int, ulong>, uint, ulong>;
            _specialTable[(int)FunctionCode.MultiplyUnsigned] = &MultR<MultuLogic<uint, ulong>, uint, ulong>;
            _specialTable[(int)FunctionCode.Divide] = &DivR<DivLogic<uint, int>, uint>;
            _specialTable[(int)FunctionCode.DivideUnsigned] = &DivR<DivuLogic<uint>, uint>;
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
            _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZeroLikely] = &NotImplemented; // TODO
            _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely] = &NotImplemented; // TODO
            _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZeroLikelyAndLink] = &NotImplemented; // TODO
            _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink] = &NotImplemented; // TODO
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
        _special2Table[(int)Func2Code.MultiplyToGPR] = &AluR<MulLogic<uint, int>, uint>;
        _special2Table[(int)Func2Code.MultiplyAndAddHiLow] = &MultAddR<MultAddLogic<uint, int, ulong>, uint, ulong>;
        _special2Table[(int)Func2Code.MultiplyAndAddHiLowUnsigned] = &MultAddR<MultAdduLogic<uint, ulong>, uint, ulong>;
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLow] = &MultAddR<MultSubLogic<uint, int, ulong>, uint, ulong>;
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLowUnsigned] = &MultAddR<MultSubuLogic<uint, ulong>, uint, ulong>;
        _special2Table[(int)Func2Code.CountLeadingZeros] = &AluR<ClzLogic<uint>, uint>;
        _special2Table[(int)Func2Code.CountLeadingOnes] = &AluR<CloLogic<uint>, uint>;
    }
}
