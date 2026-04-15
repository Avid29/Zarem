// Avishai Dernis 2026

using System.Numerics;
using System.Reflection.Emit;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A class which compiles blocks of MIPS code into JIT IL code.
/// </summary>
public partial class MipsJitCompiler<T>
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
            il.Emit(OpCodes.Ldc_I4, inst.ShiftAmount);
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

    private bool Jump(ILGenerator il, MipsInstruction inst, T pc, bool link = false)
    {
        if (link)
        {
            // Store the Return Address ($ra = PC + 8)
            // We use +8 because +4 is the delay slot, and we want to return AFTER that.
            T returnAddr = pc + T.CreateTruncating(8);
            EmitRegisterWrite(il, MipsGpRegister.ReturnAddress, () => EmitLoadConstant(il, returnAddr));
        }

        // Handle the Delay Slot
        T delaySlotPc = pc + T.CreateTruncating(4);
        EmitDelaySlot(il, delaySlotPc);

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

        // Handle Delay Slot
        EmitDelaySlot(il, pc + T.CreateTruncating(4));

        // Read the target from the register and return it
        EmitRegisterRead(il, inst.RS);
        il.Emit(OpCodes.Ret);

        return true;
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
