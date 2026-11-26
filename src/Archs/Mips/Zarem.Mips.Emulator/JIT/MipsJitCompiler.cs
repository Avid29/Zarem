// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.JIT;
using Zarem.Emulator.JIT.Compilers;
using Zarem.Emulator.Models.Enums;
using Zarem.Mips.Emulator.JIT;
using Zarem.Mips.Emulator.Machine;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Operations;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models.JIT;

/// <summary>
/// A class which compiles blocks of MIPS code into JIT IL code.
/// </summary>
public unsafe partial class MipsJitCompiler<T> : JitCompiler<T, MipsJitBlock<T>, MipsGpRegister, MipsTrap>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private delegate bool MipsEmitter(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc);

    private readonly MipsInstructionDecodeTable<MipsEmitter> _instructionTable;

    private readonly MethodInfo _clzMethod;
    private readonly Dictionary<Type, MethodInfo> _multiplyMethod = [];
    private readonly Dictionary<Type, MethodInfo> _castDownMethods = [];
    private readonly Dictionary<Type, MethodInfo> _rightShiftMethods = [];
    private readonly MethodInfo _handleAccessResultMethod;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsJitCompiler{T}"/> class.
    /// </summary>
    public MipsJitCompiler(MipsCpu<T> cpu) : base(cpu)
    {
        Cpu = cpu;

        _instructionTable = new MipsInstructionDecodeTable<MipsEmitter>(ReservedInstruction);

        var clzMethod = typeof(BitOperations).GetMethod(nameof(BitOperations.LeadingZeroCount), [typeof(uint)]);
        Guard.IsNotNull(clzMethod);
        _clzMethod = clzMethod;

        var handleAccessResultMethod = typeof(MipsJitCompiler<T>).GetMethod(nameof(HandleMemoryAccessResult), BindingFlags.Static | BindingFlags.NonPublic);
        Guard.IsNotNull(handleAccessResultMethod);
        _handleAccessResultMethod = handleAccessResultMethod;

        Type[] multiplyTypes = [typeof(int), typeof(uint), typeof(long), typeof(ulong)];
        foreach (var type in multiplyTypes)
        {
            var method = typeof(Math).GetMethod(nameof(Math.BigMul), [type, type]);
            Guard.IsNotNull(method);
            _multiplyMethod[type] = method;
        }

        (Type, Type)[] bigTypePairs = [(typeof(Int128), typeof(long)), (typeof(UInt128), typeof(ulong))];
        foreach (var (type, pair) in bigTypePairs)
        {
            var castDownMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "op_Explicit" && m.ReturnType == pair);
            var rightShiftMethod = type.GetMethod("op_RightShift", [type, typeof(int)]);
            Guard.IsNotNull(castDownMethod);
            Guard.IsNotNull(rightShiftMethod);
            _castDownMethods[type] = castDownMethod;
            _rightShiftMethods[type] = rightShiftMethod;
        }

        InitTables(Cpu.Config);
    }

    /// <inheritdoc/>
    public override MipsCpu<T> Cpu { get; }

    /// <summary>
    /// Compiles a single instruction as a cli dynamic method.
    /// </summary>
    public MipsBlockDelegate<T> CompileLoneInstruction(MipsInstruction inst, T pc)
    {
        Type[] parameterTypes = [typeof(MipsCpu<T>), typeof(MipsTrap).MakeByRefType()];
        var method = new DynamicMethod($"Insert_0x{pc:X}", typeof(T), parameterTypes, true);
        var il = method.GetILGenerator();
        var context = new JitBlockContext<T, MipsGpRegister, MipsTrap>(il, pc);
        
        bool isControlFlow = CompileInstruction(context, inst, pc);

        if (!isControlFlow)
        {
            EmitRet(context, pc + T.CreateTruncating(4));
        }

        context.EmitSetupLocalRegisters(this);

#if DEBUG
        byte[] ilBytes = GetDynamicMethodILBytes(il);
        Debug.WriteLine($"[JIT Dump] {method.Name}: {BitConverter.ToString(ilBytes)}");
#endif
        return (MipsBlockDelegate<T>)method.CreateDelegate(typeof(MipsBlockDelegate<T>));
    }

    /// <inheritdoc/>
    protected override MipsJitBlock<T> CreateBlockDelegate(DynamicMethod method, JitBlockContext<T, MipsGpRegister, MipsTrap> context)
    {
        var @delegate = (MipsBlockDelegate<T>)method.CreateDelegate(typeof(MipsBlockDelegate<T>));
        return new MipsJitBlock<T>(@delegate, context.End - context.Start);
    }

    /// <summary>
    /// Compiles a single instruction as part of a <see cref="ILGenerator"/> block.
    /// </summary>
    protected override bool CompileNextInstruction(JitBlockContext<T, MipsGpRegister, MipsTrap> context, ref T pc)
    {
        var instruction = Fetch(pc);
        bool isControlFlow = CompileInstruction(context, instruction, pc);
        pc += T.CreateTruncating(4);
        return isControlFlow;
    }

    /// <summary>
    /// Compiles a single instruction as part of a <see cref="ILGenerator"/> block.
    /// </summary>
    private bool CompileInstruction(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction instruction, T pc)
    {
        var emitter = _instructionTable.Lookup(instruction);
        return emitter(context, instruction, pc);
    }

    private MipsInstruction Fetch(T pc) => (MipsInstruction)Cpu.Memory.Read<uint>(ulong.CreateTruncating(pc));

    private bool Shift<TData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            EmitLoadRegister<TData>(context, inst.RT);
            il.Emit(OpCodes.Ldc_I4, (int)inst.ShiftAmount);
            il.Emit(ilOpCode);
        });
        return false;
    }

    private bool ShiftPlus32<TData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            EmitLoadRegister<TData>(context, inst.RT);
            il.Emit(OpCodes.Ldc_I4, inst.ShiftAmount + 32);
            il.Emit(ilOpCode);
        });
        return false;
    }

    private bool ShiftVar<TData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            EmitLoadRegister<TData>(context, inst.RT); // Value to shift
            EmitLoadRegister<TData>(context, inst.RS); // Shift amount from register

            // Ensure the shift amount is treated as an int for the IL stack
            if (typeof(TData) == typeof(ulong))
                il.Emit(OpCodes.Conv_I4);

            il.Emit(ilOpCode);
        });
        return false;
    }

    private bool AluR<TData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, OpCode ilOpCode, OpCode? followUp = null)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            EmitLoadRegister<TData>(context, inst.RS);
            EmitLoadRegister<TData>(context, inst.RT);
            il.Emit(ilOpCode);

            if (followUp.HasValue)
            {
                il.Emit(followUp.Value);
            }

            // Convert to T if neccesary
            context.ILGenerator.EmitConv<TData, T>(IsSigned<TData>());
        });
        return false;
    }

    private bool CheckedAluR<TData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc, OpCode ilOpCode, bool isSubtraction)
        where TData : unmanaged, INumber<TData>
    {
        var il = context.ILGenerator;
        Label noOverflow = il.DefineLabel();

        // Get register locals
        // NOTE: This is safe because these registers will not be written to
        LocalBuilder rs = GetRegisterLocal(context, inst.RS);
        LocalBuilder rt = GetRegisterLocal(context, inst.RT);

        // Calculate
        EmitLoadRegister<TData>(il, rs);
        EmitLoadRegister<TData>(il, rt);
        il.Emit(ilOpCode);

        // Store result
        LocalBuilder result = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, result);

        // Overflow Guard
        EmitOverflowGuard<TData>(il, rs, rt, result, () =>
        {
            EmitTrapRet(context, MipsTrap.ArithmeticOverflow, pc);
        }, isSubtraction);

        // Safe Path
        EmitStoreRegister(context, inst.RD, il =>
        {
            il.Emit(OpCodes.Ldloc, result);
            il.EmitConv<TData, T>(Sign.Signed);
        });
        return false;
    }

    private bool AluI<TData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, OpCode ilOpCode, bool signExtend = false)
        where TData : unmanaged, INumber<TData>
    {
        // Fetch the raw immediate from the instruction
        var rawImm = inst.Immediate;
        var extended = signExtend ? TData.CreateTruncating((long)rawImm) : TData.CreateTruncating((ulong)rawImm);

        EmitStoreRegister(context, inst.RT, il =>
        {
            EmitLoadRegister<TData>(context, inst.RS);
            il.EmitLoadConstant(extended);

            il.Emit(ilOpCode);
        });
        return false;
    }

    private bool CheckedAluI<TData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        var il = context.ILGenerator;

        // Get RS register local
        LocalBuilder rs = GetRegisterLocal(context, inst.RS);

        // Load Immediate into local (Sign-extended)
        il.EmitLoadConstant(TData.CreateTruncating(inst.Immediate));
        LocalBuilder imm = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, imm);

        // Calculate
        EmitLoadRegister<TData>(il, rs);
        EmitLoadRegister<TData>(il, imm);
        il.Emit(ilOpCode);

        // Store result
        LocalBuilder result = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, result);

        // Overflow Guard
        EmitOverflowGuard<TData>(il, rs, imm, result, () =>
        {
            EmitTrapRet(context, MipsTrap.ArithmeticOverflow, pc);
        });

        // Safe Path
        EmitStoreRegister(context, inst.RT, il =>
        {
            il.Emit(OpCodes.Ldloc, result);
            if (sizeof(TData) != sizeof(T))
            {
                il.EmitConv<T>(Sign.Signed);
            }
        });
        return false;
    }

    private bool MultR<TData, TLong>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, int c = 0)
        where TData : unmanaged, INumber<TData>
        where TLong : unmanaged, INumber<TLong>
    {
        var il = context.ILGenerator;

        // Retrieve the rs/rt registers
        EmitLoadRegister<TData>(context, inst.RS);
        EmitLoadRegister<TData>(context, inst.RT);

        // Apply multiplication and store rsult as a local
        var localResult = il.DeclareLocal(typeof(TLong));
        il.Emit(OpCodes.Call, _multiplyMethod[typeof(TData)]);
        il.Emit(OpCodes.Stloc, localResult);

        int shiftAmount = sizeof(TData) * 8;
        bool bigLong = sizeof(TLong) > sizeof(long);

        // Store high
        EmitStoreRegister(context, MipsGpRegister.High, il =>
        {
            if (c is not 0)
            {
                EmitLoadRegister(context, MipsGpRegister.High);
            }

            il.Emit(OpCodes.Ldloc, localResult);
            il.EmitLoadConstant(shiftAmount);

            if (bigLong)
            {
                il.Emit(OpCodes.Call, _rightShiftMethods[typeof(TLong)]);
                il.Emit(OpCodes.Call, _castDownMethods[typeof(TLong)]);
            }
            else
            {
                il.Emit(OpCodes.Shr_Un);
                il.EmitConv<TData>();
            }

            if (c is not 0)
            {
                il.Emit(c is > 0 ? OpCodes.Add : OpCodes.Sub);
            }

            il.EmitConv<T>(IsSigned<TData>());
        });

        // Store low
        EmitStoreRegister(context, MipsGpRegister.Low, il =>
        {
            if (c is not 0)
            {
                EmitLoadRegister(context, MipsGpRegister.Low);
            }

            il.Emit(OpCodes.Ldloc, localResult);

            if (bigLong)
            {
                il.Emit(OpCodes.Call, _castDownMethods[typeof(TLong)]);
            }
            else
            {
                il.EmitConv<TData>();
            }

            if (c is not 0)
            {
                il.Emit(c is > 0 ? OpCodes.Add : OpCodes.Sub);
            }

            il.EmitConv<T>();
        });
        return false;
    }

    private bool DivR<TData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, bool signed)
        where TData : unmanaged, INumber<TData>
    {
        var il = context.ILGenerator;
        Label endDiv = il.DefineLabel();

        // Load operands into locals to keep stack predictable
        var rs = GetRegisterLocal(context, inst.RS);
        var rt = GetRegisterLocal(context, inst.RT);

        // Guard against Div-By-Zero
        EmitLoadRegister<TData>(il, rt);
        il.EmitLoadConstant(TData.Zero);
        il.Emit(OpCodes.Beq, endDiv);

        // Calculate and store the remainder to High
        EmitStoreRegister(context, MipsGpRegister.High, il =>
        {
            EmitLoadRegister<TData>(il, rs);
            EmitLoadRegister<TData>(il, rt);
            il.Emit(signed ? OpCodes.Rem : OpCodes.Rem_Un);
            if (sizeof(TData) != sizeof(T))
            {
                il.EmitConv<T>(signed ? Sign.Signed : Sign.Unsigned);
            }
        });

        // Calculate and store the quotient to low
        EmitStoreRegister(context, MipsGpRegister.Low, il =>
        {
            EmitLoadRegister<TData>(il, rs);
            EmitLoadRegister<TData>(il, rt);
            il.Emit(signed ? OpCodes.Div : OpCodes.Div_Un);
            if (sizeof(TData) != sizeof(T))
            {
                il.EmitConv<T>(signed ? Sign.Signed : Sign.Unsigned);
            }
        });

        il.MarkLabel(endDiv);
        return false;
    }

    private bool Load<TData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc)
        where TData : unmanaged
    {
        var il = context.ILGenerator;
        var addrVar = EmitLoadEffectiveAddress<TData>(context, inst, pc, MipsTrap.AddressErrorLoad);
        var trapVar = il.DeclareLocal(typeof(MipsTrap));

        // Allocate a local variable to receive the 'out TData value'
        var dataVar = il.DeclareLocal(typeof(TData));

        // Call Memory.TryRead<TData>(ulong, out TData)
        var tryReadMethod = TryReadMethods[typeof(TData)];
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, GetMemoryMethod);
        il.Emit(OpCodes.Ldloc, addrVar);                // Arg 1: ulong addr
        il.Emit(OpCodes.Conv_U8);
        il.Emit(OpCodes.Ldloca, dataVar);               // Arg 2: out TData value
        il.Emit(OpCodes.Callvirt, tryReadMethod);       // Call TryRead, returns MemoryAccessResult

        // Evaluate access result
        il.EmitLoadBool(false);                             // isWrite = false
        il.Emit(OpCodes.Call, _handleAccessResultMethod);   // Convert to MIPS Trap
        il.Emit(OpCodes.Stloc, trapVar);

        // Branch to success path if no trap occured
        var successLabel = il.DefineLabel();
        il.Emit(OpCodes.Ldloc, trapVar);
        il.Emit(OpCodes.Brfalse, successLabel);

        // Trap path. Return trap
        EmitTrapRet(context, trapVar, pc);

        // Success path! Write back to RT
        il.MarkLabel(successLabel);
        EmitStoreRegister(context, inst.RT, il =>
        {
            // Load the value filled by TryRead
            il.Emit(OpCodes.Ldloc, dataVar);

            // Sign-Extension / Zero-Extension then convert to T
            il.EmitConv<TData>();
            il.EmitConv<T>();
        });
        return false;
    }

    private bool Store<TData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc)
        where TData : unmanaged
    {
        var addrVar = EmitLoadEffectiveAddress<TData>(context, inst, pc, MipsTrap.AddressErrorStore);
        var il = context.ILGenerator;
        var trapVar = il.DeclareLocal(typeof(MipsTrap));

        // Call Memory.TryWrite<TData>(ulong, TData)
        var tryWriteMethod = TryWriteMethods[typeof(TData)];
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, GetMemoryMethod);
        il.Emit(OpCodes.Ldloc, addrVar);            // Arg 1: ulong addr
        il.Emit(OpCodes.Conv_U8);
        EmitLoadRegister(context, inst.RT);              // Arg 2: TData value (Truncate the RT register value)
        il.EmitConv<TData>();
        il.Emit(OpCodes.Callvirt, tryWriteMethod);  // Call TryRead, returns MemoryAccessResult

        // Evaluate access result
        il.EmitLoadBool(true);                              // isWrite = true
        il.Emit(OpCodes.Call, _handleAccessResultMethod);   // Convert to MIPS Trap
        il.Emit(OpCodes.Stloc, trapVar);

        // Branch to success path if no trap occured
        var successLabel = il.DefineLabel();
        il.Emit(OpCodes.Ldloc, trapVar);
        il.Emit(OpCodes.Brfalse, successLabel);

        // Trap path. Return trap
        EmitTrapRet(context, trapVar, pc);

        // Success
        il.MarkLabel(successLabel);
        return false;
    }

    private bool Jump(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc, bool link = false)
        => Jump(context, pc, link: link, pushAddress: il =>
    {
        il.EmitLoadConstant(T.CreateTruncating(inst.Address));
    });

    private bool JumpR(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc, bool link = false)
        => Jump(context, pc, link: link, pushAddress: il =>
    {
        EmitLoadRegister(context, inst.RS);
    });

    private bool Jump(JitBlockContext<T, MipsGpRegister, MipsTrap> context, T pc, Action<ILGenerator> pushAddress, bool link = false)
    {
        bool delaySlots = !Cpu.Config.DisableDelaySlots;

        if (link)
        {
            // Store the Return Address ($ra = PC + 8)
            // We use +8 because +4 is the delay slot, and we want to return AFTER that.
            T returnAddr = pc + (delaySlots ? T.CreateTruncating(8) : T.CreateTruncating(4));
            EmitStoreRegister(context, MipsGpRegister.ReturnAddress, il => il.EmitLoadConstant(returnAddr));
        }

        // Handle the Delay Slot
        if (delaySlots)
        {
            T delaySlotPc = pc + T.CreateTruncating(4);
            EmitDelaySlot(context, delaySlotPc);
        }

        // Exit the block by returning the new PC
        EmitRet(context, pushAddress);
        return true;
    }

    private bool BranchCompareReg(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc, OpCode conditionOpCode, bool likely = false)
        => Branch(context, inst, pc, conditionOpCode, likely: likely, pushOperands: il =>
        {
            EmitLoadRegister(context, inst.RS);
            EmitLoadRegister(context, inst.RT);
        });

    private bool BranchCompareZero(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc, OpCode conditionOpCode, bool likely = false)
        => Branch(context, inst, pc, conditionOpCode, likely: likely, pushOperands: il =>
        {
            EmitLoadRegister(context, inst.RS);
            il.EmitLoadConstant(T.Zero);
        });

    private bool Branch(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc, OpCode conditionOpCode, Action<ILGenerator> pushOperands, bool likely = false)
    {
        bool delaySlotsEnabled = !Cpu.Config.DisableDelaySlots;

        var il = context.ILGenerator;
        Label takeBranch = il.DefineLabel();

        // Evaluate the branch condition
        pushOperands(il);
        il.Emit(conditionOpCode, takeBranch);

        // Branch NOT taken
        if (delaySlotsEnabled && !likely)
        {
            EmitDelaySlot(context, pc + T.CreateTruncating(4));
        }

        // Branch NOT taken
        var nextPc = pc + (delaySlotsEnabled ? T.CreateTruncating(8) : T.CreateTruncating(4));
        EmitRet(context, nextPc);

        // Branch taken
        // In both Likely and Normal modes, the delay slot executes if the branch is taken.
        il.MarkLabel(takeBranch);

        if (delaySlotsEnabled)
        {
            EmitDelaySlot(context, pc + T.CreateTruncating(4));
        }

        T targetPc = pc + T.CreateTruncating(4) + T.CreateTruncating(inst.Offset);
        EmitRet(context, targetPc);
        return true;
    }

    private bool TrapCompareReg(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc, OpCode invertedBranch)
        => ConditionalTrap(context, inst, pc, invertedBranch, il =>
    {
        EmitLoadRegister(context, inst.RS);
        EmitLoadRegister(context, inst.RT);
    });

    private bool TrapCompareImmediate(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc, OpCode invertedBranch)
        => ConditionalTrap(context, inst, pc, invertedBranch, il =>
    {
        EmitLoadRegister(context, inst.RS);
        il.EmitLoadConstant(T.CreateTruncating(inst.Immediate));
    });

    private bool ConditionalTrap(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc, OpCode invertedBranch, Action<ILGenerator> pushOperands)
    {
        var il = context.ILGenerator;
        Label noTrap = il.DefineLabel();

        // Evaluate the trap condition
        pushOperands(il);
        il.Emit(invertedBranch, noTrap);

        // DO trap
        EmitTrapRet(context, MipsTrap.Trap, pc);

        // Do NOT trap
        il.MarkLabel(noTrap);
        EmitRet(context, pc);
        return false;
    }

    private bool Move(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, OpCode invertedBranch)
    {
        var il = context.ILGenerator;
        Label noMove = il.DefineLabel();

        EmitLoadRegister(context, inst.RT);
        il.Emit(invertedBranch, noMove);

        // DO move
        EmitStoreRegister(context, inst.RD, il => EmitLoadRegister(context, inst.RS));

        // Do NOT move
        il.MarkLabel(noMove);
        return false;
    }

    private bool MoveFromTo(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsGpRegister from, MipsGpRegister to)
    {
        // Can't writeback to $zero.
        // Skip as no-op
        if (to is MipsGpRegister.Zero)
            return false;

        EmitStoreRegister(context, to, il => EmitLoadRegister(context, from));
        return false;
    }

    private bool Lui(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc)
    {
        uint value = (uint)inst.Immediate << 16;

        EmitStoreRegister(context, inst.RT, il => il.EmitLoadConstant(T.CreateTruncating(value)));
        return false;
    }

    private bool ExecuteCoProc(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc)
    {
        var coProcIndex = inst.OpCode switch
        {
            MipsOpCode.Coprocessor0 => 0,
            MipsOpCode.Coprocessor1 => 1,
            MipsOpCode.Coprocessor2 => 2,
            MipsOpCode.Coprocessor3 => 3,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<int>(),
        };

        var coProc = Cpu.CoProcessors[coProcIndex];
        coProc.Executor.CompileInstruction(context, inst, pc);
        return false;
    }

    private bool ReservedInstruction(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc)
    {
        EmitTrapRet(context, MipsTrap.ReservedInstruction, pc);
        return false;
    }

    private bool MethodUnary<TData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, Action<ILGenerator> method)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            EmitLoadRegister<TData>(context, inst.RS);

            method(il);

            // Convert to T if neccesary
            context.ILGenerator.EmitConv<TData, T>();
        });
        return false;
    }

    private static MipsTrap HandleMemoryAccessResult(MemoryAccessResult result, bool isWrite)
    {
        return result switch
        {
            MemoryAccessResult.Success => MipsTrap.None,

            MemoryAccessResult.TranslationFault when isWrite => MipsTrap.TlbMissStore,
            MemoryAccessResult.TranslationFault => MipsTrap.TlbMissLoad,

            MemoryAccessResult.AddressError when isWrite => MipsTrap.AddressErrorStore,
            MemoryAccessResult.AddressError => MipsTrap.AddressErrorLoad,

            MemoryAccessResult.AccessViolation when isWrite => MipsTrap.AddressErrorStore,
            MemoryAccessResult.AccessViolation => MipsTrap.AddressErrorLoad,

            _ => ThrowHelper.ThrowArgumentOutOfRangeException<MipsTrap>(),
        };
    }

    private static byte[] GetDynamicMethodILBytes(ILGenerator il)
    {
        // Reflection fallback to read the underlying baked byte stream from DynamicMethod's ILGenerator
        var bakeMethod = il.GetType().GetMethod("BakeByteArray", BindingFlags.NonPublic | BindingFlags.Instance);
        if (bakeMethod?.Invoke(il, null) is byte[] bytes)
        {
            return bytes;
        }
        return Array.Empty<byte>();
    }
}
