// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;

namespace Zarem.Emulator.Models.JIT;

public partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private bool DispatchCoProc1(ILGenerator il, FloatInstruction inst, T pc)
    {
        return inst.CoProc1RSCode switch
        {
            CoProc1RSCode.MFC1 => MoveFromFloat(il, inst),
            CoProc1RSCode.MTC1 => MoveToFloat(il, inst),

            _ => inst.Format switch
            {
                MipsFloatFormat.Single => DispatchFloatOp<float>(il, inst, pc),
                MipsFloatFormat.Double => DispatchFloatOp<double>(il, inst, pc),
                _ => throw new NotImplementedException()
            },
        };
    }

    private bool DispatchFloatOp<TFloat>(ILGenerator il, FloatInstruction inst, T pc)
        where TFloat : unmanaged
    {
        return inst.FloatFuncCode switch
        {
            FloatFuncCode.Add => FloatAlu<TFloat>(il, inst, OpCodes.Add),
            FloatFuncCode.Subtract => FloatAlu<TFloat>(il, inst, OpCodes.Sub),
            FloatFuncCode.Multiply => FloatAlu<TFloat>(il, inst, OpCodes.Mul),
            FloatFuncCode.Divide => FloatAlu<TFloat>(il, inst, OpCodes.Div),

            _ => throw new NotImplementedException($"FPU opcode {inst.FloatFuncCode} not JIT-ted yet.")
        };
    }

    private bool MoveToFloat(ILGenerator il, FloatInstruction inst)
    {
        EmitStoreRegister<T>(il, inst.FS, () =>
        {
            EmitLoadRegister(il, inst.RT);
            EmitConv(il);
        });

        return false;
    }

    private bool MoveFromFloat(ILGenerator il, FloatInstruction inst)
    {
        EmitStoreRegister(il, inst.RT, () =>
        {
            EmitLoadRegister<T>(il, inst.FS);
            EmitConv(il);
        });

        return false;
    }
}
