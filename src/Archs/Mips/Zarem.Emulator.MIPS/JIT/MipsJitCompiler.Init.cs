// Avishai Dernis 2026

using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Config;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Emulator.JIT;

public partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private void InitTables(MIPSEmulatorConfig config)
    {
        var version = config.Version;
    }

    private void InitRoot(MipsVersion version)
    {
        _opCodeTable[(int)MipsOpCode.AddImmediateUnsigned] = (il, inst, pc) => AluI(il, inst, OpCodes.Add, signExtend: true);
        _opCodeTable[(int)MipsOpCode.SetLessThanImmediate] = (il, inst, pc) => AluI(il, inst, OpCodes.Clt, signExtend: true);
    }

    private void InitSpecial(MipsVersion version)
    {
        _specialTable[(int)FunctionCode.ShiftLeftLogical] = (il, inst, pc) => Shift(il, inst, OpCodes.Shl);
        _specialTable[(int)FunctionCode.ShiftRightLogical] = (il, inst, pc) => Shift(il, inst, OpCodes.Shr_Un);
        _specialTable[(int)FunctionCode.ShiftRightArithmetic] = (il, inst, pc) => Shift(il, inst, OpCodes.Shr);
        _specialTable[(int)FunctionCode.ShiftLeftLogicalVariable] = (il, inst, pc) => ShiftVar(il, inst, OpCodes.Shl);
        _specialTable[(int)FunctionCode.ShiftRightLogicalVariable] = (il, inst, pc) => ShiftVar(il, inst, OpCodes.Shr_Un);
        _specialTable[(int)FunctionCode.ShiftRightArithmeticVariable] = (il, inst, pc) => ShiftVar(il, inst, OpCodes.Shr);

        _specialTable[(int)FunctionCode.AddUnsigned] = (il, inst, pc) => AluR(il, inst, OpCodes.Add);
        _specialTable[(int)FunctionCode.SubtractUnsigned] = (il, inst, pc) => AluR(il, inst, OpCodes.Sub);
        _specialTable[(int)FunctionCode.And] = (il, inst, pc) => AluR(il, inst, OpCodes.And);
        _specialTable[(int)FunctionCode.Or] = (il, inst, pc) => AluR(il, inst, OpCodes.Or);
        _specialTable[(int)FunctionCode.ExclusiveOr] = (il, inst, pc) => AluR(il, inst, OpCodes.Xor);
        _specialTable[(int)FunctionCode.Nor] = (il, inst, pc) => AluR(il, inst, OpCodes.Or, followUp: OpCodes.Not);

        _specialTable[(int)FunctionCode.SetLessThan] = (il, inst, pc) => AluR(il, inst, OpCodes.Clt);
    }
}
