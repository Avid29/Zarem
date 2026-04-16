// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;

namespace Zarem.Emulator.Models.JIT;

public unsafe partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private bool DispatchCoProc1(ILGenerator il, FloatInstruction inst, T pc)
    {
        var floatInstruction = (FloatInstruction)inst;

        return floatInstruction.CoProc1RSCode switch
        {
            _ => floatInstruction.Format switch
            {
                MipsFloatFormat.Single => DispatchFloatOp<float>(il, floatInstruction, pc),
                MipsFloatFormat.Double => DispatchFloatOp<double>(il, floatInstruction, pc),
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
}
