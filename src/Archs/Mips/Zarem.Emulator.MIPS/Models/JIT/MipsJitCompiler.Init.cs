// Avishai Dernis 2026

using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Config;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Emulator.JIT;

public partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private void InitTables(MipsEmulatorConfig config)
    {
        var version = config.Version;

        // Populate tables
        InitRoot(version);
        InitSpecial(version);
    }

    private void InitRoot(MipsVersion version)
    {
        _opCodeTable[(int)MipsOpCode.Special] = DispatchSpecial;
        _opCodeTable[(int)MipsOpCode.Jump] = (il, inst, pc) => Jump(il, inst, pc);
        _opCodeTable[(int)MipsOpCode.JumpAndLink] = (il, inst, pc) => Jump(il, inst, pc, link: true);
        _opCodeTable[(int)MipsOpCode.AddImmediateUnsigned] = (il, inst, pc) => AluI(il, inst, OpCodes.Add, signExtend: true);
        _opCodeTable[(int)MipsOpCode.SetLessThanImmediate] = (il, inst, pc) => AluI(il, inst, OpCodes.Clt, signExtend: true);
        _opCodeTable[(int)MipsOpCode.AndImmediate] = (il, inst, pc) => AluI(il, inst, OpCodes.And);
        _opCodeTable[(int)MipsOpCode.OrImmediate] = (il, inst, pc) => AluI(il, inst, OpCodes.Or);
        _opCodeTable[(int)MipsOpCode.ExclusiveOrImmediate] = (il, inst, pc) => AluI(il, inst, OpCodes.Xor);
        _opCodeTable[(int)MipsOpCode.LoadUpperImmediate] = (il, inst, pc) => Lui(il, inst);
    }

    private void InitSpecial(MipsVersion version)
    {
        _specialTable[(int)FunctionCode.ShiftLeftLogical] = (il, inst, pc) => Shift(il, inst, OpCodes.Shl);
        _specialTable[(int)FunctionCode.ShiftRightLogical] = (il, inst, pc) => Shift(il, inst, OpCodes.Shr_Un);
        _specialTable[(int)FunctionCode.ShiftRightArithmetic] = (il, inst, pc) => Shift(il, inst, OpCodes.Shr);
        _specialTable[(int)FunctionCode.ShiftLeftLogicalVariable] = (il, inst, pc) => ShiftVar(il, inst, OpCodes.Shl);
        _specialTable[(int)FunctionCode.ShiftRightLogicalVariable] = (il, inst, pc) => ShiftVar(il, inst, OpCodes.Shr_Un);
        _specialTable[(int)FunctionCode.ShiftRightArithmeticVariable] = (il, inst, pc) => ShiftVar(il, inst, OpCodes.Shr);
        _specialTable[(int)FunctionCode.JumpAndLinkRegister] = (il, inst, pc) => JumpR(il, inst, pc, link: true);
        _specialTable[(int)FunctionCode.SystemCall] = (il, inst, pc) => Trap(il, inst, pc, MipsTrap.Syscall);
        _specialTable[(int)FunctionCode.Break] = (il, inst, pc) => Trap(il, inst, pc, MipsTrap.Breakpoint);
        _specialTable[(int)FunctionCode.Add] = (il, inst, pc) => CheckedAluR(il, inst, pc, OpCodes.Add, false);
        _specialTable[(int)FunctionCode.AddUnsigned] = (il, inst, pc) => AluR(il, inst, OpCodes.Add);
        _specialTable[(int)FunctionCode.Subtract] = (il, inst, pc) => CheckedAluR(il, inst, pc, OpCodes.Sub, true);
        _specialTable[(int)FunctionCode.SubtractUnsigned] = (il, inst, pc) => AluR(il, inst, OpCodes.Sub);
        _specialTable[(int)FunctionCode.And] = (il, inst, pc) => AluR(il, inst, OpCodes.And);
        _specialTable[(int)FunctionCode.Or] = (il, inst, pc) => AluR(il, inst, OpCodes.Or);
        _specialTable[(int)FunctionCode.ExclusiveOr] = (il, inst, pc) => AluR(il, inst, OpCodes.Xor);
        _specialTable[(int)FunctionCode.Nor] = (il, inst, pc) => AluR(il, inst, OpCodes.Or, followUp: OpCodes.Not);
        _specialTable[(int)FunctionCode.SetLessThan] = (il, inst, pc) => AluR(il, inst, OpCodes.Clt);
        _specialTable[(int)FunctionCode.SetLessThanUnsigned] = (il, inst, pc) => AluR(il, inst, OpCodes.Clt_Un);


        if (version is < MipsVersion.Mips_R6)
        {
            _specialTable[(int)FunctionCode.JumpRegister] = (il, inst, pc) => JumpR(il, inst, pc);
            _specialTable[(int)FunctionCode.Multiply] = (il, inst, pc) => MultR(il, inst, true);
            _specialTable[(int)FunctionCode.MultiplyUnsigned] = (il, inst, pc) => MultR(il, inst, false);
            _specialTable[(int)FunctionCode.Divide] = (il, inst, pc) => DivR(il, inst, true);
            _specialTable[(int)FunctionCode.DivideUnsigned] = (il, inst, pc) => DivR(il, inst, false);
            _specialTable[(int)FunctionCode.MoveFromHigh] = (il, inst, pc) => MoveFromTo(il, MipsGpRegister.High, inst.RD);
            _specialTable[(int)FunctionCode.MoveToHigh] = (il, inst, pc) => MoveFromTo(il, inst.RS, MipsGpRegister.High);
            _specialTable[(int)FunctionCode.MoveFromLow] = (il, inst, pc) => MoveFromTo(il, MipsGpRegister.Low, inst.RD);
            _specialTable[(int)FunctionCode.MoveToLow] = (il, inst, pc) => MoveFromTo(il, inst.RS, MipsGpRegister.Low);
        }
    }
}
