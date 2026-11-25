// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Models.Enums;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A base class for a component which recompiles blocks of assembly into .NET CIL code.
/// </summary>
public unsafe abstract partial class JitCompiler<T, TRegister, TTrap>
{
    /// <inheritdoc cref="EmitLoadRegister{TData}(JitBlockContext{T, TRegister, TTrap}, TRegister)"/>
    public void EmitLoadRegister(JitBlockContext<T, TRegister, TTrap> context, TRegister register) => EmitLoadRegister<T>(context, register);

    /// <summary>
    /// Emits the CIL to load a register to the CLR stack from locals.
    /// </summary>
    public virtual void EmitLoadRegister<TData>(JitBlockContext<T, TRegister, TTrap> context, TRegister register)
        where TData : unmanaged, INumber<TData>
    {
        var il = context.ILGenerator;

        // Load the register from local
        var regLocal = GetRegisterLocal(context, register);
        EmitLoadRegister<TData>(il, regLocal);
    }

    /// <summary>
    /// Emits the CIL to load a register to the CLR stack from locals.
    /// </summary>
    protected virtual void EmitLoadRegister<TData>(ILGenerator il, LocalBuilder local)
        where TData : unmanaged, INumber<TData>
    {
        // Load the register from local
        il.Emit(OpCodes.Ldloc, local);

        // Convert the value to TData if neccesary
        if (local.LocalType != typeof(TData))
            il.EmitConv<TData>();
    }

    /// <summary>
    /// Emits the CIL to load a register to the CLR stack from locals.
    /// </summary>
    public virtual void EmitStoreRegister(JitBlockContext<T, TRegister, TTrap> context, TRegister register, Action<ILGenerator> emitEvaluation)
    {
        var il = context.ILGenerator;
        emitEvaluation(il);

        // Store the value to the register's local
        var regLocal = GetRegisterLocal(context, register);
        il.Emit(OpCodes.Stloc, regLocal);
    }

    /// <summary>
    /// Gets the <see cref="LocalBuilder"/> for the specified <typeparamref name="TRegister"/>.
    /// </summary>
    protected virtual LocalBuilder GetRegisterLocal(JitBlockContext<T, TRegister, TTrap> context, TRegister register)
        => context.GetRegisterLocal(Cpu.RegisterFile, register);

    /// <summary>
    /// Emits the CIL to load the address of a register from a register file in memory.
    /// </summary>
    public void EmitLoadRegisterAddress<TData>(ILGenerator il, int index , TData* regs)
        where TData : unmanaged
    {
        // Calculate the address of the register in memory
        nint regAddress = (nint)regs + (index * sizeof(TData));

        // Emit the address 
        if (nint.Size == 8) il.Emit(OpCodes.Ldc_I8, regAddress);
        else if (nint.Size == 4) il.Emit(OpCodes.Ldc_I4, (int)regAddress);
        else throw new PlatformNotSupportedException($"Unsupported pointer size: {nint.Size}");

        // Convert to a native integer
        il.Emit(OpCodes.Conv_U);
    }

    /// <summary>
    /// Emits the CIL to check for arithmetic overflow for addition or subtraction.
    /// </summary>
    protected void EmitOverflowGuard<TData>(ILGenerator il, LocalBuilder rs, LocalBuilder rtOrImm, LocalBuilder result, Action emitOverflowHandling, bool isSubtraction = false)
        where TData : unmanaged, INumber<TData>
    {
        // Logic: ((rs ^ result) & (rtOrImm ^ result)) < 0  (for Addition)
        // Logic: ((rs ^ result) & (rs ^ rtOrImm)) < 0      (for Subtraction)

        Label noOverflow = il.DefineLabel();

        // First term: (rs ^ result)
        EmitLoadRegister<TData>(il, rs);
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Xor);

        // Second term:
        if (isSubtraction)
        {
            // (rs ^ rtOrImm)
            EmitLoadRegister<TData>(il, rs);
            il.Emit(OpCodes.Ldloc, rtOrImm);
            il.Emit(OpCodes.Xor);
        }
        else
        {
            // (rtOrImm ^ result)
            EmitLoadRegister<TData>(il, rtOrImm);
            il.Emit(OpCodes.Ldloc, result);
            il.Emit(OpCodes.Xor);
        }

        il.Emit(OpCodes.And);

        // Check sign bit
        il.EmitLoadConstant(TData.Zero);

        il.Emit(OpCodes.Bge, noOverflow);

        // Handling path (ends block)
        emitOverflowHandling();

        il.MarkLabel(noOverflow);
    }

    /// <summary>
    /// Emits the CIL update the trap out argument.
    /// </summary>
    protected static void EmitTrapArg(ILGenerator il, TTrap trap)
    {
        var trapCode = Unsafe.As<TTrap, int>(ref trap);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldc_I4, trapCode);
        il.Emit(OpCodes.Stind_I4);
    }

    /// <summary>
    /// Emits the CIL update the trap out argument.
    /// </summary>
    protected static void EmitTrapArg(ILGenerator il, LocalBuilder trapVar)
    {
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldloc, trapVar);
        il.Emit(OpCodes.Stind_I4);
    }

    /// <summary>
    /// Emits the CIL to return a trap.
    /// </summary>
    public void EmitTrapRet(JitBlockContext<T, TRegister, TTrap> context, TTrap trap, T pc)
    {
        var il = context.ILGenerator;
        context.EmitFlushLocalRegisters(this);
        EmitTrapArg(il, trap);
        il.EmitLoadConstant(pc);
        il.Emit(OpCodes.Ret);
    }

    /// <summary>
    /// Emits the CIL to return a trap.
    /// </summary>
    protected void EmitTrapRet(JitBlockContext<T, TRegister, TTrap> context, LocalBuilder trapVar, T pc)
    {
        var il = context.ILGenerator;
        context.EmitFlushLocalRegisters(this);
        EmitTrapArg(il, trapVar);
        il.EmitLoadConstant(pc);
        il.Emit(OpCodes.Ret);
    }

    /// <summary>
    /// Gets the <see cref="Sign"/> of <typeparamref name="TData"/>.
    /// </summary>
    public static Sign IsSigned<TData>()
        where TData : unmanaged, INumber<TData>
    {
        if (typeof(TData) == typeof(sbyte) || typeof(TData) == typeof(short) ||
            typeof(TData) == typeof(int) || typeof(TData) == typeof(long))
            return Sign.Signed;
        else
            return Sign.Unsigned;
    }
}
