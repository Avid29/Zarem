// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Emulator.JIT;

public unsafe partial class RiscVJitCompiler<T, TFloat>
{
    private void FloatAlu<TFormat>(ILGenerator il, RiscVFloatInstruction inst, OpCode ilOpCode)
        where TFormat : unmanaged
    {
        EmitStoreRegister<TFormat>(il, inst.FRD, () =>
        {
            EmitLoadRegister<TFormat>(il, inst.FRS1);
            EmitLoadRegister<TFormat>(il, inst.FRS1);
            il.Emit(ilOpCode);
        });
    }

    private void EmitLoadRegister<TFloatData>(ILGenerator il, RiscVFloatRegister register)
        where TFloatData : unmanaged
    {
        // Load the register's address then retrieve the value at that address
        EmitLoadRegisterAddress(il, register);
        il.EmitLdind<TFloatData>();
    }

    private void EmitStoreRegister<TFloatData>(ILGenerator il, RiscVFloatRegister register, Action emitEvaluation)
        where TFloatData : unmanaged
    {
        // Load the register's address, emit the evaluation instructions, and store the value
        EmitLoadRegisterAddress(il, register);
        emitEvaluation();
        il.EmitStind<TFloatData>();
    }
    
    private void EmitLoadRegisterAddress(ILGenerator il, RiscVFloatRegister register)
    {
        Guard.IsNotNull(_cpu.FloatRegisterFile);

        EmitLoadRegisterAddress(il, (int)register, _cpu.FloatRegisterFile.Regs);
    }
}
