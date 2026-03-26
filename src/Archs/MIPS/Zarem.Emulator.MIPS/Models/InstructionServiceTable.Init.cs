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
        InitRoot(config);
        InitSpecial(config);
        InitRegImm(config);
    }

    private void InitRoot(MIPSEmulatorConfig config)
    {
        // Special and RegImmediate (R and B types)
        _opCodeTable[(int)OperationCode.Special] = &DispatchSpecial;
        _opCodeTable[(int)OperationCode.RegisterImmediate] = &DispatchRegImm;

        // Jump (J-Type)
        _opCodeTable[(int)OperationCode.Jump] = &Jump;
        _opCodeTable[(int)OperationCode.JumpAndLink] = &JumpLink;

        // Branch
        _opCodeTable[(int)OperationCode.BranchOnEquals] =
        _opCodeTable[(int)OperationCode.BranchOnEqualLikely] = &BranchOn<XeqLogic>;
        _opCodeTable[(int)OperationCode.BranchOnNotEquals] =
        _opCodeTable[(int)OperationCode.BranchOnNotEqualLikely] = &BranchOn<XneLogic>;
        _opCodeTable[(int)OperationCode.BranchOnLessThanOrEqualToZero] =
        _opCodeTable[(int)OperationCode.BranchOnLessThanOrEqualToZeroLikely] = &BranchOn<XlezLogic>;
        _opCodeTable[(int)OperationCode.BranchOnGreaterThanZero] =
        _opCodeTable[(int)OperationCode.BranchOnGreaterThanZeroLikely] = &BranchOn<XgtzLogic>;

        // Arithmetic
        _opCodeTable[(int)OperationCode.AddImmediate] = &CheckedAluI<AddLogic>;
        _opCodeTable[(int)OperationCode.AddImmediateUnsigned] = &AluI<AdduLogic>;

        // Compare
        _opCodeTable[(int)OperationCode.SetLessThanImmediate] = &AluISigned<SltLogic>;
        _opCodeTable[(int)OperationCode.SetLessThanImmediateUnsigned] = &AluI<SltuLogic>;

        // Logical
        _opCodeTable[(int)OperationCode.AndImmediate] = &AluI<AndLogic>;
        _opCodeTable[(int)OperationCode.OrImmediate] = &AluI<OrLogic>;
        _opCodeTable[(int)OperationCode.ExclusiveOrImmediate] = &AluI<XorLogic>;

        // Load Upper Immediate
        _opCodeTable[(int)OperationCode.LoadUpperImmediate] = &Lui;

        // CoProcessor Instructions
        _opCodeTable[(int)OperationCode.Coprocessor0] = &CreateCoProc0Execution;
        _opCodeTable[(int)OperationCode.Coprocessor1] = &CreateCoProc1Execution;

        // Trap
        _opCodeTable[(int)OperationCode.Trap] = &Trap<TrapLogic>;

        // Initialize sub tables
        if (config.MipsVersion is <= MipsVersion.MipsV)
        {
            InitSpecial2(config);
            _opCodeTable[(int)OperationCode.Special2] = &DispatchSpecial2;
        }

        // Load
        _opCodeTable[(int)OperationCode.LoadByte] = &Load<sbyte>;
        _opCodeTable[(int)OperationCode.LoadHalfWord] = &Load<short>;
        //_opCodeTable[(int)OperationCode.LoadWordLeft] = TODO:
        _opCodeTable[(int)OperationCode.LoadWord] = &Load<int>;
        _opCodeTable[(int)OperationCode.LoadByteUnsigned] = &Load<byte>;
        _opCodeTable[(int)OperationCode.LoadHalfWordUnsigned] = &Load<ushort>;
        //_opCodeTable[(int)OperationCode.LoadWordRight] = TODO:

        // Store
        _opCodeTable[(int)OperationCode.StoreByte] = &Store<sbyte>;
        _opCodeTable[(int)OperationCode.StoreHalfWord] = &Store<short>;
        //_opCodeTable[(int)OperationCode.StoreWordLeft] = TODO:
        _opCodeTable[(int)OperationCode.StoreWord] = &Store<int>;
        //_opCodeTable[(int)OperationCode.StoreWordRight] = TODO:
    }

    private void InitSpecial(MIPSEmulatorConfig config)
    {
        // Shift
        _specialTable[(int)FunctionCode.ShiftLeftLogical] = &Shift<SllLogic>;
        _specialTable[(int)FunctionCode.ShiftRightLogical] = &Shift<SrlLogic>;
        _specialTable[(int)FunctionCode.ShiftRightArithmetic] = &Shift<SraLogic>;
        _specialTable[(int)FunctionCode.ShiftLeftLogicalVariable] = &ShiftVar<SllLogic>;
        _specialTable[(int)FunctionCode.ShiftRightLogicalVariable] = &ShiftVar<SrlLogic>;
        _specialTable[(int)FunctionCode.ShiftRightArithmeticVariable] = &ShiftVar<SraLogic>;

        // Jump Register
        _specialTable[(int)FunctionCode.JumpRegister] = &JumpR;
        _specialTable[(int)FunctionCode.JumpAndLinkRegister] = &JumpLinkR;

        // System
        _specialTable[(int)FunctionCode.SystemCall] = &Trap<SyscallLogic>;
        _specialTable[(int)FunctionCode.Break] = &Trap<BreakLogic>;
        //_specialTable[(int)FunctionCode.Sync] = NotImplemented;

        if (config.MipsVersion is <= MipsVersion.MipsV)
        {
            _specialTable[(int)FunctionCode.MoveOnZero] = &Move<MovzLogic>;
            _specialTable[(int)FunctionCode.MoveOnNotZero] = &Move<MovnLogic>;
        }

        // Hi/Low
        _specialTable[(int)FunctionCode.MoveFromHigh] = &Mfhi;
        _specialTable[(int)FunctionCode.MoveToHigh] = &Mthi;
        _specialTable[(int)FunctionCode.MoveFromLow] = &Mflo;
        _specialTable[(int)FunctionCode.MoveToLow] = &Mtlo;

        // Arithmetic
        _specialTable[(int)FunctionCode.Multiply] = &MultR<MultLogic>;
        _specialTable[(int)FunctionCode.MultiplyUnsigned] = &MultR<MultuLogic>;
        _specialTable[(int)FunctionCode.Divide] = &DivR<DivLogic>;
        _specialTable[(int)FunctionCode.DivideUnsigned] = &DivR<DivuLogic>;
        _specialTable[(int)FunctionCode.Add] = &CheckedAluR<AddLogic>;
        _specialTable[(int)FunctionCode.AddUnsigned] = &AluR<AdduLogic>;
        _specialTable[(int)FunctionCode.Subtract] = &CheckedAluR<SubLogic>;
        _specialTable[(int)FunctionCode.SubtractUnsigned] = &AluR<SubuLogic>;

        // Logical
        _specialTable[(int)FunctionCode.And] = &AluR<AndLogic>;
        _specialTable[(int)FunctionCode.Or] = &AluR<OrLogic>;
        _specialTable[(int)FunctionCode.ExclusiveOr] = &AluR<XorLogic>;
        _specialTable[(int)FunctionCode.Nor] = &AluR<NorLogic>;

        // Compare
        _specialTable[(int)FunctionCode.SetLessThan] = &AluR<SltLogic>;
        _specialTable[(int)FunctionCode.SetLessThanUnsigned] = &AluR<SltuLogic>;

        if (config.MipsVersion is >= MipsVersion.MipsII)
        {
            // Trap
            _specialTable[(int)FunctionCode.TrapOnGreaterOrEqual] = &TrapOn<XgeLogic>;
            _specialTable[(int)FunctionCode.TrapOnGreaterOrEqualUnsigned] = &TrapOn<XgeuLogic>;
            _specialTable[(int)FunctionCode.TrapOnLessThan] = &TrapOn<XltLogic>;
            _specialTable[(int)FunctionCode.TrapOnLessThanUnsigned] = &TrapOn<XltuLogic>;
            _specialTable[(int)FunctionCode.TrapOnEquals] = &TrapOn<XeqLogic>;
            _specialTable[(int)FunctionCode.TrapOnNotEquals] = &TrapOn<XneLogic>;
        }
    }

    private void InitRegImm(MIPSEmulatorConfig config)
    {
        // Branch
        _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZero] = 
        _regImmTable[(int)RegImmFuncCode.BranchOnLessThanZeroLikely] = &BranchOn<XltzLogic>;
        _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZero] = 
        _regImmTable[(int)RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely] = &BranchOn<XgezLogic>;

        // Trap
        _regImmTable[(int)RegImmFuncCode.TrapOnGreaterOrEqualImmediate] = &TrapOnI<XgeLogic>;
        _regImmTable[(int)RegImmFuncCode.TrapOnGreaterOrEqualImmediateUnsigned] = &TrapOnI<XgeuLogic>;
        _regImmTable[(int)RegImmFuncCode.TrapOnLessThanImmediate] = &TrapOnI<XltLogic>;
        _regImmTable[(int)RegImmFuncCode.TrapOnLessThanImmediateUnsigned] = &TrapOnI<XltuLogic>;
        _regImmTable[(int)RegImmFuncCode.TrapOnEqualsImmediate] = &TrapOnI<XeqLogic>;
        _regImmTable[(int)RegImmFuncCode.TrapOnNotEqualsImmediate] = &TrapOnI<XneLogic>;
    }

    private void InitSpecial2(MIPSEmulatorConfig config)
    {
        // Multiply
        _special2Table[(int)Func2Code.MultiplyToGPR] = &AluR<MulLogic>;
        _special2Table[(int)Func2Code.MultiplyAndAddHiLow] = &MultAddR<MultAddLogic>;
        _special2Table[(int)Func2Code.MultiplyAndAddHiLowUnsigned] = &MultAddR<MultAddLogic>;
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLow] = &MultAddR<MultSubLogic>;
        _special2Table[(int)Func2Code.MultiplyAndSubtractHiLowUnsigned] = &MultAddR<MultSubuLogic>;

        // Bit Counting
        _special2Table[(int)Func2Code.CountLeadingZeros] = &AluR<ClzLogic>;
        _special2Table[(int)Func2Code.CountLeadingOnes] = &AluR<CloLogic>;
    }
}
