// Avishai Dernis 2026

using Zarem.Emulator.Config;
using Zarem.Extensions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Emulator.Models;

public unsafe partial class InstructionServiceTable<T, TSigned>
{
    private void InitTables(MIPSEmulatorConfig config)
    {
        var version = config.MipsVersion;

        // Set default behavior to reserve instruction trap
        for (int i = 0; i < 64; i++)
        {
            _opCodeTable[i] = &Reserved;
            _specialTable[i] = &Reserved;
            _special2Table[i] = &Reserved;
        }

        for (int i = 0; i < 32; i++)
        {
            _regImmTable[i] = &Reserved;
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
        _opCodeTable[(int)OperationCode.BranchOnEquals] = &BranchOn<XeqLogic>;
        _opCodeTable[(int)OperationCode.BranchOnNotEquals] = &BranchOn<XneLogic>;
        _opCodeTable[(int)OperationCode.BranchOnLessThanOrEqualToZero] = &BranchOn<XlezLogic>;
        _opCodeTable[(int)OperationCode.BranchOnGreaterThanZero] = &BranchOn<XgtzLogic>;
        _opCodeTable[(int)OperationCode.AddImmediate] = &CheckedAluI<AddLogic32, uint, int>;
        _opCodeTable[(int)OperationCode.AddImmediateUnsigned] = &AluI<AdduLogic32, uint>;
        _opCodeTable[(int)OperationCode.SetLessThanImmediate] = &AluISigned<SltLogic, T, TSigned>;
        _opCodeTable[(int)OperationCode.SetLessThanImmediateUnsigned] = &AluI<SltuLogic, T>;
        _opCodeTable[(int)OperationCode.AndImmediate] = &AluI<AndLogic, T>;
        _opCodeTable[(int)OperationCode.OrImmediate] = &AluI<OrLogic, T>;
        _opCodeTable[(int)OperationCode.ExclusiveOrImmediate] = &AluI<XorLogic, T>;
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
            _opCodeTable[(int)OperationCode.DoubleWordAddImmediate] = &CheckedAluI<AddLogic64, ulong, long>;
            _opCodeTable[(int)OperationCode.DoubleWordAddImmediateUnsigned] = &AluI<AddLogic64, ulong>;
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
        _specialTable[(int)FunctionCode.ShiftLeftLogical] = &Shift<SllLogic32, uint>;
        _specialTable[(int)FunctionCode.ShiftRightLogical] = &Shift<SrlLogic32, uint>;
        _specialTable[(int)FunctionCode.ShiftRightArithmetic] = &Shift<SraLogic32, uint>;
        _specialTable[(int)FunctionCode.ShiftLeftLogicalVariable] = &ShiftVar<SllLogic32, uint>;
        _specialTable[(int)FunctionCode.ShiftRightLogicalVariable] = &ShiftVar<SrlLogic32, uint>;
        _specialTable[(int)FunctionCode.ShiftRightArithmeticVariable] = &ShiftVar<SraLogic32, uint>;
        _specialTable[(int)FunctionCode.JumpAndLinkRegister] = &JumpLinkR;
        _specialTable[(int)FunctionCode.SystemCall] = &Trap<SyscallLogic>;
        _specialTable[(int)FunctionCode.Break] = &Trap<BreakLogic>;
        _specialTable[(int)FunctionCode.Add] = &CheckedAluR<AddLogic32, uint, int>;
        _specialTable[(int)FunctionCode.AddUnsigned] = &AluR<AdduLogic32, uint>;
        _specialTable[(int)FunctionCode.Subtract] = &CheckedAluR<SubLogic32, uint, int>;
        _specialTable[(int)FunctionCode.SubtractUnsigned] = &AluR<SubuLogic32, uint>;
        _specialTable[(int)FunctionCode.And] = &AluR<AndLogic, T>;
        _specialTable[(int)FunctionCode.Or] = &AluR<OrLogic, T>;
        _specialTable[(int)FunctionCode.ExclusiveOr] = &AluR<XorLogic, T>;
        _specialTable[(int)FunctionCode.Nor] = &AluR<NorLogic,T>;
        _specialTable[(int)FunctionCode.SetLessThan] = &AluR<SltLogic, T>;
        _specialTable[(int)FunctionCode.SetLessThanUnsigned] = &AluR<SltuLogic, T>;

        if (version is >= MipsVersion.MipsII)
        {
            _specialTable[(int)FunctionCode.Sync] = &NotImplemented; // TODO
            _specialTable[(int)FunctionCode.TrapOnGreaterOrEqual] = &TrapOn<XgeLogic>;
            _specialTable[(int)FunctionCode.TrapOnGreaterOrEqualUnsigned] = &TrapOn<XgeuLogic>;
            _specialTable[(int)FunctionCode.TrapOnLessThan] = &TrapOn<XltLogic>;
            _specialTable[(int)FunctionCode.TrapOnLessThanUnsigned] = &TrapOn<XltuLogic>;
            _specialTable[(int)FunctionCode.TrapOnEquals] = &TrapOn<XeqLogic>;
            _specialTable[(int)FunctionCode.TrapOnNotEquals] = &TrapOn<XneLogic>;
        }

        if (version is >= MipsVersion.MipsIII && version.Is64Bit())
        {
            _specialTable[(int)FunctionCode.DoubleWordShiftLeftLogicalVariable] = &ShiftVar<SllLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightLogicalVariable] = &ShiftVar<SrlLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightArithmeticVariable] = &ShiftVar<SraLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordMultiply] = &MultR<MultLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordMultiplyUnsigned] = &MultR<MultuLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordDivide] = &DivR<DivLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordDivideUnsigned] = &DivR<DivuLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordAdd] = &AluR<AddLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordAddUnsigned] = &AluR<AdduLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordSubtract] = &AluR<SubLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordSubtractUnsigned] = &AluR<SubuLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftLeftLogical] = &Shift<SllLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightLogical] = &Shift<SrlLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightArithmetic] = &Shift<SraLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftLeftLogicalPlus32] = &ShiftPlus32<SllLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightLogicalPlus32] = &ShiftPlus32<SrlLogic64, ulong>;
            _specialTable[(int)FunctionCode.DoubleWordShiftRightArithmeticPlus32] = &ShiftPlus32<SraLogic64, ulong>;
        }

        if (version is >= MipsVersion.MipsIV and < MipsVersion.Mips_R6)
        {
            _specialTable[(int)FunctionCode.MoveOnZero] = &Move<Xeqz>;
            _specialTable[(int)FunctionCode.MoveOnNotZero] = &Move<Xnez>;
        }

        if (version is < MipsVersion.Mips_R6)
        {
            _specialTable[(int)FunctionCode.JumpRegister] = &JumpR;
            _specialTable[(int)FunctionCode.Multiply] = &MultR<MultLogic32, uint>;
            _specialTable[(int)FunctionCode.MultiplyUnsigned] = &MultR<MultuLogic32, uint>;
            _specialTable[(int)FunctionCode.Divide] = &DivR<DivLogic32, uint>;
            _specialTable[(int)FunctionCode.DivideUnsigned] = &DivR<DivuLogic32, uint>;
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
        _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZero] = &BranchOn<XltzLogic>;
        _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZero] = &BranchOn<XgezLogic>;


        if (version is >= MipsVersion.MipsII and < MipsVersion.Mips_R6)
        {
            _regImmTable[(int)RegImmFuncCode.TrapOnGreaterOrEqualImmediate] = &TrapOnI<XgeLogic>;
            _regImmTable[(int)RegImmFuncCode.TrapOnGreaterOrEqualImmediateUnsigned] = &TrapOnI<XgeuLogic>;
            _regImmTable[(int)RegImmFuncCode.TrapOnLessThanImmediate] = &TrapOnI<XltLogic>;
            _regImmTable[(int)RegImmFuncCode.TrapOnLessThanImmediateUnsigned] = &TrapOnI<XltuLogic>;
            _regImmTable[(int)RegImmFuncCode.TrapOnEqualsImmediate] = &TrapOnI<XeqLogic>;
            _regImmTable[(int)RegImmFuncCode.TrapOnNotEqualsImmediate] = &TrapOnI<XneLogic>;
            _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZeroLikely] = &NotImplemented; // TODO
            _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely] = &NotImplemented; // TODO
            _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZeroLikelyAndLink] = &NotImplemented; // TODO
            _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink] = &NotImplemented; // TODO
        }

        if (version is < MipsVersion.Mips_R6)
        {
            _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZeroAndLink] = &BranchLinkOn<XltzLogic>;
            _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroAndLink] = &BranchLinkOn<XgezLogic>;
        }

        if (version >= MipsVersion.Mips_R6)
        {
            _regImmTable[(int)RegImmFuncCode.NoOpAndLink] = &NotImplemented; // TODO
            _regImmTable[(int)RegImmFuncCode.BranchAndLink] = &NotImplemented; // TODO
        }
    }

    private void InitSpecial2()
    {
        _special2Table[(int)Func2Code.MultiplyToGPR] = &AluR<MulLogic32, uint>;
        _special2Table[(int)Func2Code.MultiplyAndAddHiLow] = &MultAddR<MultAddLogic32, uint>;
        _special2Table[(int)Func2Code.MultiplyAndAddHiLowUnsigned] = &MultAddR<MultAddLogic32, uint>;
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLow] = &MultAddR<MultSubLogic32, uint>;
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLowUnsigned] = &MultAddR<MultSubuLogic32, uint>;
        _special2Table[(int)Func2Code.CountLeadingZeros] = &AluR<ClzLogic32, uint>;
        _special2Table[(int)Func2Code.CountLeadingOnes] = &AluR<CloLogic32, uint>;
    }
}
