// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models.JIT;

public unsafe partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private LocalBuilder[] _regLocals = [];

    private void EmitSetupLocalRegisters(ILGenerator il)
    {
        // Setup local registers
        _regLocals = new LocalBuilder[_cpu.RegisterFile.Count];
        for (int i = 1; i < _cpu.RegisterFile.Count; i++)
            _regLocals[i] = il.DeclareLocal(typeof(T));

        // Load read registers
        foreach (var reg in _loadRegs)
        {
            if (reg is MipsGpRegister.Zero)
                continue;

            // Load register from memory into local i
            EmitStoreRegister(il, reg, () =>
            {
                EmitLoadRegisterAddress(il, reg);
                EmitLdind(il);
            });
        }
    }

    private void EmitFlushLocalRegisters(ILGenerator il)
    {
        foreach (var reg in _storeRegs)
        {
            if (reg is MipsGpRegister.Zero)
                continue;

            // Load register local i into memory
            EmitLoadRegisterAddress(il, reg);
            EmitLoadRegister(il, reg);
            EmitStind(il);
        }
    }

    private void EmitDelaySlot(ILGenerator il, T delaySlotPc)
    {
        uint rawInstr = _cpu.Memory.Read<uint>(ulong.CreateTruncating(delaySlotPc));
        MipsInstruction instr = (MipsInstruction)rawInstr;

        // We dispatch to our existing table to emit the IL for this instruction
        // Note: MIPS forbids putting a jump/branch inside a delay slot!
        CompileInstruction(il, instr, delaySlotPc);
    }

    private void EmitLoadRegister(ILGenerator il, MipsGpRegister register)
        => EmitLoadRegister<T>(il, register);

    private void EmitLoadRegister<TData>(ILGenerator il, MipsGpRegister register)
        where TData : unmanaged, INumber<TData>
    {
        if (register is 0)
        {
            // MIPS $zero is always 0. 
            // We push a constant 0 instead of looking at memory.
            EmitLoadConstant(il, TData.Zero);
            return;
        }

        // Load the register's address then retrieve the value at that address
        il.Emit(OpCodes.Ldloc, _regLocals[(int)register]);

        // Convert the value to TData if neccesary
        if (sizeof(T) != sizeof(TData))
            EmitConv<TData>(il);
    }

    private void EmitLoadRegister<TFloat>(ILGenerator il, MipsFloatRegister register)
        where TFloat : unmanaged
    {
        // Load the register's address then retrieve the value at that address
        EmitLoadRegisterAddress(il, register);
        EmitLdind<TFloat>(il);
    }

    private void EmitStoreRegister(ILGenerator il, MipsGpRegister register, Action emitEvaluation)
    {
        if (register is 0)
        {
            // $zero cannot be written to.
            // We still emit the value calculation in case it has side effects,
            // then we immediately pop it off the stack.
            emitEvaluation();
            il.Emit(OpCodes.Pop);
            return;
        }

        // Load the register's address, emit the evaluation instructions, and store the value
        emitEvaluation();
        il.Emit(OpCodes.Stloc, _regLocals[(int)register]);
    }

    private void EmitStoreRegister<TFloat>(ILGenerator il, MipsFloatRegister register, Action emitEvaluation)
        where TFloat : unmanaged
    {
        // Load the register's address, emit the evaluation instructions, and store the value
        EmitLoadRegisterAddress(il, register);
        emitEvaluation();
        EmitStind<TFloat>(il);
    }

    private void EmitLoadRegisterAddress(ILGenerator il, MipsGpRegister register) => EmitLoadRegisterAddress(il, (int)register, _cpu.RegisterFile.Regs);

    private void EmitLoadRegisterAddress(ILGenerator il, MipsFloatRegister register) => EmitLoadRegisterAddress(il, (int)register, _cpu.FloatProcessor.RegisterFile.Regs);

    private static void EmitLoadRegisterAddress(ILGenerator il, int index, T* regs)
    {
        nint regAddress = (nint)regs + (index * sizeof(T));

        if (nint.Size == 8) il.Emit(OpCodes.Ldc_I8, regAddress);
        else if (nint.Size == 4) il.Emit(OpCodes.Ldc_I4, (int)regAddress);
        else throw new PlatformNotSupportedException($"Unsupported pointer size: {nint.Size}");

        il.Emit(OpCodes.Conv_U);
    }

    /// <remarks>
    /// Set <paramref name="accessFailureTrap"/> to <see cref="MipsTrap.None"/> to skip alignment check.
    /// </remarks>
    private LocalBuilder EmitLoadEffectiveAddress<TData>(ILGenerator il, MipsInstruction inst, T pc, MipsTrap accessFailureTrap = MipsTrap.None)
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
        if (accessFailureTrap is not MipsTrap.None && size > 1)
        {
            Label labelAligned = il.DefineLabel();
            il.Emit(OpCodes.Ldloc, addrVar);
            il.Emit(OpCodes.Ldc_I4, size - 1);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Ldc_I8, 0L);
            il.Emit(OpCodes.Beq, labelAligned);

            // Trap: Address Error Load
            EmitTrapArg(il, accessFailureTrap);
            EmitLoadConstant(il, pc);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(labelAligned);
        }

        return addrVar;
    }

    private void EmitRet(ILGenerator il, T pc) => EmitTrapRet(il, MipsTrap.None, pc);

    private void EmitRet(ILGenerator il, Action<ILGenerator> pushAddress)
    {
        EmitFlushLocalRegisters(il);
        EmitTrapArg(il, MipsTrap.None);
        pushAddress(il);
        il.Emit(OpCodes.Ret);
    }

    private static void EmitTrapArg(ILGenerator il, MipsTrap trap)
    {
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldc_I4, (int)trap);
        il.Emit(OpCodes.Stind_I1);
    }

    private void EmitTrapRet(ILGenerator il, MipsTrap trap, T pc)
    {
        EmitFlushLocalRegisters(il);
        EmitTrapArg(il, trap);
        EmitLoadConstant(il, pc);
        il.Emit(OpCodes.Ret);
    }

    private static void EmitLdind(ILGenerator il) => EmitLdind<T>(il);

    private static void EmitLdind<TData>(ILGenerator il)
    {
        if (typeof(TData) == typeof(int)) il.Emit(OpCodes.Ldind_I4);
        else if (typeof(TData) == typeof(uint)) il.Emit(OpCodes.Ldind_I4);
        else if (typeof(TData) == typeof(float)) il.Emit(OpCodes.Ldind_R4);
        else if (typeof(TData) == typeof(long)) il.Emit(OpCodes.Ldind_I8);
        else if (typeof(TData) == typeof(ulong)) il.Emit(OpCodes.Ldind_I8);
        else if (typeof(TData) == typeof(double)) il.Emit(OpCodes.Ldind_R8);
        else throw new NotSupportedException("Unsupported register width.");
    }

    private static void EmitStind(ILGenerator il) => EmitStind<T>(il);

    private static void EmitStind<TData>(ILGenerator il)
        where TData : unmanaged
    {
        if (typeof(TData) == typeof(int)) il.Emit(OpCodes.Stind_I4);
        else if (typeof(TData) == typeof(uint)) il.Emit(OpCodes.Stind_I4);
        else if (typeof(TData) == typeof(float)) il.Emit(OpCodes.Stind_R4);
        else if (typeof(TData) == typeof(long)) il.Emit(OpCodes.Stind_I8);
        else if (typeof(TData) == typeof(ulong)) il.Emit(OpCodes.Stind_I8);
        else if (typeof(TData) == typeof(double)) il.Emit(OpCodes.Stind_R8);
        else throw new NotSupportedException("Unsupported register width.");
    }

    private static void EmitConv(ILGenerator il)
        => EmitConv<T>(il);

    private static void EmitConv<TData>(ILGenerator il)
    {
        if (typeof(TData) == typeof(sbyte)) il.Emit(OpCodes.Conv_I1);
        else if (typeof(TData) == typeof(byte)) il.Emit(OpCodes.Conv_U1);
        else if (typeof(TData) == typeof(short)) il.Emit(OpCodes.Conv_I2);
        else if (typeof(TData) == typeof(ushort)) il.Emit(OpCodes.Conv_U2);
        else if (typeof(TData) == typeof(int)) il.Emit(OpCodes.Conv_I4);
        else if (typeof(TData) == typeof(uint)) il.Emit(OpCodes.Conv_U4);
        else if (typeof(TData) == typeof(float)) il.Emit(OpCodes.Conv_R4);
        else if (typeof(TData) == typeof(long)) il.Emit(OpCodes.Conv_I8);
        else if (typeof(TData) == typeof(ulong)) il.Emit(OpCodes.Conv_U8);
        else if (typeof(TData) == typeof(double)) il.Emit(OpCodes.Conv_R8);
    }

    private static void EmitLoadConstant<TData>(ILGenerator il, TData value)
        where TData : unmanaged, INumber<TData>
    {
        if (typeof(TData) == typeof(int) || typeof(TData) == typeof(uint))
        {
            var iValue = int.CreateTruncating(value);
            var opCode = iValue switch
            {
                -1 => OpCodes.Ldc_I4_M1,
                0 => OpCodes.Ldc_I4_0,
                1 => OpCodes.Ldc_I4_1,
                2 => OpCodes.Ldc_I4_2,
                3 => OpCodes.Ldc_I4_3,
                4 => OpCodes.Ldc_I4_4,
                5 => OpCodes.Ldc_I4_5,
                6 => OpCodes.Ldc_I4_6,
                7 => OpCodes.Ldc_I4_7,
                8 => OpCodes.Ldc_I4_8,
                >= sbyte.MinValue and <= sbyte.MaxValue => OpCodes.Ldc_I4_S,
                _ => OpCodes.Ldc_I4,
            };

            if (opCode == OpCodes.Ldc_I4) il.Emit(opCode, iValue);
            else if (opCode == OpCodes.Ldc_I4_S) il.Emit(opCode, (sbyte)iValue);
            else il.Emit(opCode);
        }
        else if (typeof(TData) == typeof(long) || typeof(TData) == typeof(ulong))
        {
            long lValue = long.CreateTruncating(value);

            // Optimization: If the 64-bit constant fits in a 32-bit integer, load the integer and convert.
            // The theory here is that this allows what would be a 9 byte instruction to become either a 2-6 byte
            // instruction, resulting in a smaller CIL JIT for a change that is optimized away by the CLR. Discuss.
            if (lValue >= int.MinValue && lValue <= int.MaxValue)
            {
                EmitLoadConstant(il, (int)lValue);
                il.Emit(OpCodes.Conv_I8);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I8, lValue);
            }
        }
        else
        {
            throw new NotSupportedException("Unsupported register width.");
        }
    }

    private static void EmitOverflowGuard<TData>(ILGenerator il, T pc, LocalBuilder rs, LocalBuilder rtOrImm, LocalBuilder result, Label noOverflow, bool isSubtraction = false)
        where TData : unmanaged, INumber<TData>
    {
        // Logic: ((rs ^ result) & (rtOrImm ^ result)) < 0  (for Addition)
        // Logic: ((rs ^ result) & (rs ^ rtOrImm)) < 0      (for Subtraction)

        // First term: (rs ^ result)
        il.Emit(OpCodes.Ldloc, rs);
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Xor);

        // Second term:
        if (isSubtraction)
        {
            // (rs ^ rtOrImm)
            il.Emit(OpCodes.Ldloc, rs);
            il.Emit(OpCodes.Ldloc, rtOrImm);
            il.Emit(OpCodes.Xor);
        }
        else
        {
            // (rtOrImm ^ result)
            il.Emit(OpCodes.Ldloc, rtOrImm);
            il.Emit(OpCodes.Ldloc, result);
            il.Emit(OpCodes.Xor);
        }

        il.Emit(OpCodes.And);

        // Check sign bit
        EmitLoadConstant(il, TData.Zero);

        il.Emit(OpCodes.Bge, noOverflow);

        // Trap Path (ends block)
        EmitTrapArg(il, MipsTrap.ArithmeticOverflow);
        EmitLoadConstant(il, pc);
        il.Emit(OpCodes.Ret);
    }
}
