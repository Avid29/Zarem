// Avishai Dernis 2026

using Zarem.Emulator.Config;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Emulator.Models;

public unsafe partial class InstructionServiceTable
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
        _opCodeTable[(int)OperationCode.AddImmediate] = &CheckedAluI<AddLogic>;
        _opCodeTable[(int)OperationCode.AddImmediateUnsigned] = &AluI<AdduLogic>;
        _opCodeTable[(int)OperationCode.SetLessThanImmediate] = &AluISigned<SltLogic>;
        _opCodeTable[(int)OperationCode.SetLessThanImmediateUnsigned] = &AluI<SltuLogic>;
        _opCodeTable[(int)OperationCode.AndImmediate] = &AluI<AndLogic>;
        _opCodeTable[(int)OperationCode.OrImmediate] = &AluI<OrLogic>;
        _opCodeTable[(int)OperationCode.ExclusiveOrImmediate] = &AluI<XorLogic>;
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

        if (version is < MipsVersion.Mips32R6)
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
            _opCodeTable[(int)OperationCode.Trap] = &Trap<TrapLogic>;
            _opCodeTable[(int)OperationCode.LoadLinkedWord] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.StoreConditionalWord] = &NotImplemented; // TODO
        }

        if (version is >= MipsVersion.MipsII and < MipsVersion.Mips32R6)
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

        if (version is >= MipsVersion.Mips32R1 and < MipsVersion.Mips32R6)
        {
            InitSpecial2();
            _opCodeTable[(int)OperationCode.Special2] = &DispatchSpecial2;
        }

        if (version is >= MipsVersion.Mips32R6)
        {
            _opCodeTable[(int)OperationCode.BranchCompact] = &NotImplemented; // TODO
            _opCodeTable[(int)OperationCode.BranchAndLinkCompact] = &NotImplemented; // TODO
        }
    }

    private void InitSpecial(MipsVersion version)
    {
        _specialTable[(int)FunctionCode.ShiftLeftLogical] = &Shift<SllLogic>;
        _specialTable[(int)FunctionCode.ShiftRightLogical] = &Shift<SrlLogic>;
        _specialTable[(int)FunctionCode.ShiftRightArithmetic] = &Shift<SraLogic>;
        _specialTable[(int)FunctionCode.ShiftLeftLogicalVariable] = &ShiftVar<SllLogic>;
        _specialTable[(int)FunctionCode.ShiftRightLogicalVariable] = &ShiftVar<SrlLogic>;
        _specialTable[(int)FunctionCode.ShiftRightArithmeticVariable] = &ShiftVar<SraLogic>;
        _specialTable[(int)FunctionCode.JumpRegister] = &JumpR;
        _specialTable[(int)FunctionCode.JumpAndLinkRegister] = &JumpLinkR;
        _specialTable[(int)FunctionCode.SystemCall] = &Trap<SyscallLogic>;
        _specialTable[(int)FunctionCode.Break] = &Trap<BreakLogic>;
        _specialTable[(int)FunctionCode.Add] = &CheckedAluR<AddLogic>;
        _specialTable[(int)FunctionCode.AddUnsigned] = &AluR<AdduLogic>;
        _specialTable[(int)FunctionCode.Subtract] = &CheckedAluR<SubLogic>;
        _specialTable[(int)FunctionCode.SubtractUnsigned] = &AluR<SubuLogic>;
        _specialTable[(int)FunctionCode.And] = &AluR<AndLogic>;
        _specialTable[(int)FunctionCode.Or] = &AluR<OrLogic>;
        _specialTable[(int)FunctionCode.ExclusiveOr] = &AluR<XorLogic>;
        _specialTable[(int)FunctionCode.Nor] = &AluR<NorLogic>;
        _specialTable[(int)FunctionCode.SetLessThan] = &AluR<SltLogic>;
        _specialTable[(int)FunctionCode.SetLessThanUnsigned] = &AluR<SltuLogic>;

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

        if (version is >= MipsVersion.MipsIV and < MipsVersion.Mips32R6)
        {
            _specialTable[(int)FunctionCode.MoveOnZero] = &Move<MovzLogic>;
            _specialTable[(int)FunctionCode.MoveOnNotZero] = &Move<MovnLogic>;
        }

        if (version is < MipsVersion.Mips32R6)
        {
            _specialTable[(int)FunctionCode.Multiply] = &MultR<MultLogic>;
            _specialTable[(int)FunctionCode.MultiplyUnsigned] = &MultR<MultuLogic>;
            _specialTable[(int)FunctionCode.Divide] = &DivR<DivLogic>;
            _specialTable[(int)FunctionCode.DivideUnsigned] = &DivR<DivuLogic>;
            _specialTable[(int)FunctionCode.MoveFromHigh] = &Mfhi;
            _specialTable[(int)FunctionCode.MoveToHigh] = &Mthi;
            _specialTable[(int)FunctionCode.MoveFromLow] = &Mflo;
            _specialTable[(int)FunctionCode.MoveToLow] = &Mtlo;
        }

        if (version is >= MipsVersion.Mips32R6)
        {
            _specialTable[(int)FunctionCode.SelectOnEquals] = &NotImplemented;
            _specialTable[(int)FunctionCode.SelectOnNotEquals] = &NotImplemented;
        }
    }

    private void InitRegImm(MipsVersion version)
    {
        _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZero] = &BranchOn<XltzLogic>;
        _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZero] = &BranchOn<XgezLogic>;


        if (version is >= MipsVersion.MipsII and < MipsVersion.Mips32R6)
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

        if (version is < MipsVersion.Mips32R6)
        {
            _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZeroAndLink] = &BranchLinkOn<XltzLogic>;
            _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroAndLink] = &BranchLinkOn<XgezLogic>;
        }

        if (version >= MipsVersion.Mips32R6)
        {
            _regImmTable[(int)RegImmFuncCode.NoOpAndLink] = &NotImplemented; // TODO
            _regImmTable[(int)RegImmFuncCode.BranchAndLink] = &NotImplemented; // TODO
        }
    }

    private void InitSpecial2()
    {
        _special2Table[(int)Func2Code.MultiplyToGPR] = &AluR<MulLogic>;
        _special2Table[(int)Func2Code.MultiplyAndAddHiLow] = &MultAddR<MultAddLogic>;
        _special2Table[(int)Func2Code.MultiplyAndAddHiLowUnsigned] = &MultAddR<MultAddLogic>;
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLow] = &MultAddR<MultSubLogic>;
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLowUnsigned] = &MultAddR<MultSubuLogic>;
        _special2Table[(int)Func2Code.CountLeadingZeros] = &AluR<ClzLogic>;
        _special2Table[(int)Func2Code.CountLeadingOnes] = &AluR<CloLogic>;
    }
}
