// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models.JIT;

public partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private void DispatchCoProc1(ILGenerator il, MipsFloatInstruction inst, T pc)
    {
        var fInst = inst;
        var func = _coProc1RSTable[(int)fInst.RSCode];
        func(il, fInst, pc);
    }

    private void DispatchFloatFunc<TFormat>(ILGenerator il, MipsFloatInstruction inst, T pc)
        where TFormat : unmanaged, INumber<TFormat>
    {
        int index = GetFloatFuncTableIndex<TFormat>();
        var func = _floatFuncTables[index][(int)inst.Function];
        func(il, inst, pc);
    }

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

    private static int GetFloatFuncTableIndex<TFormat>()
    {
        if (typeof(TFormat) == typeof(float)) return 0;
        if (typeof(TFormat) == typeof(double)) return 1;
        if (typeof(TFormat) == typeof(int)) return 2;
        if (typeof(TFormat) == typeof(long)) return 3;
        else return ThrowHelper.ThrowFormatException<int>();
    }
}
