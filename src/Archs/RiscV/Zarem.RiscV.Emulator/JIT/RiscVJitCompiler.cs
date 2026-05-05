// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Extensions;
using Zarem.RiscV.Emulator.JIT;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A class which compiles blocks of RISC-V code into JIT IL code.
/// </summary>
public unsafe partial class RiscVJitCompiler<T> : JitCompiler<T, RiscVGpRegister, RiscVTrap>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private delegate void RiscVEmitter(ILGenerator il, RiscVInstruction inst, T pc);

    // Tables
    private readonly RiscVEmitter[][] _func7Table = new RiscVEmitter[128][];
    private readonly RiscVEmitter[] _emptyTable = new RiscVEmitter[1024];

    private readonly RiscVJitCpu<T> _cpu;

    private readonly HashSet<RiscVGpRegister> _loadRegs = [];
    private readonly HashSet<RiscVGpRegister> _storeRegs = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVJitCompiler{T}"/> class.
    /// </summary>
    public RiscVJitCompiler(RiscVJitCpu<T> cpu) : base(cpu)
    {
        _cpu = cpu;

        InitTables(cpu.Config);
    }

    /// <summary>
    /// Compiles a block of RISC-V code to JIT starting at <paramref name="startPc"/>.
    /// </summary>
    /// <param name="startPc">The entry point of the JIT block.</param>
    /// <returns>The method block.</returns>
    public RiscVJitBlock<T> CompileBlock(T startPc)
    {
        Type[] parameterTypes = [typeof(RiscVJitCpu<T>), typeof(RiscVTrap).MakeByRefType()];
        var method = new DynamicMethod($"Block_0x{startPc:X}", typeof(T), parameterTypes, true);
        var il = method.GetILGenerator();

        var endPc = DiscoverBlock(startPc);
        ScanRegisterUsage(startPc, endPc);
        EmitSetupLocalRegisters(il);

        T currentPc = startPc;

        while (currentPc < endPc)
        {
            CompileInstruction(il, Fetch(currentPc), currentPc);
            currentPc += T.CreateTruncating(4);
        }

        var @delegate = (RiscVBlockDelegate<T>)method.CreateDelegate(typeof(RiscVBlockDelegate<T>));
        return new(@delegate, int.CreateTruncating(endPc - startPc));
    }

    /// <summary>
    /// Compiles a single instruction as a cli dynamic method.
    /// </summary>
    public RiscVBlockDelegate<T> CompileLoneInstruction(RiscVInstruction inst, T pc)
    {
        Type[] parameterTypes = [typeof(RiscVJitCpu<T>), typeof(RiscVTrap).MakeByRefType()];
        var method = new DynamicMethod($"Insert_0x{pc:X}", typeof(T), parameterTypes, true);
        var il = method.GetILGenerator();

        ScanRegisterUsage(pc, pc);
        LogRegisterUsage(inst);
        EmitSetupLocalRegisters(il);
        CompileInstruction(il, inst, pc);

        if (!IsControlFlow(inst))
        {
            EmitRet(il, pc + T.CreateTruncating(4));
        }

        return (RiscVBlockDelegate<T>)method.CreateDelegate(typeof(RiscVBlockDelegate<T>));
    }

    private void CompileInstruction(ILGenerator il, RiscVInstruction inst, T pc)
    {
        var func7code = inst.OpCode is RiscVOpCode.Op or RiscVOpCode.Op32 or RiscVOpCode.Op64 ? inst.Funct7 : Funct7Code.Base;
        var table = _func7Table[(int)func7code];
        var func = table[GetLookupIndex(inst)];
        func(il, inst, pc);
    }

    private void AluR<TData>(ILGenerator il, RiscVInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, il =>
        {
            EmitLoadRegister<TData>(il, inst.RS1);
            EmitLoadRegister<TData>(il, inst.RS2);
            il.Emit(ilOpCode);
        });

        // Convert to T if neccesary
        if (sizeof(TData) != sizeof(T))
            il.EmitConv<T>(IsSigned<TData>());
    }

    private void AluI<TData>(ILGenerator il, RiscVInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, il =>
        {
            EmitLoadRegister<TData>(il, inst.RS1);
            il.EmitLoadConstant<int>(inst.Immediate);
            il.Emit(ilOpCode);
        });
    }

    private void ShiftI<TData>(ILGenerator il, RiscVInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, il =>
        {
            EmitLoadRegister<TData>(il, inst.RS1);
            il.EmitLoadConstant(inst.Immediate & (sizeof(TData) * 8 - 1));
            il.Emit(ilOpCode);
        });
    }

    private void MulH<TData, TLong>(ILGenerator il, RiscVInstruction inst)
        where TData : unmanaged, INumber<TData>
        where TLong : unmanaged, INumber<TLong>
    {
        EmitStoreRegister(il, inst.RD, il =>
        {
            EmitLoadRegister<TLong>(il, inst.RS1);
            EmitLoadRegister<TLong>(il, inst.RS2);
            il.Emit(OpCodes.Mul);

            int shiftAmount = sizeof(TData) * 8;
            il.EmitLoadConstant(shiftAmount);
            il.Emit(OpCodes.Shr_Un);
            il.EmitConv<TData>();
        });
    }

    private void MulSH<TData, TLong, TLongSigned>(ILGenerator il, RiscVInstruction inst)
        where TData : unmanaged, INumber<TData>, IUnsignedNumber<TData>
        where TLong : unmanaged, INumber<TLong>, IUnsignedNumber<TLong>
        where TLongSigned : unmanaged, INumber<TLongSigned>, ISignedNumber<TLongSigned>
    {
        EmitStoreRegister(il, inst.RD, il =>
        {
            EmitLoadRegister<TLongSigned>(il, inst.RS1);
            EmitLoadRegister<TLong>(il, inst.RS2);
            il.Emit(OpCodes.Mul);

            int shiftAmount = sizeof(TData) * 8;
            il.EmitLoadConstant(shiftAmount);
            il.Emit(OpCodes.Shr_Un);
            il.EmitConv<TData>();
        });
    }

    private void JumpAndLink(ILGenerator il, RiscVInstruction inst, T pc)
    {
        // Link if needed
        if (inst.RD is not RiscVGpRegister.Zero)
        {
            EmitStoreRegister(il, inst.RD, il =>
            {
                il.EmitLoadConstant(pc + T.CreateTruncating(4));
            });
        }

        // Return
        EmitRet(il, T.CreateTruncating(inst.JumpOffset));
    }

    private void JumpAndLinkRegister(ILGenerator il, RiscVInstruction inst, T pc)
    {
        // Link if needed
        if (inst.RD is not RiscVGpRegister.Zero)
        {
            EmitStoreRegister(il, inst.RD, il =>
            {
                EmitLoadRegister<T>(il, inst.RS1);
            });
        }

        // Return
        EmitRet(il, T.CreateTruncating(inst.JumpOffset));
    }

    private void Load<TData>(ILGenerator il, RiscVInstruction inst, T pc)
        where TData : unmanaged
    {
        var addrVar = EmitLoadEffectiveAddress<TData>(il, inst, pc, inst.Immediate, RiscVTrap.LoadAddressMisaligned);

        // Write Back to RD
        EmitStoreRegister(il, inst.RD, il =>
        {
            // Call Memory.Read<TData>(ulong)
            var readMethod = ReadMethods[typeof(TData)];
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, GetMemoryMethod);
            il.Emit(OpCodes.Ldloc, addrVar);                // Arg 1: ulong addr
            il.Emit(OpCodes.Conv_U8);
            il.Emit(OpCodes.Callvirt, readMethod);

            // Sign-Extension / Zero-Extension then convert to T
            il.EmitConv<TData>();
            il.EmitConv<T>();
        });
    }

    private void Store<TData>(ILGenerator il, RiscVInstruction inst, T pc)
        where TData : unmanaged
    {
        var addrVar = EmitLoadEffectiveAddress<TData>(il, inst, pc, inst.StoreOffset, RiscVTrap.StoreAddressMisaligned);

        // Call Memory.Write<TData>(ulong, TData)
        var writeMethod = WriteMethods[typeof(TData)];
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, GetMemoryMethod);
        il.Emit(OpCodes.Ldloc, addrVar);                // Arg 1: ulong addr
        il.Emit(OpCodes.Conv_U8);
        EmitLoadRegister(il, inst.RS2);                 // Arg 2: TData value (Truncate the RS2 register value)
        il.EmitConv<TData>();
        il.Emit(OpCodes.Callvirt, writeMethod);
    }

    private void Branch(ILGenerator il, RiscVInstruction inst, T pc, OpCode conditionCode)
    {
        Label takeBranch = il.DefineLabel();

        EmitLoadRegister(il, inst.RS1);
        EmitLoadRegister(il, inst.RS2);
        il.Emit(conditionCode, takeBranch);

        // Branch NOT taken
        var nextPc = pc + T.CreateTruncating(4);
        EmitRet(il, nextPc);

        // Branch taken
        il.MarkLabel(takeBranch);

        T targetPc = pc + T.CreateTruncating(inst.BranchOffset);
        EmitRet(il, targetPc);
    }

    private void Lui(ILGenerator il, RiscVInstruction inst, T pc)
    {
        uint value = (uint)inst.Immediate << 12;

        EmitStoreRegister(il, inst.RD, il => il.EmitLoadConstant(T.CreateTruncating(value)));
    }

    private void IllegalInstruction(ILGenerator il, RiscVInstruction inst, T pc) => EmitTrapRet(il, RiscVTrap.IllegalInstruction, pc);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLookupIndex(RiscVInstruction instruction)
        => GetLookupIndex(instruction.OpCode, instruction.Funct3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLookupIndex(RiscVOpCode op, Funct3Code funct3)
        => (int)op << 3 | (int)funct3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (int low, int high) GetLookupRange(RiscVOpCode op)
    {
        var low = (int)op << 3;
        var high = low | 0b111;
        return (low, high);
    }
}
