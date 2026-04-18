// Avishai Dernis 2026

using System;
using Zarem.Emulator.Config;
using Zarem.Extensions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.SpecialFunctions;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;

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
            _coProc1RSTable[i] = &ReservedInstruction;
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
        _opCodeTable[(int)MipsOpCode.AddImmediate] = &CheckedAluI<AddLogic<uint, int>, uint, int>;
        _opCodeTable[(int)MipsOpCode.AddImmediateUnsigned] = &AluI<AdduLogic<uint>, uint>;
        _opCodeTable[(int)MipsOpCode.SetLessThanImmediate] = &AluISigned<SltLogic<T, TS>, T, TS>;
        _opCodeTable[(int)MipsOpCode.SetLessThanImmediateUnsigned] = &AluI<SltuLogic<T>, T>;
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
            _opCodeTable[(int)MipsOpCode.DoubleWordAddImmediate] = &CheckedAluI<AddLogic<ulong, long>, ulong, long>;
            _opCodeTable[(int)MipsOpCode.DoubleWordAddImmediateUnsigned] = &AluI<AdduLogic<ulong>, ulong>;
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
            _specialTable[(int)FunctionCode.MoveOnZero] = &Move<XeqzLogic<T>>;
            _specialTable[(int)FunctionCode.MoveOnNotZero] = &Move<XnezLogic<T>>;
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
        _special2Table[(int)Func2Code.MultiplyToGPR] = &AluR<MulLogic<uint, int>, uint>;
        _special2Table[(int)Func2Code.MultiplyAndAddHiLow] = &MultAddR<MultAddLogic<uint, int, ulong>, uint, ulong>;
        _special2Table[(int)Func2Code.MultiplyAndAddHiLowUnsigned] = &MultAddR<MultAdduLogic<uint, ulong>, uint, ulong>;
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLow] = &MultAddR<MultSubLogic<uint, int, ulong>, uint, ulong>;
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLowUnsigned] = &MultAddR<MultSubuLogic<uint, ulong>, uint, ulong>;
        _special2Table[(int)Func2Code.CountLeadingZeros] = &AluR<ClzLogic<uint>, uint>;
        _special2Table[(int)Func2Code.CountLeadingOnes] = &AluR<CloLogic<uint>, uint>;
    }

    private void InitFloat(MipsVersion version)
    {
        _coProc1RSTable[(int)CoProc1RSCode.MFC1] = &MFC1;
        _coProc1RSTable[(int)CoProc1RSCode.MTC1] = &MTC1;
        _coProc1RSTable[(int)CoProc1RSCode.Single] = &CreateFloatExecution<float>;
        _coProc1RSTable[(int)CoProc1RSCode.Double] = &CreateFloatExecution<double>;
        _coProc1RSTable[(int)CoProc1RSCode.Word] = &CreateFloatIntExecution<int>;
        _coProc1RSTable[(int)CoProc1RSCode.Long] = &CreateFloatIntExecution<long>;
    }
}
