// Avishai Dernis 2026

using System;
using Zarem.Emulator.Executor.Enum;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;
using System.ComponentModel;
using Zarem.Models.Instructions.Enums;

namespace Zarem.Emulator.Executor;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="Execution"/> models.
/// </summary>
public partial struct InstructionServiceTable
{
    private readonly void Initialize(MIPSEmulatorConfig config)
    {
        InitSpecial(config);

        _opCodeTable[(int)OperationCode.Special] = DispatchSpecial;
        _opCodeTable[(int)OperationCode.RegisterImmediate] = DispatchRegImm;

        if (config.MipsVersion is <= MipsVersion.MipsV)
        {
            _opCodeTable[(int)OperationCode.Special2] = DispatchSpecial2;
        }
    }

    private readonly void InitSpecial(MIPSEmulatorConfig config)
    {
        // Shift
        _specialTable[(int)FunctionCode.ShiftLeftLogical] = Shift<SllLogic>;
        _specialTable[(int)FunctionCode.ShiftRightLogical] = Shift<SrlLogic>;
        _specialTable[(int)FunctionCode.ShiftRightArithmetic] = Shift<SraLogic>;
        _specialTable[(int)FunctionCode.ShiftLeftLogicalVariable] = ShiftVar<SllLogic>;
        _specialTable[(int)FunctionCode.ShiftRightLogicalVariable] = ShiftVar<SrlLogic>;
        _specialTable[(int)FunctionCode.ShiftRightArithmeticVariable] = ShiftVar<SraLogic>;

        // Arithmetic
        _specialTable[(int)FunctionCode.Add] = CheckedAluR<AddLogic>;
        _specialTable[(int)FunctionCode.AddUnsigned] = AluR<AdduLogic>;
        _specialTable[(int)FunctionCode.Subtract] = CheckedAluR<SubLogic>;
        _specialTable[(int)FunctionCode.SubtractUnsigned] = AluR<SubuLogic>;
        _specialTable[(int)FunctionCode.Multiply] = MultR<MultLogic>;
        _specialTable[(int)FunctionCode.MultiplyUnsigned] = MultR<MultuLogic>;
        _specialTable[(int)FunctionCode.Divide] = DivR<DivLogic>;
        _specialTable[(int)FunctionCode.DivideUnsigned] = DivR<DivuLogic>;

        // Logical
        _specialTable[(int)FunctionCode.And] = AluR<AndLogic>;
        _specialTable[(int)FunctionCode.Or] = AluR<OrLogic>;
        _specialTable[(int)FunctionCode.ExclusiveOr] = AluR<XorLogic>;
        _specialTable[(int)FunctionCode.Nor] = AluR<NorLogic>;

        // Compare
        _specialTable[(int)FunctionCode.SetLessThan] = AluR<SltLogic>;
        _specialTable[(int)FunctionCode.SetLessThanUnsigned] = AluR<SltuLogic>;

        // Jump Register
        _specialTable[(int)FunctionCode.SetLessThan] = JumpR;
        _specialTable[(int)FunctionCode.SetLessThanUnsigned] = JumpLinkR;

        // System
        _specialTable[(int)FunctionCode.SystemCall] = Trap<SyscallLogic>;
        _specialTable[(int)FunctionCode.Break] = Trap<BreakLogic>;
        //_specialTable[(int)FunctionCode.Sync] = NotImplemented;

        // Hi/Low
        _specialTable[(int)FunctionCode.MoveFromHigh] = Mfhi;
        _specialTable[(int)FunctionCode.MoveToHigh] = Mthi;
        _specialTable[(int)FunctionCode.MoveFromLow] = Mflo;
        _specialTable[(int)FunctionCode.MoveToLow] = Mtlo;

        // Trap
        _specialTable[(int)FunctionCode.TrapOnGreaterOrEqual] = TrapOn<XgeLogic>;
        _specialTable[(int)FunctionCode.TrapOnGreaterOrEqualUnsigned] = TrapOn<XgeuLogic>;
        _specialTable[(int)FunctionCode.TrapOnLessThan] = TrapOn<XltLogic>;
        _specialTable[(int)FunctionCode.TrapOnLessThanUnsigned] = TrapOn<XltuLogic>;
        _specialTable[(int)FunctionCode.TrapOnEquals] = TrapOn<XeqLogic>;
        _specialTable[(int)FunctionCode.TrapOnNotEquals] = TrapOn<XneLogic>;
    }

    private readonly void InitSpecial2(MIPSEmulatorConfig config)
    {
        // Shift
        _specialTable[(int)FunctionCode.ShiftLeftLogical] = Shift<SllLogic>;
        _specialTable[(int)FunctionCode.ShiftRightLogical] = Shift<SrlLogic>;
        _specialTable[(int)FunctionCode.ShiftRightArithmetic] = Shift<SraLogic>;
        _specialTable[(int)FunctionCode.ShiftLeftLogicalVariable] = ShiftVar<SllLogic>;
        _specialTable[(int)FunctionCode.ShiftRightLogicalVariable] = ShiftVar<SrlLogic>;
        _specialTable[(int)FunctionCode.ShiftRightArithmeticVariable] = ShiftVar<SraLogic>;

        // Arithmetic
        _specialTable[(int)FunctionCode.Add] = CheckedAluR<AddLogic>;
        _specialTable[(int)FunctionCode.AddUnsigned] = AluR<AdduLogic>;
        _specialTable[(int)FunctionCode.Subtract] = CheckedAluR<SubLogic>;
        _specialTable[(int)FunctionCode.SubtractUnsigned] = AluR<SubuLogic>;
        _specialTable[(int)FunctionCode.Multiply] = MultR<MultLogic>;
        _specialTable[(int)FunctionCode.MultiplyUnsigned] = MultR<MultuLogic>;
        _specialTable[(int)FunctionCode.Divide] = DivR<DivLogic>;
        _specialTable[(int)FunctionCode.DivideUnsigned] = DivR<DivuLogic>;

        // Logical
        _specialTable[(int)FunctionCode.And] = AluR<AndLogic>;
        _specialTable[(int)FunctionCode.Or] = AluR<OrLogic>;
        _specialTable[(int)FunctionCode.ExclusiveOr] = AluR<XorLogic>;
        _specialTable[(int)FunctionCode.Nor] = AluR<NorLogic>;

        // Compare
        _specialTable[(int)FunctionCode.SetLessThan] = AluR<SltLogic>;
        _specialTable[(int)FunctionCode.SetLessThanUnsigned] = AluR<SltuLogic>;

        // Jump Register
        _specialTable[(int)FunctionCode.SetLessThan] = JumpR;
        _specialTable[(int)FunctionCode.SetLessThanUnsigned] = JumpLinkR;

        // System
        _specialTable[(int)FunctionCode.SystemCall] = Trap<SyscallLogic>;
        _specialTable[(int)FunctionCode.Break] = Trap<BreakLogic>;
        //_specialTable[(int)FunctionCode.Sync] = NotImplemented;

        // Hi/Low
        _specialTable[(int)FunctionCode.MoveFromHigh] = Mfhi;
        _specialTable[(int)FunctionCode.MoveToHigh] = Mthi;
        _specialTable[(int)FunctionCode.MoveFromLow] = Mflo;
        _specialTable[(int)FunctionCode.MoveToLow] = Mtlo;

        // Trap
        _specialTable[(int)FunctionCode.TrapOnGreaterOrEqual] = TrapOn<XgeLogic>;
        _specialTable[(int)FunctionCode.TrapOnGreaterOrEqualUnsigned] = TrapOn<XgeuLogic>;
        _specialTable[(int)FunctionCode.TrapOnLessThan] = TrapOn<XltLogic>;
        _specialTable[(int)FunctionCode.TrapOnLessThanUnsigned] = TrapOn<XltuLogic>;
        _specialTable[(int)FunctionCode.TrapOnEquals] = TrapOn<XeqLogic>;
        _specialTable[(int)FunctionCode.TrapOnNotEquals] = TrapOn<XneLogic>;
    }
}
