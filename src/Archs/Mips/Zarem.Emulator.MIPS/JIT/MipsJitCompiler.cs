// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
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
    public MipsJitCompiler(MipsJitCpu<T> cpu) => _cpu = cpu;

    /// <summary>
    /// Compiles a block of MIPS code to JIT starting at <paramref name="startPc"/>.
    /// </summary>
    /// <param name="startPc">The entry point of the JIT block.</param>
    /// <returns>The method block.</returns>
    public MipsBlockDelegate<T> CompileBlock(T startPc)
    {
        var method = new DynamicMethod($"Block_{startPc:X}", typeof(T), [typeof(MipsJitCpu<T>)], true);
        var il = method.GetILGenerator();

        T currentPc = startPc;
        bool isFinished = false;

        while (!isFinished)
        {
            var inst = (MipsInstruction)_cpu.Memory.Read<uint>(ulong.CreateTruncating(currentPc));
            var emitter = _opCodeTable[(int)inst.OpCode];
            isFinished = emitter(il, inst, currentPc);
        }

        return (MipsBlockDelegate<T>)method.CreateDelegate(typeof(MipsBlockDelegate<T>));
    }

    private bool AluI(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, bool signExtend = false)
    {
        // Fetch the raw immediate from the instruction
        short rawImm = (short)inst.Immediate;

        EmitRegisterWrite(il, inst.RT, () =>
        {
            EmitRegisterRead(il, inst.RS);

            // Handle the extension at JIT-time
            if (signExtend)
            {
                // Sign-extend 16-bit to T
                long extended = (long)rawImm;
                EmitLoadConstant(il, T.CreateTruncating(extended));
            }
            else
            {
                // Zero-extend 16-bit to T
                ulong extended = (ushort)rawImm;
                EmitLoadConstant(il, T.CreateTruncating(extended));
            }

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
}
