// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models.JIT;

public partial class MipsJitCompiler<T, TFloat>
{
    private void FloatAlu<TFormat>(ILGenerator il, MipsFloatInstruction inst, OpCode ilOpCode)
        where TFormat : unmanaged
    {
        EmitStoreRegister<TFormat>(il, inst.FD, () =>
        {
            EmitLoadRegister<TFormat>(il, inst.FS);
            EmitLoadRegister<TFormat>(il, inst.FT);
            il.Emit(ilOpCode);
        });
    }

    private void FloatUnary<TFormat>(ILGenerator il, MipsFloatInstruction inst, OpCode ilOpCode)
        where TFormat : unmanaged
    {
        EmitStoreRegister<TFormat>(il, inst.FD, () =>
        {
            EmitLoadRegister<TFormat>(il, inst.FS);
            il.Emit(ilOpCode);
        });
    }

    private void FloatUnary<TFormat>(ILGenerator il, MipsFloatInstruction inst, string methodName)
        where TFormat : unmanaged
    {
        EmitStoreRegister<TFormat>(il, inst.FD, () =>
        {
            EmitLoadRegister<TFormat>(il, inst.FS);

            Type mathClass = typeof(TFormat) == typeof(float) ? typeof(MathF) : typeof(Math);
            var method = mathClass.GetMethod(methodName, [typeof(TFormat)]);
            Guard.IsNotNull(method);
            il.Emit(OpCodes.Call, method);
        });
    }

    private void FloatRound<TFrom, TTo>(ILGenerator il, MipsFloatInstruction inst, string methodName)
        where TFrom : unmanaged
        where TTo : unmanaged
    {
        EmitStoreRegister<TTo>(il, inst.FD, () =>
        {
            EmitLoadRegister<TFrom>(il, inst.FS);

            Type mathClass = typeof(TFrom) == typeof(float) ? typeof(MathF) : typeof(Math);
            var method = mathClass.GetMethod(methodName, [typeof(TFrom)]);
            Guard.IsNotNull(method);
            il.Emit(OpCodes.Call, method);
            il.EmitConv<TTo>();
        });
    }

    private void FloatConvert<TFrom, TTo>(ILGenerator il, MipsFloatRegister fs, MipsFloatRegister fd)
        where TFrom : unmanaged
        where TTo : unmanaged
    {
        if (typeof(TTo) != typeof(TFrom))
        {
            EmitStoreRegister<TTo>(il, fd, () =>
            {
                EmitLoadRegister<TFrom>(il, fs);
                il.EmitConv<TTo>();
            });
        }
    }

    private void MoveFloat<TFormat>(ILGenerator il, MipsFloatRegister fs, MipsFloatRegister fd)
        where TFormat : unmanaged
    {
        EmitStoreRegister<TFormat>(il, fd, () =>
        {
            EmitLoadRegister<TFormat>(il, fs);
        });
    }

    private void MoveToFloat(ILGenerator il, MipsFloatInstruction inst)
    {
        EmitStoreRegister<T>(il, inst.FS, () =>
        {
            EmitLoadRegister(il, inst.RT);
            il.EmitConv<T>();
        });
    }

    private void MoveFromFloat(ILGenerator il, MipsFloatInstruction inst)
    {
        EmitStoreRegister(il, inst.RT, il =>
        {
            EmitLoadRegister<T>(il, inst.FS);
            il.EmitConv<T>();
        });
    }
}
