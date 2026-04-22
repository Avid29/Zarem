// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Machine;
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
            EmitStoreRegister<T>(il, reg, () =>
            {
                var register = reg;
                var index = Unsafe.As<TRegister, int>(ref register);
                EmitLoadRegisterAddress(il, index, regFile);
                il.EmitLdind<T>();
            });
        }
    }

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
            EmitLoadRegisterAddress(il, index, regFile);
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
        var regIndex = Unsafe.As<TRegister, int>(ref register);
        var regLocal = _regLocals[regIndex];
        il.Emit(OpCodes.Ldloc, regLocal);

        if (sizeof(T) != sizeof(TData))
            il.EmitConv<T>();
    }

    /// <inheritdoc cref="EmitStoreRegister{TData}(ILGenerator, TRegister, Action)"/>
    protected void EmitStoreRegister(ILGenerator il, TRegister register, Action emitEvaluation) => EmitStoreRegister<T>(il, register, emitEvaluation);

    /// <summary>
    /// Emits the CIL to load a register to the CLR stack from locals.
    /// </summary>
    protected virtual void EmitStoreRegister<TData>(ILGenerator il, TRegister register, Action emitEvaluation)
        where TData : unmanaged, INumber<TData>
    {
        emitEvaluation();
        var regIndex = Unsafe.As<TRegister, int>(ref register);
        var regLocal = _regLocals[regIndex];
        il.Emit(OpCodes.Stloc, regLocal);
    }
    
    /// <summary>
    /// Emits the CIL to load the address of a register from a register file in memory.
    /// </summary>
    protected void EmitLoadRegisterAddress(ILGenerator il, int index , RegisterFile<T> regFile)
    {
        // Calculate the address of the register in memory
        var regs = regFile.Regs;
        nint regAddress = (nint)regs + (index * sizeof(T));

        // Emit the address 
        if (nint.Size == 8) il.Emit(OpCodes.Ldc_I8, regAddress);
        else if (nint.Size == 4) il.Emit(OpCodes.Ldc_I4, (int)regAddress);
        else throw new PlatformNotSupportedException($"Unsupported pointer size: {nint.Size}");

        // Convert to a native integer
        il.Emit(OpCodes.Conv_U);
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
