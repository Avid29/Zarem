// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.JIT;
using Zarem.Emulator.Models.JIT;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.JIT.CoProcessors.FloatCoProcessor;

/// <summary>
/// A class which compiles blocks of MIPS floating-point code into JIT IL code.
/// </summary>
public unsafe partial class MipsFloatJitCompiler<T, TFloat> : JitCompiler<T, MipsGpRegister, MipsTrap>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    private delegate void MipsFloatEmitter(ILGenerator il, MipsFloatInstruction inst, T pc);

    private readonly MipsFloatInstructionDecodeTable<MipsFloatEmitter> _instructionTable;
    private readonly MipsJitCpu<T> _cpu;
    private readonly JitFloatCoProcessor<T, TFloat> _fpu;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsFloatJitCompiler{T, TFloat}"/> class.
    /// </summary>
    public MipsFloatJitCompiler(MipsJitCpu<T> cpu, JitFloatCoProcessor<T, TFloat> fpu) : base(cpu)
    {
        _cpu = cpu;
        _fpu = fpu;

        _instructionTable = new MipsFloatInstructionDecodeTable<MipsFloatEmitter>(ReservedInstruction);

        InitFloat(cpu.Config.VersionInfo);
    }

    /// <inheritdoc/>
    public void CompileInstruction(ILGenerator il, MipsInstruction instruction, T pc)
    {
        var emitter = _instructionTable.Lookup(instruction);
        emitter(il, instruction, pc);
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

    private void EmitLoadRegister<TFloatData>(ILGenerator il, MipsFloatRegister register)
        where TFloatData : unmanaged
    {
        // In legacy mode, double precision floating point values are stored in two consecutive single precision registers.
        // We use paired registers when we are loading a double precision value (TFloatData is 64 bits) and we are in legacy mode.
        bool legacyMode = sizeof(TFloat) == sizeof(uint) || !_cpu.ControlCoProcessor.FloatingPoint64BitMode;
        bool usePairedRegisters = sizeof(TFloatData) == sizeof(ulong) && legacyMode;

        if (usePairedRegisters)
        {
            // Load the upper half of the double (the second 32 bits)
            EmitLoadRegisterAddress(il, register + 1);
            il.EmitLdind<TFloat>();
            il.EmitConv<uint>();
            il.EmitConv<ulong>();
            il.EmitLoadConstant(32);                        // Shift the upper half left by 32 bits to make room for the lower half
            il.Emit(OpCodes.Shl);
            EmitLoadRegisterAddress(il, register);          // Load the lower half of the double (the first 32 bits)
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
            EmitLoadRegisterAddress(il, register);
            il.EmitLdind<TFloatData>();
        }
    }

    private void EmitStoreRegister<TFloatData>(ILGenerator il, MipsFloatRegister register, Action emitEvaluation)
        where TFloatData : unmanaged
    {
        // In legacy mode, double precision floating point values are stored in two consecutive single precision registers.
        // We use paired registers when we are loading a double precision value (TFloatData is 64 bits) and we are in legacy mode.
        bool legacyMode = sizeof(TFloat) == sizeof(uint) || !_cpu.ControlCoProcessor.FloatingPoint64BitMode;
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
            EmitLoadRegisterAddress(il, register);

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
            EmitLoadRegisterAddress(il, register + 1);

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
            EmitLoadRegisterAddress(il, register);
            emitEvaluation();
            il.EmitStind<TFloatData>();
        }
    }


    /// <inheritdoc/>
    protected override void EmitSetupLocalRegisters(ILGenerator il)
    {
    }

    /// <inheritdoc/>
    protected override void EmitFlushLocalRegisters(ILGenerator il)
    {
    }

    private void EmitLoadRegisterAddress(ILGenerator il, MipsFloatRegister register) => EmitLoadRegisterAddress(il, (int)register, _fpu.RegisterFile.Regs);

    private void ReservedInstruction(ILGenerator il, MipsFloatInstruction inst, T pc) => EmitTrapRet(il, MipsTrap.ReservedInstruction, pc);
}
