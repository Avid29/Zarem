// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
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
        var method = new DynamicMethod($"Block_0x{startPc:X}", typeof(T), [typeof(MipsJitCpu<T>)], true);
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
        var method = new DynamicMethod($"Insert_0x{pc:X}", typeof(T), [typeof(MipsJitCpu<T>)], true);
        var il = method.GetILGenerator();
        bool ended = CompileInstruction(il, inst, pc);

        if (!ended)
        {
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

    private bool Shift(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, OpCode? followUp = null)
    {
        EmitRegisterWrite(il, inst.RD, () =>
        {
            EmitRegisterRead(il, inst.RT);
            il.Emit(OpCodes.Ldc_I4, (int)inst.ShiftAmount);
            il.Emit(ilOpCode);
        });

        return false;
    }

    private bool ShiftPlus32(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, OpCode? followUp = null)
    {
        EmitRegisterWrite(il, inst.RD, () =>
        {
            EmitRegisterRead(il, inst.RT);
            il.Emit(OpCodes.Ldc_I4, inst.ShiftAmount + 32);
            il.Emit(ilOpCode);
        });

        return false;
    }

    private bool ShiftVar(ILGenerator il, MipsInstruction inst, OpCode ilOpCode)
    {
        EmitRegisterWrite(il, inst.RD, () =>
        {
            EmitRegisterRead(il, inst.RT); // Value to shift
            EmitRegisterRead(il, inst.RS); // Shift amount from register

            // Ensure the shift amount is treated as an int for the IL stack
            if (typeof(T) == typeof(ulong))
                il.Emit(OpCodes.Conv_I4);

            il.Emit(ilOpCode);
        });
        return false;
    }

    private bool AluR(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, OpCode? followUp = null)
    {
        EmitRegisterWrite(il, inst.RD, () =>
        {
            EmitRegisterRead(il, inst.RS);
            EmitRegisterRead(il, inst.RT);
            il.Emit(ilOpCode);

            if (followUp.HasValue)
            {
                il.Emit(followUp.Value);
            }
        });

        return false;
    }

    private bool AluI(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, bool signExtend = false)
    {
        // Fetch the raw immediate from the instruction
        short rawImm = inst.Immediate;
        T extended = signExtend ? T.CreateTruncating((long)rawImm) : T.CreateTruncating((ulong)rawImm);

        EmitRegisterWrite(il, inst.RT, () =>
        {
            EmitRegisterRead(il, inst.RS);
            EmitLoadConstant(il, extended);

            il.Emit(ilOpCode);
        });

        return false;
    }

    private bool MultR(ILGenerator il, MipsInstruction inst, bool signed)
    {
        // Retrieve the rs/rt registers
        EmitRegisterRead(il, inst.RS);
        il.Emit(signed ? OpCodes.Conv_I8 : OpCodes.Conv_U8);
        EmitRegisterRead(il, inst.RT);
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
        EmitRegisterRead(il, inst.RS);
        LocalBuilder rsLocal = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, rsLocal);
        EmitRegisterRead(il, inst.RT);
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

    private bool Jump(ILGenerator il, MipsInstruction inst, T pc, bool link = false)
    {
        if (link)
        {
            // Store the Return Address ($ra = PC + 8)
            // We use +8 because +4 is the delay slot, and we want to return AFTER that.
            T returnAddr = pc + T.CreateTruncating(8);
            EmitRegisterWrite(il, MipsGpRegister.ReturnAddress, () => EmitLoadConstant(il, returnAddr));
        }

        if (!_cpu.Config.DisableDelaySlots)
        {
            // Handle the Delay Slot
            T delaySlotPc = pc + T.CreateTruncating(4);
            EmitDelaySlot(il, delaySlotPc);
        }

        // Calculate the Jump Target
        T targetPc = T.CreateTruncating(inst.Address);

        // Exit the block by returning the new PC
        EmitLoadConstant(il, targetPc);
        il.Emit(OpCodes.Ret);

        return true; // Signals the compiler that this block is finished
    }

    private bool JumpR(ILGenerator il, MipsInstruction inst, T pc, bool link = false)
    {
        if (link)
        {
            // Store the Return Address ($ra = PC + 8)
            // We use +8 because +4 is the delay slot, and we want to return AFTER that.
            T returnAddr = pc + T.CreateTruncating(8);
            EmitRegisterWrite(il, MipsGpRegister.ReturnAddress, () => EmitLoadConstant(il, returnAddr));
        }

        if (!_cpu.Config.DisableDelaySlots)
        {
            // Handle Delay Slot
            EmitDelaySlot(il, pc + T.CreateTruncating(4));
        }

        // Read the target from the register and return it
        EmitRegisterRead(il, inst.RS);
        il.Emit(OpCodes.Ret);

        return true;
    }

    [DynamicDependency(nameof(MipsJitCpu<>.HandleTrap), typeof(MipsJitCpu<>))]
    private bool Trap(ILGenerator il, MipsInstruction inst, T pc, MipsTrap trap)
    {
        // Push arguments:
        // this, trap, currentPc
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4, (int)trap);
        EmitLoadConstant(il, T.CreateTruncating(pc));

        var handleMethod = typeof(MipsJitCpu<T>).GetMethod(nameof(MipsJitCpu<>.HandleTrap));
        var pcGetter = typeof(MipsJitCpu<T>).GetProperty(nameof(MipsJitCpu<>.ProgramCounter))?.GetGetMethod();
#if DEBUG
        Guard.IsNotNull(handleMethod);
        Guard.IsNotNull(pcGetter);
#endif
        il.Emit(OpCodes.Call, handleMethod);

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, pcGetter);
        il.Emit(OpCodes.Ret);

        return true; // Terminate the IL block here
    }

    private bool MoveFromTo(ILGenerator il, MipsGpRegister from, MipsGpRegister to)
    {
        // Can't writeback to $zero.
        // Skip as no-op
        if (to is MipsGpRegister.Zero)
            return false;

        EmitRegisterWrite(il, to, () =>
        {
            EmitRegisterRead(il, from);
        });

        return false;
    }

    private bool Lui(ILGenerator il, MipsInstruction inst)
    {
        uint value = (uint)inst.Immediate << 16;

        EmitRegisterWrite(il, inst.RT, () =>
        {
            EmitLoadConstant(il, T.CreateTruncating(value));
        });

        return false;
    }
}
