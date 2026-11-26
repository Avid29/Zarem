// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.JIT;
using Zarem.Emulator.JIT.Compilers;
using Zarem.RiscV.Emulator.Helper;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Registers;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.RiscV.Emulator.JIT;

/// <summary>
/// A class which compiles blocks of RISC-V code into JIT IL code.
/// </summary>
public unsafe partial class RiscVJitCompiler<T, TFloat> : JitCompiler<T, RiscVJitBlock<T, TFloat>, RiscVGpRegister, RiscVTrap>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    private delegate bool RiscVEmitter(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed);

    private readonly RiscVInstructionDecodeTable<RiscVEmitter> _instructionTable;

    private readonly InstructionDecompressor? _decompressor;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVJitCompiler{T, TFloat}"/> class.
    /// </summary>
    public RiscVJitCompiler(RiscVJitCpu<T, TFloat> cpu) : base(cpu)
    {
        Cpu = cpu;

        _instructionTable = new RiscVInstructionDecodeTable<RiscVEmitter>(Cpu.Config.VersionInfo.Base, IllegalInstruction);
        InitTables(cpu.Config);

        // Initialize the decompressor if the compression extension is in use
        if (cpu.Config.VersionInfo.HasExtensions(RiscVExtensions.Compressed))
            _decompressor = new InstructionDecompressor(cpu.Config);
    }

    /// <inheritdoc/>
    public override RiscVJitCpu<T, TFloat> Cpu { get; }

    /// <summary>
    /// Compiles a single instruction as a cli dynamic method.
    /// </summary>
    public RiscVBlockDelegate<T, TFloat> CompileLoneInstruction(RiscVInstruction inst, T pc)
    {
        Type[] parameterTypes = [typeof(RiscVJitCpu<T, TFloat>), typeof(RiscVTrap).MakeByRefType()];
        var method = new DynamicMethod($"Insert_0x{pc:X}", typeof(T), parameterTypes, true);
        var il = method.GetILGenerator();

        RiscVInstruction decompressed = inst;
        if (inst.IsCompressed)
        {
            _decompressor?.Decompress((RiscVCompressedInstruction)inst, out decompressed);
        }

        var context = new JitBlockContext<T, RiscVGpRegister, RiscVTrap>(il, pc);
        bool isControlFlow = CompileInstruction(context, decompressed, pc, inst.IsCompressed);

        if (!isControlFlow)
        {
            EmitRet(context, pc + T.CreateTruncating(inst.IsCompressed ? 2 : 4));
        }

        context.EmitSetupLocalRegisters(this);
        return (RiscVBlockDelegate<T, TFloat>)method.CreateDelegate(typeof(RiscVBlockDelegate<T, TFloat>));
    }

    /// <summary>
    /// Compiles a single instruction as part of a <see cref="ILGenerator"/> block.
    /// </summary>
    protected override bool CompileNextInstruction(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, ref T pc)
    {
        var instruction = Fetch(pc, out var decompressed);
        bool isControlFlow = CompileInstruction(context, decompressed, pc, instruction.IsCompressed);
        pc += T.CreateTruncating(instruction.IsCompressed ? 2 : 4);
        return isControlFlow;
    }

    /// <inheritdoc/>
    protected override RiscVJitBlock<T, TFloat> CreateBlockDelegate(DynamicMethod method, JitBlockContext<T, RiscVGpRegister, RiscVTrap> context)
    {
        var @delegate = (RiscVBlockDelegate<T, TFloat>)method.CreateDelegate(typeof(RiscVBlockDelegate<T, TFloat>));
        return new(@delegate, context.End - context.Start);
    }

    private RiscVInstruction Fetch(T pc, out RiscVInstruction decompressed)
    {
        var inst = (RiscVInstruction)Cpu.Memory.Read<uint>(ulong.CreateTruncating(pc));
        decompressed = inst;

        if (inst.IsCompressed)
        {
            _decompressor?.Decompress((RiscVCompressedInstruction)inst, out decompressed);
        }

        return inst;
    }

    private bool CompileInstruction(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed)
    {
        var func = _instructionTable.Lookup(inst);
        return func(context, inst, pc, compressed);
    }

    private bool AluR<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, OpCode ilOpCode, bool not = false)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            EmitLoadRegister<TData>(context, inst.RS1);
            EmitLoadRegister<TData>(context, inst.RS2);
            if (not)
            {
                il.Emit(OpCodes.Not);
            }

            il.Emit(ilOpCode);
        });

        // Convert to T if neccesary
        context.ILGenerator.EmitConv<TData, T>(IsSigned<TData>());
        return false;
    }

    private bool AluI<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            EmitLoadRegister<TData>(context, inst.RS1);
            il.EmitLoadConstant<int>(inst.Immediate);
            il.Emit(ilOpCode);
        });
        return false;
    }

    private bool ShiftI<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            EmitLoadRegister<TData>(context, inst.RS1);
            il.EmitLoadConstant(inst.Immediate & (sizeof(TData) * 8 - 1));
            il.Emit(ilOpCode);
        });
        return false;
    }

    private bool MulH<TData, TLong>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst)
        where TData : unmanaged, INumber<TData>
        where TLong : unmanaged, INumber<TLong>
    {
        Type longType = typeof(TLong);
        int shiftAmount = sizeof(TData) * 8;

        // Primitive types can use native CIL hardware opcodes
        if (longType.IsPrimitive)
        {
            EmitStoreRegister(context, inst.RD, il =>
            {
                EmitLoadRegister<TLong>(context, inst.RS1);
                EmitLoadRegister<TLong>(context, inst.RS2);
                il.Emit(OpCodes.Mul);
                il.EmitLoadConstant(shiftAmount);
                il.Emit(OpCodes.Shr_Un);
                il.EmitConv<TData>();
            });
        }
        else
        {
            // For non-primitive types, we need to call methods to perform the multiplication and shifting
            // Resolve those methods
            var mulMethod = longType.GetMethod("op_Multiply", [longType, longType]);
            var shrMethod = longType.GetMethod("op_UnsignedRightShift", [longType, typeof(int)]);
            var convUpDef = typeof(TLong)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(TLong.CreateTruncating) && m.IsGenericMethod);
            var convUpMethod = convUpDef.MakeGenericMethod(typeof(TData));
            var convDownDef = typeof(TData)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(TData.CreateTruncating) && m.IsGenericMethod);
            var convDownMethod = convDownDef.MakeGenericMethod(typeof(TLong));
            Guard.IsNotNull(mulMethod);
            Guard.IsNotNull(shrMethod);
            Guard.IsNotNull(convUpMethod);
            Guard.IsNotNull(convDownMethod);

            EmitStoreRegister(context, inst.RD, il =>
            {
                // Load and widen both operands to TLong
                EmitLoadRegister<TData>(context, inst.RS1);
                il.Emit(OpCodes.Call, convUpMethod);
                EmitLoadRegister<TData>(context, inst.RS2);
                il.Emit(OpCodes.Call, convUpMethod);

                // TLong multiplication and shift
                il.Emit(OpCodes.Call, mulMethod);
                il.EmitLoadConstant(shiftAmount);
                il.Emit(OpCodes.Call, shrMethod);

                // Narrow back down to TData
                il.Emit(OpCodes.Call, convDownMethod);
            });

        }

        return false;
    }

    private bool MulSH<TData, TLong, TLongSigned>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst)
        where TData : unmanaged, INumber<TData>, IUnsignedNumber<TData>
        where TLong : unmanaged, INumber<TLong>, IUnsignedNumber<TLong>
        where TLongSigned : unmanaged, INumber<TLongSigned>, ISignedNumber<TLongSigned>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            EmitLoadRegister<TLongSigned>(context, inst.RS1);
            EmitLoadRegister<TLong>(context, inst.RS2);
            il.Emit(OpCodes.Mul);

            int shiftAmount = sizeof(TData) * 8;
            il.EmitLoadConstant(shiftAmount);
            il.Emit(OpCodes.Shr_Un);
            il.EmitConv<TData>();
        });

        return false;
    }

    private bool JumpAndLink(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed)
        => JumpAndLink(context, inst, pc, compressed, pushAddress: il =>
        {
            var target = pc + T.CreateTruncating(inst.JumpOffset);
            il.EmitLoadConstant(target);
        });

    private bool JumpAndLinkRegister(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed)
        => JumpAndLink(context, inst, pc, compressed, pushAddress: il =>
        {
            EmitLoadRegister(context, inst.RS1);

            // Add offset if non-zero
            if (inst.Immediate is not 0)
            {
                il.EmitLoadConstant(T.CreateTruncating(inst.Immediate));
                il.Emit(OpCodes.Add);
            }

            // Clear the least significant bit
            il.EmitLoadConstant(T.CreateTruncating(~1L));
            il.Emit(OpCodes.And);
        });

    private bool JumpAndLink(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed, Action<ILGenerator> pushAddress)
    {
        // Link if needed
        if (inst.RD is not RiscVGpRegister.Zero)
        {
            EmitStoreRegister(context, inst.RD, il =>
            {
                var returnAddress = pc + T.CreateTruncating(compressed ? 2 : 4);
                il.EmitLoadConstant(returnAddress);
            });
        }

        EmitRet(context, pushAddress);
        return true;
    }

    private bool Load<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed)
        where TData : unmanaged
    {
        var il = context.ILGenerator;
        var addrVar = EmitLoadEffectiveAddress<TData>(context, inst, pc, inst.Immediate, RiscVTrap.LoadAddressMisaligned);

        // Allocate a local variable to receive the 'out TData value'
        var dataVar = il.DeclareLocal(typeof(TData));

        // Call Memory.Read<TData>(ulong)
        var readMethod = TryReadMethods[typeof(TData)];
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, GetMemoryMethod);
        il.Emit(OpCodes.Ldloc, addrVar);                // Arg 1: ulong addr
        il.Emit(OpCodes.Conv_U8);
        il.Emit(OpCodes.Ldloca, dataVar);               // Arg 2: out TData value
        il.Emit(OpCodes.Callvirt, readMethod);

        // TODO: Evaulate access result
        il.Emit(OpCodes.Pop);

        // Write Back to RD
        EmitStoreRegister(context, inst.RD, il =>
        {
            // Load the value filled by TryRead
            il.Emit(OpCodes.Ldloc, dataVar);

            // Sign-Extension / Zero-Extension then convert to T
            il.EmitConv<TData>();
            il.EmitConv<T>();
        });

        return false;
    }

    private bool Store<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed)
        where TData : unmanaged
    {
        var il = context.ILGenerator;
        var addrVar = EmitLoadEffectiveAddress<TData>(context, inst, pc, inst.StoreOffset, RiscVTrap.StoreAddressMisaligned);

        // Call Memory.Write<TData>(ulong, TData)
        var writeMethod = TryWriteMethods[typeof(TData)];
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, GetMemoryMethod);
        il.Emit(OpCodes.Ldloc, addrVar);                // Arg 1: ulong addr
        il.Emit(OpCodes.Conv_U8);
        EmitLoadRegister(context, inst.RS2);                 // Arg 2: TData value (Truncate the RS2 register value)
        il.EmitConv<TData>();
        il.Emit(OpCodes.Callvirt, writeMethod);

        // TODO: Evaulate access result
        il.Emit(OpCodes.Pop);

        return false;
    }

    private bool Branch(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed, OpCode conditionCode)
    {
        var il = context.ILGenerator;
        Label takeBranch = il.DefineLabel();

        EmitLoadRegister(context, inst.RS1);
        EmitLoadRegister(context, inst.RS2);
        il.Emit(conditionCode, takeBranch);

        // Branch NOT taken
        var nextPc = pc + T.CreateTruncating(compressed ? 2 : 4);
        EmitRet(context, nextPc);

        // Branch taken
        il.MarkLabel(takeBranch);

        T targetPc = pc + T.CreateTruncating(inst.BranchOffset);
        EmitRet(context, targetPc);
        return true;
    }

    private bool Lui(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed)
    {
        uint value = (uint)inst.Immediate << 12;

        EmitStoreRegister(context, inst.RD, il => il.EmitLoadConstant(T.CreateTruncating(value)));
        return false;
    }

    private bool IllegalInstruction(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed)
    {
        EmitTrapRet(context, RiscVTrap.IllegalInstruction, pc);
        return false;
    }

    private bool MethodBinary<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, string methodName)
        where TData : unmanaged, INumber<TData>
        => MethodBinary<TData, TData>(context, inst, methodName);

    private bool MethodBinary<TData, TData2>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, string methodName)
        where TData : unmanaged, INumber<TData>
        where TData2 : unmanaged, INumber<TData2>
    {
        MethodInfo? method = typeof(TData).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, [typeof(TData), typeof(TData2)]);
        Guard.IsNotNull(method);
        MethodBinary<T, TData2>(context, inst, il => il.Emit(OpCodes.Call, method));
        return false;
    }

    private bool MethodUnary<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, string methodName)
        where TData : unmanaged, INumber<TData>
    {
        MethodInfo? method = typeof(TData).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, [typeof(TData)]);
        Guard.IsNotNull(method);
        MethodUnary<T>(context, inst, il => il.Emit(OpCodes.Call, method));
        return false;
    }

    private bool MethodBinary<TData, TData2>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, Action<ILGenerator> method)
        where TData : unmanaged, INumber<TData>
        where TData2 : unmanaged, INumber<TData2>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            EmitLoadRegister<TData>(context, inst.RS1);
            EmitLoadRegister<TData2>(context, inst.RS2);
            method(il);

            // Convert to T if neccesary
            if (sizeof(TData) != sizeof(T))
                il.EmitConv<T>(IsSigned<TData>());
        });

        return false;
    }

    private bool MethodUnary<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, Action<ILGenerator> method)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            EmitLoadRegister<TData>(context, inst.RS1);
            method(il);

            // Convert to T if neccesary
            if (sizeof(TData) != sizeof(T))
                il.EmitConv<T>(IsSigned<TData>());
        });

        return false;
    }
}
