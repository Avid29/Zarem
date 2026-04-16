// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Machine.JIT;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A class which compiles blocks of MIPS code into JIT IL code.
/// </summary>
public unsafe partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private delegate bool MipsEmitter(ILGenerator il, MipsInstruction inst, T pc);

    private readonly MipsEmitter[] _opCodeTable = new MipsEmitter[64];
    private readonly MipsEmitter[] _specialTable = new MipsEmitter[64];
    private readonly MipsEmitter[] _special2Table = new MipsEmitter[64];
    private readonly MipsEmitter[] _regImmTable = new MipsEmitter[32];
    private readonly MipsJitCpu<T> _cpu;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsJitCompiler{T}"/> class.
    /// </summary>
    public MipsJitCompiler(MipsJitCpu<T> cpu)
    {
        _cpu = cpu;

        InitTables(_cpu.Config);
    }

    /// <summary>
    /// Compiles a block of MIPS code to JIT starting at <paramref name="startPc"/>.
    /// </summary>
    /// <param name="startPc">The entry point of the JIT block.</param>
    /// <returns>The method block.</returns>
    public MipsBlockDelegate<T> CompileBlock(T startPc)
    {
        Type[] parameterTypes = [typeof(MipsJitCpu<T>), typeof(MipsTrap).MakeByRefType()];
        var method = new DynamicMethod($"Block_0x{startPc:X}", typeof(T), parameterTypes, true);
        var il = method.GetILGenerator();

        T currentPc = startPc;
        bool isFinished = false;

        while (!isFinished)
        {
            var inst = (MipsInstruction)_cpu.Memory.Read<uint>(ulong.CreateTruncating(currentPc));
            isFinished = CompileInstruction(il, inst, currentPc);
            currentPc += T.CreateTruncating(4);
        }

        return (MipsBlockDelegate<T>)method.CreateDelegate(typeof(MipsBlockDelegate<T>));
    }

    /// <summary>
    /// Compiles a single instruction as a cli dynamic method.
    /// </summary>
    public MipsBlockDelegate<T> CompileLoneInstruction(MipsInstruction inst, T pc)
    {
        Type[] parameterTypes = [typeof(MipsJitCpu<T>), typeof(MipsTrap).MakeByRefType()];
        var method = new DynamicMethod($"Insert_0x{pc:X}", typeof(T), parameterTypes, true);
        var il = method.GetILGenerator();
        bool ended = CompileInstruction(il, inst, pc);

        if (!ended)
        {
            EmitTrapArg(il, MipsTrap.None);
            EmitLoadConstant(il, pc + T.CreateTruncating(4));
            il.Emit(OpCodes.Ret);
        }

        return (MipsBlockDelegate<T>)method.CreateDelegate(typeof(MipsBlockDelegate<T>));
    }

    private bool CompileInstruction(ILGenerator il, MipsInstruction inst, T pc)
    {
        var emitter = _opCodeTable[(int)inst.OpCode];
        return emitter(il, inst, pc);
    }

    private bool DispatchSpecial(ILGenerator il, MipsInstruction inst, T pc)
    {
        var emmiter = _specialTable[(int)inst.FuncCode];
        return emmiter(il, inst, pc);
    }

    private bool DispatchRegImm(ILGenerator il, MipsInstruction inst, T pc)
    {
        var emmiter = _regImmTable[(int)inst.RTFuncCode];
        return emmiter(il, inst, pc);
    }

    private bool Shift(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, OpCode? followUp = null)
    {
        EmitStoreRegister(il, inst.RD, () =>
        {
            EmitLoadRegister(il, inst.RT);
            il.Emit(OpCodes.Ldc_I4, (int)inst.ShiftAmount);
            il.Emit(ilOpCode);
        });

        return false;
    }

    private bool ShiftPlus32(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, OpCode? followUp = null)
    {
        EmitStoreRegister(il, inst.RD, () =>
        {
            EmitLoadRegister(il, inst.RT);
            il.Emit(OpCodes.Ldc_I4, inst.ShiftAmount + 32);
            il.Emit(ilOpCode);
        });

        return false;
    }

    private bool ShiftVar(ILGenerator il, MipsInstruction inst, OpCode ilOpCode)
    {
        EmitStoreRegister(il, inst.RD, () =>
        {
            EmitLoadRegister(il, inst.RT); // Value to shift
            EmitLoadRegister(il, inst.RS); // Shift amount from register

            // Ensure the shift amount is treated as an int for the IL stack
            if (typeof(T) == typeof(ulong))
                il.Emit(OpCodes.Conv_I4);

            il.Emit(ilOpCode);
        });
        return false;
    }

    private bool AluR(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, OpCode? followUp = null)
    {
        EmitStoreRegister(il, inst.RD, () =>
        {
            EmitLoadRegister(il, inst.RS);
            EmitLoadRegister(il, inst.RT);
            il.Emit(ilOpCode);

            if (followUp.HasValue)
            {
                il.Emit(followUp.Value);
            }
        });

        return false;
    }

    private bool CheckedAluR(ILGenerator il, MipsInstruction inst, T pc, OpCode ilOpCode, bool isSubtraction)
    {
        Label noOverflow = il.DefineLabel();

        // Load RS into local
        EmitLoadRegister(il, inst.RS);
        LocalBuilder rs = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, rs);

        // Load RT into local
        EmitLoadRegister(il, inst.RT);
        LocalBuilder rt = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, rt);

        // Calculate
        il.Emit(OpCodes.Ldloc, rs);
        il.Emit(OpCodes.Ldloc, rt);
        il.Emit(ilOpCode);
        LocalBuilder result = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, result);

        // Overflow Guard
        EmitOverflowGuard(il, pc, isSubtraction, rs, rt, result, noOverflow);

        // Safe Path
        il.MarkLabel(noOverflow);
        EmitStoreRegister(il, inst.RD, () => il.Emit(OpCodes.Ldloc, result));

        return false;
    }

    private bool AluI(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, bool signExtend = false)
    {
        // Fetch the raw immediate from the instruction
        short rawImm = inst.Immediate;
        T extended = signExtend ? T.CreateTruncating((long)rawImm) : T.CreateTruncating((ulong)rawImm);

        EmitStoreRegister(il, inst.RT, () =>
        {
            EmitLoadRegister(il, inst.RS);
            EmitLoadConstant(il, extended);

            il.Emit(ilOpCode);
        });

        return false;
    }

    private bool CheckedAluI(ILGenerator il, MipsInstruction inst, T pc, OpCode ilOpCode)
    {
        Label noOverflow = il.DefineLabel();

        // Load RS into local
        EmitLoadRegister(il, inst.RS);
        LocalBuilder rs = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, rs);

        // Load Immediate into local (Sign-extended)
        EmitLoadConstant(il, T.CreateTruncating(inst.Immediate));
        LocalBuilder imm = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, imm);

        // Calculate
        il.Emit(OpCodes.Ldloc, rs);
        il.Emit(OpCodes.Ldloc, imm);
        il.Emit(ilOpCode);
        LocalBuilder result = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, result);

        // Overflow Guard
        EmitOverflowGuard(il, pc, false, rs, imm, result, noOverflow);

        // Safe Path
        il.MarkLabel(noOverflow);
        EmitStoreRegister(il, inst.RT, () => il.Emit(OpCodes.Ldloc, result));

        return false;
    }

    private bool MultR(ILGenerator il, MipsInstruction inst, bool signed)
    {
        // Retrieve the rs/rt registers
        EmitLoadRegister(il, inst.RS);
        il.Emit(signed ? OpCodes.Conv_I8 : OpCodes.Conv_U8);
        EmitLoadRegister(il, inst.RT);
        il.Emit(signed ? OpCodes.Conv_I8 : OpCodes.Conv_U8);

        // Apply multiplication and store result as a local
        var localResult = il.DeclareLocal(typeof(long));
        il.Emit(OpCodes.Mul);
        il.Emit(OpCodes.Stloc, localResult);

        // Store high
        EmitLoadRegisterAddress(il, MipsGpRegister.High);
        il.Emit(OpCodes.Ldloc, localResult);
        il.Emit(OpCodes.Ldc_I4, 32);
        il.Emit(OpCodes.Shr_Un);
        il.Emit(OpCodes.Conv_U4);
        il.Emit(OpCodes.Stind_I4);

        // Store low
        EmitLoadRegisterAddress(il, MipsGpRegister.Low);
        il.Emit(OpCodes.Ldloc, localResult);
        il.Emit(OpCodes.Conv_U4);
        il.Emit(OpCodes.Stind_I4);

        return false;
    }

    private bool DivR(ILGenerator il, MipsInstruction inst, bool signed)
    {
        Label endDiv = il.DefineLabel();

        // Load operands into locals to keep stack predictable
        EmitLoadRegister(il, inst.RS);
        LocalBuilder rsLocal = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, rsLocal);
        EmitLoadRegister(il, inst.RT);
        LocalBuilder rtLocal = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, rtLocal);

        // Guard against Div-By-Zero
        il.Emit(OpCodes.Ldloc, rtLocal);
        il.Emit(OpCodes.Ldc_I4_0);
        if (sizeof(T) == 8)
        {
            il.Emit(OpCodes.Conv_I8); // Ensure width matches T
        }
        il.Emit(OpCodes.Beq, endDiv);

        // Calculate and store the remainder to High
        EmitLoadRegisterAddress(il, MipsGpRegister.High);
        il.Emit(OpCodes.Ldloc, rsLocal);
        il.Emit(OpCodes.Ldloc, rtLocal);
        il.Emit(signed ? OpCodes.Rem : OpCodes.Rem_Un);
        if (sizeof(T) == 4)
        {
            il.Emit(OpCodes.Conv_U4);
        }
        EmitStind(il);

        // Calculate and store the quotient to low
        EmitLoadRegisterAddress(il, MipsGpRegister.Low);
        il.Emit(OpCodes.Ldloc, rsLocal);
        il.Emit(OpCodes.Ldloc, rtLocal);
        il.Emit(signed ? OpCodes.Div : OpCodes.Div_Un);
        if (sizeof(T) == 4)
        {
            il.Emit(OpCodes.Conv_U4);
        }
        EmitStind(il);

        il.MarkLabel(endDiv);

        return false;
    }
    
    private bool Load<TData>(ILGenerator il, MipsInstruction inst, T pc)
        where TData : unmanaged
    {
        // Calculate Effective Address (rs + offset)
        EmitLoadRegister(il, inst.RS);
        il.Emit(OpCodes.Ldc_I8, (long)inst.Immediate);
        il.Emit(OpCodes.Add);

        var addrVar = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, addrVar);

        // Alignment Check
        int size = sizeof(TData);
        if (size > 1)
        {
            Label labelAligned = il.DefineLabel();
            il.Emit(OpCodes.Ldloc, addrVar);
            il.Emit(OpCodes.Ldc_I4, size - 1);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Ldc_I8, 0L);
            il.Emit(OpCodes.Beq, labelAligned);

            // Trap: Address Error Load
            EmitTrapArg(il, MipsTrap.AddressErrorLoad);
            EmitLoadConstant(il, pc);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(labelAligned);
        }

        // Write Back to RT
        EmitStoreRegister(il, inst.RT, () =>
        {
            var getMemoryMethod = _cpu.GetType().GetProperty("Memory")?.GetGetMethod();
#if DEBUG
            Guard.IsNotNull(getMemoryMethod);
#endif

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, getMemoryMethod);

            // Arg 1: ulong addr
            il.Emit(OpCodes.Ldloc, addrVar);
            il.Emit(OpCodes.Conv_U8);

            // 4. Call Memory.Read<TData>(ulong)
            var readMethodBase = _cpu.Memory.GetType()
                .GetMethods()
                .First(m => m.Name == "Read" && m.IsGenericMethod && m.GetParameters().Length == 1);
            var readMethod = readMethodBase.MakeGenericMethod(typeof(TData));

            il.Emit(OpCodes.Callvirt, readMethod);

            // Sign-Extension / Zero-Extension
            EmitConv<TData>(il);
            EmitConv(il);
        });

        return false;
    }

    private bool Store<TData>(ILGenerator il, MipsInstruction inst, T pc)
        where TData : unmanaged
    {
        // Calculate Effective Address: rs + offset
        EmitLoadRegister(il, inst.RS);
        il.Emit(OpCodes.Ldc_I8, (long)inst.Immediate); // Sign-extended
        il.Emit(OpCodes.Add);

        // Store in a local to avoid repeated math for alignment check
        var addrVar = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, addrVar);

        // Alignment Check
        int size = sizeof(TData);
        if (size > 1)
        {
            Label labelAligned = il.DefineLabel();

            // if ((addr & (size - 1)) != 0)
            il.Emit(OpCodes.Ldloc, addrVar);
            il.Emit(OpCodes.Ldc_I4, size - 1);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Ldc_I8, 0L);
            il.Emit(OpCodes.Beq, labelAligned);

            // Not Aligned: Return Trap
            EmitTrapArg(il, MipsTrap.AddressErrorStore);
            EmitLoadConstant(il, pc);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(labelAligned);
        }

        var getMemoryMethod = _cpu.GetType().GetProperty("Memory")?.GetGetMethod();
#if DEBUG
        Guard.IsNotNull(getMemoryMethod);
#endif

        // Perform Memory Write
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, getMemoryMethod);

        // Arg 1: ulong addr
        il.Emit(OpCodes.Ldloc, addrVar);
        il.Emit(OpCodes.Conv_U8);

        // Arg 2: TData value (Truncate the RT register value)
        EmitLoadRegister(il, inst.RT);
        EmitConv<TData>(il);

        // Find the generic method definition "Write<T>(ulong, T)"
        var writeMethodBase = _cpu.Memory.GetType()
            .GetMethods()
            .First(m => m.Name == "Write" && m.IsGenericMethod && m.GetParameters().Length == 2);

        // Specialize it for the current TData (byte, ushort, uint, etc.)
        var writeMethod = writeMethodBase.MakeGenericMethod(typeof(TData));

#if DEBUG
        Guard.IsNotNull(writeMethod);
#endif

        il.Emit(OpCodes.Callvirt, writeMethod);

        return false; // Does not complete the block
    }

    private bool Jump(ILGenerator il, MipsInstruction inst, T pc, bool link = false)
    {
        bool delaySlots = !_cpu.Config.DisableDelaySlots;

        if (link)
        {
            // Store the Return Address ($ra = PC + 8)
            // We use +8 because +4 is the delay slot, and we want to return AFTER that.
            T returnAddr = pc + (delaySlots ? T.CreateTruncating(8) : T.CreateTruncating(4));
            EmitStoreRegister(il, MipsGpRegister.ReturnAddress, () => EmitLoadConstant(il, returnAddr));
        }

        // Handle the Delay Slot
        if (delaySlots)
        {
            T delaySlotPc = pc + T.CreateTruncating(4);
            EmitDelaySlot(il, delaySlotPc);
        }

        // Calculate the Jump Target
        T targetPc = T.CreateTruncating(inst.Address);

        // Exit the block by returning the new PC
        EmitTrapArg(il, MipsTrap.None);
        EmitLoadConstant(il, targetPc);
        il.Emit(OpCodes.Ret);

        return true; // Signals the compiler that this block is finished
    }

    private bool JumpR(ILGenerator il, MipsInstruction inst, T pc, bool link = false)
    {
        bool delaySlots = !_cpu.Config.DisableDelaySlots;

        if (link)
        {
            // Store the Return Address ($ra = PC + 8)
            // We use +8 because +4 is the delay slot, and we want to return AFTER that.
            T returnAddr = pc + (delaySlots ? T.CreateTruncating(8) : T.CreateTruncating(4));
            EmitStoreRegister(il, MipsGpRegister.ReturnAddress, () => EmitLoadConstant(il, returnAddr));
        }

        if (delaySlots)
        {
            // Handle Delay Slot
            EmitDelaySlot(il, pc + T.CreateTruncating(4));
        }

        // Read the target from the register and return it
        EmitTrapArg(il, MipsTrap.None);
        EmitLoadRegister(il, inst.RS);
        il.Emit(OpCodes.Ret);

        return true;
    }

    private static bool Trap(ILGenerator il, T pc, MipsTrap trap)
    {
        EmitTrapArg(il, trap);
        EmitLoadConstant(il, pc);
        il.Emit(OpCodes.Ret);

        return true; // Terminate the IL block here
    }

    private bool Branch(ILGenerator il, MipsInstruction inst, T pc, OpCode conditionOpCode, Action<ILGenerator> pushOperands, bool likely = false)
    {
        bool delaySlots = !_cpu.Config.DisableDelaySlots;

        Label takeBranch = il.DefineLabel();

        // Prepare the stack for the branch condition
        // This calls the delegate to push RS, RT, or RS and 0.
        pushOperands(il);

        // Append delay slot operation
        if (!delaySlots && !likely)
        {
            EmitDelaySlot(il, pc + T.CreateTruncating(4));
        }

        // Evaluate the branch condition
        il.Emit(conditionOpCode, takeBranch);

        // Branch NOT taken
        EmitTrapArg(il, MipsTrap.None);
        EmitLoadConstant(il, pc + (delaySlots ? T.CreateTruncating(8) : T.CreateTruncating(4)));
        il.Emit(OpCodes.Ret);

        // Branch taken
        il.MarkLabel(takeBranch);
        long offset = (long)inst.Immediate << 2;
        T targetPc = pc + T.CreateTruncating(4) + T.CreateTruncating(offset);

        EmitTrapArg(il, MipsTrap.None);
        EmitLoadConstant(il, targetPc);
        il.Emit(OpCodes.Ret);

        return true;
    }

    private bool MoveFromTo(ILGenerator il, MipsGpRegister from, MipsGpRegister to)
    {
        // Can't writeback to $zero.
        // Skip as no-op
        if (to is MipsGpRegister.Zero)
            return false;

        EmitStoreRegister(il, to, () =>
        {
            EmitLoadRegister(il, from);
        });

        return false;
    }

    private bool Lui(ILGenerator il, MipsInstruction inst)
    {
        uint value = (uint)inst.Immediate << 16;

        EmitStoreRegister(il, inst.RT, () =>
        {
            EmitLoadConstant(il, T.CreateTruncating(value));
        });

        return false;
    }
}
