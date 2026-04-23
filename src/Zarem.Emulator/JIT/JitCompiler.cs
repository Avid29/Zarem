// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Models.Enums;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A base class for a component which recompiles blocks of assembly into .NET CIL code.
/// </summary>
public unsafe abstract class JitCompiler<T, TRegister, TTrap>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TRegister : unmanaged, Enum
    where TTrap : unmanaged, Enum
{
    private LocalBuilder[] _regLocals = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="JitCompiler{T, TRegister, TTrap}"/> class.
    /// </summary>
    public JitCompiler(ICpu cpu)
    {
        var getMemoryMethod = cpu.GetType().GetProperty(nameof(ICpu.Memory))?.GetGetMethod();
        Guard.IsNotNull(getMemoryMethod);
        GetMemoryMethod = getMemoryMethod;

        Type[] readWritetypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint)];
        foreach (var type in readWritetypes)
        {
            ReadMethods[type] = typeof(MemorySystem)
                .GetMethods()
                .First(m => m.Name == nameof(MemorySystem.Read) && m.IsGenericMethod && m.GetParameters().Length == 1)
                .MakeGenericMethod(type);
            WriteMethods[type] = typeof(MemorySystem)
                .GetMethods()
                .First(m => m.Name == nameof(MemorySystem.Write) && m.IsGenericMethod && m.GetParameters().Length == 2)
                .MakeGenericMethod(type);
        }
    }

    /// <summary>
    /// Gets a <see cref="MethodInfo"/> for retreiving the <see cref="ICpu.Memory"/> for the compiler.
    /// </summary>
    protected MethodInfo GetMemoryMethod { get; }

    /// <summary>
    /// Gets a <see cref="Dictionary{Type, MethodInfo}"/> for looking up memory read methods by type.
    /// </summary>
    protected Dictionary<Type, MethodInfo> ReadMethods { get; } = [];

    /// <summary>
    /// Gets a <see cref="Dictionary{Type, MethodInfo}"/> for looking up memory write methods by type.
    /// </summary>
    protected Dictionary<Type, MethodInfo> WriteMethods { get; } = [];

    /// <inheritdoc cref="EmitSetupLocalRegisters(ILGenerator, RegisterFile{T}, HashSet{TRegister})"/>
    protected abstract void EmitSetupLocalRegisters(ILGenerator il);

    /// <summary>
    /// Emits the CIL to load registers into CIL locals.
    /// </summary>
    protected void EmitSetupLocalRegisters(ILGenerator il, RegisterFile<T> regFile, HashSet<TRegister> loadRegs)
    {
        // Setup local registers
        _regLocals = new LocalBuilder[regFile.Count];
        for (int i = 0; i < regFile.Count; i++)
        {
            _regLocals[i] = il.DeclareLocal(typeof(T));
        }

        // Load needed registers
        foreach (var reg in loadRegs)
        {
            // Load register from memory into local i
            EmitStoreRegister(il, reg, () =>
            {
                var register = reg;
                var index = Unsafe.As<TRegister, int>(ref register);
                EmitLoadRegisterAddress(il, index, regFile.Regs);
                il.EmitLdind<T>();
            });
        }
    }

    /// <inheritdoc cref="EmitFlushLocalRegisters(ILGenerator, RegisterFile{T}, HashSet{TRegister})"/>
    protected abstract void EmitFlushLocalRegisters(ILGenerator il);

    /// <summary>
    /// Emits the CIL to flush registers from CIL locals back to the register module object.
    /// </summary>
    protected void EmitFlushLocalRegisters(ILGenerator il, RegisterFile<T> regFile, HashSet<TRegister> storeRegs)
    {
        foreach (var reg in storeRegs)
        {
            // Load the address and value of the register
            var register = reg;
            var index = Unsafe.As<TRegister, int>(ref register);
            EmitLoadRegisterAddress(il, index, regFile.Regs);
            EmitLoadRegister<T>(il, reg);
            il.EmitStind<T>();
        }
    }

    /// <inheritdoc cref="EmitLoadRegister{TData}(ILGenerator, TRegister)"/>
    protected void EmitLoadRegister(ILGenerator il, TRegister register) => EmitLoadRegister<T>(il, register);

    /// <summary>
    /// Emits the CIL to load a register to the CLR stack from locals.
    /// </summary>
    protected virtual void EmitLoadRegister<TData>(ILGenerator il, TRegister register)
        where TData : unmanaged, INumber<TData>
    {
        // Load the register from local
        var regIndex = Unsafe.As<TRegister, int>(ref register);
        var regLocal = _regLocals[regIndex];
        il.Emit(OpCodes.Ldloc, regLocal);

        // Convert the value to TData if neccesary
        if (sizeof(T) != sizeof(TData))
            il.EmitConv<TData>();
    }

    /// <summary>
    /// Emits the CIL to load a register to the CLR stack from locals.
    /// </summary>
    protected virtual void EmitStoreRegister(ILGenerator il, TRegister register, Action emitEvaluation)
    {
        emitEvaluation();

        // Store the value to the register's local
        var regIndex = Unsafe.As<TRegister, int>(ref register);
        var regLocal = _regLocals[regIndex];
        il.Emit(OpCodes.Stloc, regLocal);
    }
    
    /// <summary>
    /// Emits the CIL to load the address of a register from a register file in memory.
    /// </summary>
    protected void EmitLoadRegisterAddress(ILGenerator il, int index , T* regs)
    {
        // Calculate the address of the register in memory
        nint regAddress = (nint)regs + (index * sizeof(T));

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
        il.Emit(OpCodes.Ldloc, rs);
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Xor);

        // Second term:
        if (isSubtraction)
        {
            // (rs ^ rtOrImm)
            il.Emit(OpCodes.Ldloc, rs);
            il.Emit(OpCodes.Ldloc, rtOrImm);
            il.Emit(OpCodes.Xor);
        }
        else
        {
            // (rtOrImm ^ result)
            il.Emit(OpCodes.Ldloc, rtOrImm);
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
        il.Emit(OpCodes.Stind_I1);
    }

    /// <summary>
    /// Emits the CIL to return a trap.
    /// </summary>
    protected void EmitTrapRet(ILGenerator il, TTrap trap, T pc)
    {
        EmitFlushLocalRegisters(il);
        EmitTrapArg(il, trap);
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
