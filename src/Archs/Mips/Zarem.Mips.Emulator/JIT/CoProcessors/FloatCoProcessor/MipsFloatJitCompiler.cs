// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.JIT;
using Zarem.Emulator.JIT.Compilers;
using Zarem.Emulator.Models.JIT;
using Zarem.Mips.Emulator.Machine.CoProcessors.FloatingPoint;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.JIT.CoProcessors.FloatCoProcessor;

/// <summary>
/// A class which compiles blocks of MIPS floating-point code into JIT IL code.
/// </summary>
public unsafe partial class MipsFloatJitCompiler<T, TFloat> : JitChildCompiler<T, MipsGpRegister, MipsTrap>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    private delegate void MipsFloatEmitter(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsFloatInstruction inst, T pc);

    private readonly MipsFloatInstructionDecodeTable<MipsFloatEmitter> _instructionTable;
    private readonly FloatCoProcessor<T, TFloat> _fpu;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsFloatJitCompiler{T, TFloat}"/> class.
    /// </summary>
    public MipsFloatJitCompiler(MipsJitCompiler<T> parent, FloatCoProcessor<T, TFloat> fpu)
    {
        _instructionTable = new MipsFloatInstructionDecodeTable<MipsFloatEmitter>(ReservedInstruction);

        Parent = parent;
        _fpu = fpu;

        InitFloat(Parent.Cpu.Config.VersionInfo);
    }

    /// <inheritdoc/>
    public override MipsJitCompiler<T> Parent { get; }

    /// <inheritdoc/>
    public bool CompileInstruction(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction instruction, T pc)
    {
        var emitter = _instructionTable.Lookup(instruction);
        emitter(context, instruction, pc);
        return false;
    }

    private void FloatAlu<TFormat>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsFloatInstruction inst, OpCode ilOpCode)
        where TFormat : unmanaged
    {
        EmitStoreRegister<TFormat>(context, inst.FD, () =>
        {
            EmitLoadRegister<TFormat>(context, inst.FS);
            EmitLoadRegister<TFormat>(context, inst.FT);
            context.ILGenerator.Emit(ilOpCode);
        });
    }

    private void FloatUnary<TFormat>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsFloatInstruction inst, OpCode ilOpCode)
        where TFormat : unmanaged
    {
        EmitStoreRegister<TFormat>(context, inst.FD, () =>
        {
            EmitLoadRegister<TFormat>(context, inst.FS);
            context.ILGenerator.Emit(ilOpCode);
        });
    }

    private void FloatUnary<TFormat>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsFloatInstruction inst, string methodName)
        where TFormat : unmanaged
    {
        EmitStoreRegister<TFormat>(context, inst.FD, () =>
        {
            EmitLoadRegister<TFormat>(context, inst.FS);

            Type mathClass = typeof(TFormat) == typeof(float) ? typeof(MathF) : typeof(Math);
            var method = mathClass.GetMethod(methodName, [typeof(TFormat)]);
            Guard.IsNotNull(method);
            context.ILGenerator.Emit(OpCodes.Call, method);
        });
    }

    private void FloatRound<TFrom, TTo>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsFloatInstruction inst, string methodName)
        where TFrom : unmanaged
        where TTo : unmanaged
    {
        EmitStoreRegister<TTo>(context, inst.FD, () =>
        {
            EmitLoadRegister<TFrom>(context, inst.FS);

            Type mathClass = typeof(TFrom) == typeof(float) ? typeof(MathF) : typeof(Math);
            var method = mathClass.GetMethod(methodName, [typeof(TFrom)]);
            Guard.IsNotNull(method);
            context.ILGenerator.Emit(OpCodes.Call, method);
            context.ILGenerator.EmitConv<TTo>();
        });
    }

    private void FloatConvert<TFrom, TTo>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsFloatRegister fs, MipsFloatRegister fd)
        where TFrom : unmanaged
        where TTo : unmanaged
    {
        if (typeof(TTo) != typeof(TFrom))
        {
            EmitStoreRegister<TTo>(context, fd, () =>
            {
                EmitLoadRegister<TFrom>(context, fs);
                context.ILGenerator.EmitConv<TTo>();
            });
        }
    }

    private void MoveFloat<TFormat>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsFloatRegister fs, MipsFloatRegister fd)
        where TFormat : unmanaged
    {
        EmitStoreRegister<TFormat>(context, fd, () =>
        {
            EmitLoadRegister<TFormat>(context, fs);
        });
    }

    private void MoveToFloat(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsFloatInstruction inst)
    {
        EmitStoreRegister<T>(context, inst.FS, () =>
        {
            Parent.EmitLoadRegister(context, inst.RT);
            context.ILGenerator.EmitConv<T>();
        });
    }

    private void MoveFromFloat(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsFloatInstruction inst)
    {
        Parent.EmitStoreRegister(context, inst.RT, il =>
        {
            EmitLoadRegister<T>(context, inst.FS);
            il.EmitConv<T>();
        });
    }

    private void EmitLoadRegister<TFloatData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsFloatRegister register)
        where TFloatData : unmanaged
    {
        var il = context.ILGenerator;

        // In legacy mode, double precision floating point values are stored in two consecutive single precision registers.
        // We use paired registers when we are loading a double precision value (TFloatData is 64 bits) and we are in legacy mode.
        bool legacyMode = sizeof(TFloat) == sizeof(uint) || !Parent.Cpu.ControlCoProcessor.FloatingPoint64BitMode;
        bool usePairedRegisters = sizeof(TFloatData) == sizeof(ulong) && legacyMode;

        if (usePairedRegisters)
        {
            // Load the upper half of the double (the second 32 bits)
            EmitLoadRegisterAddress(context, register + 1);
            il.EmitLdind<TFloat>();
            il.EmitConv<uint>();
            il.EmitConv<ulong>();
            il.EmitLoadConstant(32);                        // Shift the upper half left by 32 bits to make room for the lower half
            il.Emit(OpCodes.Shl);
            EmitLoadRegisterAddress(context, register);          // Load the lower half of the double (the first 32 bits)
            il.EmitLdind<TFloat>();
            il.EmitConv<uint>();
            il.EmitConv<ulong>();
            il.Emit(OpCodes.Or);                            // Combine the two halves into a single 64-bit value

            // If the target type is not ulong, we need to convert the 64-bit value to the appropriate type
            if (typeof(TFloatData) != typeof(ulong))
            {
                LocalBuilder bitStorage = il.DeclareLocal(typeof(ulong));
                il.Emit(OpCodes.Stloc, bitStorage);
                il.Emit(OpCodes.Ldloca, bitStorage);
                il.EmitLdind<TFloatData>();
            }
        }
        else
        {
            // Load the register's address then retrieve the value at that address
            EmitLoadRegisterAddress(context, register);
            il.EmitLdind<TFloatData>();
        }
    }

    private void EmitStoreRegister<TFloatData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsFloatRegister register, Action emitEvaluation)
        where TFloatData : unmanaged
    {
        var il = context.ILGenerator;

        // In legacy mode, double precision floating point values are stored in two consecutive single precision registers.
        // We use paired registers when we are loading a double precision value (TFloatData is 64 bits) and we are in legacy mode.
        bool legacyMode = sizeof(TFloat) == sizeof(uint) || !Parent.Cpu.ControlCoProcessor.FloatingPoint64BitMode;
        bool usePairedRegisters = sizeof(TFloatData) == sizeof(ulong) && legacyMode;

        if (usePairedRegisters)
        {
            // 1. Evaluate the incoming expression and store it as a 64-bit bitmask (ulong)
            LocalBuilder value64Local = il.DeclareLocal(typeof(ulong));
            emitEvaluation();

            // If the target type is not ulong, we need to convert the 64-bit value to the appropriate type
            if (typeof(TFloatData) == typeof(double))
            {
                LocalBuilder doubleLocal = il.DeclareLocal(typeof(double));
                il.Emit(OpCodes.Stloc, doubleLocal);
                il.Emit(OpCodes.Ldloca, doubleLocal);
                il.EmitLdind<ulong>();
            }
            il.Emit(OpCodes.Stloc, value64Local);

            // Store Lower 32 Bits into _regs[register]
            EmitLoadRegisterAddress(context, register);

            // For host-endian safety: Load existing full TFloat native slot first
            il.Emit(OpCodes.Dup);
            il.EmitLdind<TFloat>();
            il.Emit(OpCodes.Conv_U8);
            il.EmitLoadConstant(0xFFFFFFFF00000000UL);      // Mask out the lower 32 bits of the existing register value
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Ldloc, value64Local);           // Isolate our new lower 32 bits from our evaluated local
            il.Emit(OpCodes.Conv_U4);
            il.Emit(OpCodes.Conv_U8);
            il.Emit(OpCodes.Or);                            // Merge and write back out to the full native memory slot
            il.EmitConv<TFloat>();
            il.EmitStind<TFloat>();

            // Store Upper 32 Bits into _regs[register + 1]
            EmitLoadRegisterAddress(context, register + 1);

            // Load existing full TFloat native slot
            il.Emit(OpCodes.Dup);
            il.EmitLdind<TFloat>();
            il.Emit(OpCodes.Conv_U8);
            il.EmitLoadConstant(0xFFFFFFFF00000000UL);      // Mask out the lower 32 bits of the existing register value
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Ldloc, value64Local);           // Isolate our new upper 32 bits by shifting right logically
            il.EmitLoadConstant(32);
            il.Emit(OpCodes.Shr_Un);
            il.Emit(OpCodes.Conv_U4);
            il.Emit(OpCodes.Conv_U8);
            il.Emit(OpCodes.Or);                            // Merge and write back out to the full native memory slot
            il.EmitConv<TFloat>();
            il.EmitStind<TFloat>();
        }
        else
        {
            // Load the register's address, emit the evaluation instructions, and store the value
            EmitLoadRegisterAddress(context, register);
            emitEvaluation();
            il.EmitStind<TFloatData>();
        }
    }

    private void EmitLoadRegisterAddress(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsFloatRegister register)
        => Parent.EmitLoadRegisterAddress(context.ILGenerator, (int)register, _fpu.RegisterFile.Regs);

    private void ReservedInstruction(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsFloatInstruction inst, T pc)
        => Parent.EmitTrapRet(context, MipsTrap.ReservedInstruction, pc);
}
