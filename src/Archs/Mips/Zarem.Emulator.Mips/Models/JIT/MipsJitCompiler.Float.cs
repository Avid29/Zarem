// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;

namespace Zarem.Emulator.Models.JIT;

public partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private bool DispatchCoProc1(ILGenerator il, FloatInstruction inst)
    {
        return inst.CoProc1RSCode switch
        {
            CoProc1RSCode.MFC1 => MoveFromFloat(il, inst),
            CoProc1RSCode.MTC1 => MoveToFloat(il, inst),

            _ => inst.Format switch
            {
                MipsFloatFormat.Single => DispatchFloatOp<float>(il, inst),
                MipsFloatFormat.Double => DispatchFloatOp<double>(il, inst),
                MipsFloatFormat.Word => DispatchFloatIntOp<int>(il, inst),
                MipsFloatFormat.Long => DispatchFloatIntOp<long>(il, inst),
                _ => throw new NotImplementedException()
            },
        };
    }

    private bool DispatchFloatOp<TFloat>(ILGenerator il, FloatInstruction inst)
        where TFloat : unmanaged
    {
        return inst.FloatFuncCode switch
        {
            FloatFuncCode.Add => FloatAlu<TFloat>(il, inst, OpCodes.Add),
            FloatFuncCode.Subtract => FloatAlu<TFloat>(il, inst, OpCodes.Sub),
            FloatFuncCode.Multiply => FloatAlu<TFloat>(il, inst, OpCodes.Mul),
            FloatFuncCode.Divide => FloatAlu<TFloat>(il, inst, OpCodes.Div),
            FloatFuncCode.SquareRoot => FloatUnary<TFloat>(il, inst, nameof(Math.Sqrt)),
            FloatFuncCode.AbsoluteValue => FloatUnary<TFloat>(il, inst, nameof(Math.Abs)),
            FloatFuncCode.Move => MoveFloat<TFloat>(il, inst.FS, inst.FD),
            FloatFuncCode.Negate => FloatUnary<TFloat>(il, inst, OpCodes.Neg),
            FloatFuncCode.Round_L => FloatRound<TFloat, long>(il, inst, nameof(Math.Round)),
            FloatFuncCode.Truncate_L => FloatRound<TFloat, long>(il, inst, nameof(Math.Truncate)),
            FloatFuncCode.Ceiling_L => FloatRound<TFloat, long>(il, inst, nameof(Math.Ceiling)),
            FloatFuncCode.Floor_L => FloatRound<TFloat, long>(il, inst, nameof(Math.Floor)),
            FloatFuncCode.Round_W => FloatRound<TFloat, int>(il, inst, nameof(Math.Round)),
            FloatFuncCode.Truncate_W => FloatRound<TFloat, int>(il, inst, nameof(Math.Truncate)),
            FloatFuncCode.Ceiling_W => FloatRound<TFloat, int>(il, inst, nameof(Math.Ceiling)),
            FloatFuncCode.Floor_W => FloatRound<TFloat, int>(il, inst, nameof(Math.Floor)),

            FloatFuncCode.ConvertToSingle => FloatConvert<TFloat, float>(il, inst.FS, inst.FD),
            FloatFuncCode.ConvertToDouble => FloatConvert<TFloat, double>(il, inst.FS, inst.FD),
            FloatFuncCode.ConvertToWord => FloatConvert<TFloat, int>(il, inst.FS, inst.FD),
            FloatFuncCode.ConvertToLong => FloatConvert<TFloat, long>(il, inst.FS, inst.FD),

            _ => throw new NotImplementedException($"FPU opcode {inst.FloatFuncCode} not JIT-ted yet.")
        };
    }

    private bool DispatchFloatIntOp<TNumber>(ILGenerator il, FloatInstruction inst)
        where TNumber : unmanaged
    {
        return inst.FloatFuncCode switch
        {
            FloatFuncCode.ConvertToSingle => FloatConvert<TNumber, float>(il, inst.FS, inst.FD),
            FloatFuncCode.ConvertToDouble => FloatConvert<TNumber, double>(il, inst.FS, inst.FD),
            FloatFuncCode.ConvertToWord => FloatConvert<TNumber, int>(il, inst.FS, inst.FD),
            FloatFuncCode.ConvertToLong => FloatConvert<TNumber, long>(il, inst.FS, inst.FD),
            _ => throw new NotImplementedException(),
        };
    }

    private bool FloatAlu<TFloat>(ILGenerator il, FloatInstruction inst, OpCode ilOpCode)
        where TFloat : unmanaged
    {
        EmitStoreRegister<TFloat>(il, inst.FD, () =>
        {
            EmitLoadRegister<TFloat>(il, inst.FS);
            EmitLoadRegister<TFloat>(il, inst.FT);
            il.Emit(ilOpCode);
        });

        return false;
    }

    private bool FloatUnary<TFloat>(ILGenerator il, FloatInstruction inst, OpCode ilOpCode)
        where TFloat : unmanaged
    {
        EmitStoreRegister<TFloat>(il, inst.FD, () =>
        {
            EmitLoadRegister<TFloat>(il, inst.FS);
            il.Emit(ilOpCode);
        });

        return false;
    }

    private bool FloatUnary<TFloat>(ILGenerator il, FloatInstruction inst, string methodName)
        where TFloat : unmanaged
    {
        EmitStoreRegister<TFloat>(il, inst.FD, () =>
        {
            EmitLoadRegister<TFloat>(il, inst.FS);

            // Resolve Math.Sqrt(double) or MathF.Sqrt(float)
            Type mathClass = typeof(TFloat) == typeof(float) ? typeof(MathF) : typeof(Math);
            var method = mathClass.GetMethod(methodName, [typeof(TFloat)]);
            Guard.IsNotNull(method);
            il.Emit(OpCodes.Call, method);
        });

        return false;
    }

    private bool FloatRound<TFrom, TTo>(ILGenerator il, FloatInstruction inst, string methodName)
        where TFrom : unmanaged
        where TTo : unmanaged
    {
        EmitStoreRegister<TTo>(il, inst.FD, () =>
        {
            EmitLoadRegister<TFrom>(il, inst.FS);

            // Resolve Math.Sqrt(double) or MathF.Sqrt(float)
            Type mathClass = typeof(TFrom) == typeof(float) ? typeof(MathF) : typeof(Math);
            var method = mathClass.GetMethod(methodName, [typeof(TFrom)]);
            Guard.IsNotNull(method);
            il.Emit(OpCodes.Call, method);
            EmitConv<TTo>(il);
        });

        return false;
    }

    private bool FloatConvert<TFrom, TTo>(ILGenerator il, MipsFloatRegister fs, MipsFloatRegister fd)
        where TFrom : unmanaged
        where TTo : unmanaged
    {
        if (typeof(TTo) != typeof(TFrom))
        {
            EmitStoreRegister<TTo>(il, fd, () =>
            {
                EmitLoadRegister<TFrom>(il, fs);
                EmitConv<TTo>(il);
            });
        }

        return false;
    }

    private bool MoveFloat<TFloat>(ILGenerator il, MipsFloatRegister fs, MipsFloatRegister fd)
        where TFloat : unmanaged
    {
        EmitStoreRegister<TFloat>(il, fd, () =>
        {
            EmitLoadRegister<TFloat>(il, fs);
        });

        return false;
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
