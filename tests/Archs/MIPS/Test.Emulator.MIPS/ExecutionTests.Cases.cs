// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models.Enum;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Registers;

namespace Test.Emulator.MIPS;

public partial class ExecutionTests
{
    public static IEnumerable<object[]> InstructionTestList_Mips1
        => GetVersionTests(MipsVersion.MipsI);
    
    public static IEnumerable<object[]> InstructionTestList_Mips2
        => GetVersionTests(MipsVersion.MipsII);
    
    public static IEnumerable<object[]> InstructionTestList_Mips3
        => GetVersionTests(MipsVersion.MipsIII);
    
    public static IEnumerable<object[]> InstructionTestList_Mips3_32Bit
        => GetVersionTests(MipsVersion.MipsIII_32Bit);
    
    public static IEnumerable<object[]> InstructionTestList_Mips4
        => GetVersionTests(MipsVersion.MipsIV);
    
    public static IEnumerable<object[]> InstructionTestList_Mips4_32Bit
        => GetVersionTests(MipsVersion.MipsIV_32Bit);
    
    public static IEnumerable<object[]> InstructionTestList_Mips5
        => GetVersionTests(MipsVersion.MipsV);
    
    public static IEnumerable<object[]> InstructionTestList_Mips5_32Bit
        => GetVersionTests(MipsVersion.MipsV_32Bit);
    
    public static IEnumerable<object[]> InstructionTestList_Mips32R1
        => GetVersionTests(MipsVersion.Mips32R1);
    
    public static IEnumerable<object[]> InstructionTestList_Mips32R2
        => GetVersionTests(MipsVersion.Mips32R2);

    private static IEnumerable<object[]> GetVersionTests(MipsVersion version)
    {
        foreach (var test in GetArithmeticInstructionTests(version))
            yield return test;

        foreach (var test in GetLogicalInstructionTests(version))
            yield return test;

        foreach (var test in GetMemoryInstructionTests(version))
            yield return test;

        foreach (var test in GetJumpBranchInstructionTests(version))
            yield return test;

        foreach (var test in GetCompareInstructionTests(version))
            yield return test;

        foreach (var test in GetTrapInstructionTests(version))
            yield return test;

        foreach (var test in GetUncategorizedRegisterOnlyInstructionTests(version))
            yield return test;

        foreach (var test in GetSystemInstructionTests(version))
            yield return test;

        foreach (var test in GetCoProcMoveInstructionTest(version))
            yield return test;

        foreach (var test in GetFloatArithmeticInstructionTests(version))
            yield return test;

        foreach (var test in GetFloatConvertInstructionTests(version))
            yield return test;

        foreach (var test in GetFloatRoundInstructionTests(version))
            yield return test;
    }

    private static IEnumerable<object[]> GetArithmeticInstructionTests(MipsVersion version)
    {
        // Unsigned
        yield return [new ExecutionTestCase("addu $v0, $t2, $t1", 30)];
        yield return [new ExecutionTestCase("addiu $v0, $t2, 10", 30)];
        yield return [new ExecutionTestCase("subu $v0, $t3, $t2", 30 - 20)];
        yield return [new ExecutionTestCase("multu $t3, $t2", (ulong)(30 * 20))];
        yield return [new ExecutionTestCase("divu $t3, $t2", (30 % 20, 30 / 20))];

        // Signed (without signs)
        yield return [new ExecutionTestCase("add $v0, $t2, $t1", 30)];
        yield return [new ExecutionTestCase("addi $v0, $t2, 10", 30)];
        yield return [new ExecutionTestCase("sub $v0, $t3, $t2", 30 - 20)];
        yield return [new ExecutionTestCase("mult $t3, $t2", (ulong)(30 * 20))];
        yield return [new ExecutionTestCase("div $t3, $t2", (30 % 20, 30 / 20))];
        yield return [new ExecutionTestCase("sra $v0, $t8, 4", 101 >> 4)];
        yield return [new ExecutionTestCase("srav $v0, $t8, $s4", 101 >> 4)];

        // Signed (with signs)
        unchecked
        {
            yield return [new ExecutionTestCase("add $v0, $t3, $t5", 30 + (-10))];
            yield return [new ExecutionTestCase("addi $v0, $t3, -10", 30 + (-10))];
            yield return [new ExecutionTestCase("sub $v0, $t2, $t5", 20 - (-10))];
            yield return [new ExecutionTestCase("mult $t3, $t6", (ulong)(30 * -20))];
            yield return [new ExecutionTestCase("div $t3, $t6", (30 % -20, (uint)(30 / -20)))];
        }

        // Overflowing
        unchecked
        {
            // Unsigned (should not overflow)
            yield return [new ExecutionTestCase("addu $v0, $a2, $s1", uint.MaxValue + 1)];
            yield return [new ExecutionTestCase("addiu $v0, $a2, 1", uint.MaxValue + 1)];
            yield return [new ExecutionTestCase("subu $v0, $a3, $s1", uint.MinValue - 1)];
            yield return [new ExecutionTestCase("multu $a2, $a2", (ulong)uint.MaxValue * uint.MaxValue)];
            yield return [new ExecutionTestCase("divu $a2, $a2", (uint.MaxValue % uint.MaxValue, uint.MaxValue / uint.MaxValue))];

            // Note:
            // "mul" does not trap on overflow. We expect the low 32 bits of the result to be written back, and the high 32 bits to be discarded.
            // "mult" also does not trap on overflow, but instead writes the full 64-bit result into the high and low registers.
            // "div" does not trap on overflow either. The behavior is undefined if the quotient is too large to fit in 32 bits.
            // In practice, we will just take the low 32 bits of the quotient and discard the high 32 bits, and write the remainder to the high register.

            // Signed (without signs)
            yield return [new ExecutionTestCase("add $v0, $a0, $s1", MipsTrap.ArithmeticOverflow)];             // max + 1
            yield return [new ExecutionTestCase("addi $v0, $a0, 1", MipsTrap.ArithmeticOverflow)];              // max + 1
            yield return [new ExecutionTestCase("sub $v0, $a1, $s1", MipsTrap.ArithmeticOverflow)];             // min - 1
            yield return [new ExecutionTestCase("mult $a0, $a0", (long)int.MaxValue * int.MaxValue)];           // max * max
            yield return [new ExecutionTestCase("div $a0, $a0", ((uint)((long)int.MaxValue % int.MaxValue), (uint)((long)int.MaxValue / int.MaxValue)))];

            // Signed (with signs)
            yield return [new ExecutionTestCase("add $v0, $a1, $s5", MipsTrap.ArithmeticOverflow)];             // min + (-1)
            yield return [new ExecutionTestCase("addi $v0, $a1, -1", MipsTrap.ArithmeticOverflow)];             // min + (-1)
            yield return [new ExecutionTestCase("sub $v0, $a0, $s5", MipsTrap.ArithmeticOverflow)];             // max - (-1)
            yield return [new ExecutionTestCase("mult $a1, $a1", (long)int.MinValue * int.MinValue)];           // min * min
            yield return [new ExecutionTestCase("div $a1, $a1", ((uint)((long)int.MinValue % int.MinValue), (uint)((long)int.MinValue / int.MinValue)))];
        }

        // Division by zero. Undefined behavior, but NOT a trap! (Shouldn't crash the emulator either)
        yield return [new ExecutionTestCase("divu $t3, $zero", MipsTrap.None)];
        yield return [new ExecutionTestCase("div $t3, $zero", MipsTrap.None)];

        if (version is >= MipsVersion.Mips_R1)
        {
            // GPR Multiply
            yield return [new ExecutionTestCase("mul $v0, $t3, $t2", 30 * 20)];
            yield return [new ExecutionTestCase("mul $v0, $t3, $t6", unchecked((uint)(30 * -20)))];
            yield return [new ExecutionTestCase("mul $v0, $a0, $a0", unchecked(int.MaxValue * int.MaxValue))];             // max * max
            yield return [new ExecutionTestCase("mul $v0, $a1, $a1", unchecked((uint)(int.MinValue * int.MinValue)))];     // min * min
        }

        if (version is >= MipsVersion.Mips_R1 and < MipsVersion.Mips_R6)
        {
            // Multiply and Add/Subtract
            yield return [new ExecutionTestCase("maddu $t3, $t2", (0x1234, 0x5678 + (30 * 20)))];
            yield return [new ExecutionTestCase("madd $t3, $t2", (0x1234, 0x5678 + (30 * 20)))];
            yield return [new ExecutionTestCase("msubu $t3, $t2", (0x1234, 0x5678 - (30 * 20)))];
            yield return [new ExecutionTestCase("msub $t3, $t2", (0x1234, 0x5678 - (30 * 20)))];
        }
    }

    private static IEnumerable<object[]> GetLogicalInstructionTests(MipsVersion version)
    {
        yield return [new ExecutionTestCase("and $v0, $k0, $k1", K0 & K1)];
        yield return [new ExecutionTestCase("andi $v0, $k0, 0xd16", K0 & K1)];
        yield return [new ExecutionTestCase("or $v0, $k0, $k1", K0 | K1)];
        yield return [new ExecutionTestCase("ori $v0, $k0, 0xd16", K0 | K1)];
        yield return [new ExecutionTestCase("xor $v0, $k0, $k1", K0 ^ K1)];
        yield return [new ExecutionTestCase("xori $v0, $k0, 0xd16", K0 ^ K1)];
        yield return [new ExecutionTestCase("nor $v0, $k0, $k1", ~(K0 | K1))];
        yield return [new ExecutionTestCase("sll $v0, $t8, 4", 101 << 4)];
        yield return [new ExecutionTestCase("srl $v0, $t8, 4", 101 >> 4)];
        yield return [new ExecutionTestCase("sllv $v0, $t8, $s4", 101 << 4)];
        yield return [new ExecutionTestCase("srlv $v0, $t8, $s4", 101 >> 4)];
    }

    private static IEnumerable<object[]> GetMemoryInstructionTests(MipsVersion version)
    {
        // Load
        yield return [new ExecutionTestCase("lb $v0, 0x1000($zero)", 0x12)];
        yield return [new ExecutionTestCase("lh $v0, 0x1000($zero)", 0x1234)];
        yield return [new ExecutionTestCase("lw $v0, 0x1000($zero)", 0x1234_5678)];

        // TODO: Load unsigned/signed with sign

        // Store
        yield return [new ExecutionTestCase("sb $at, 0x1000($zero)", (0x1000, [0xef, 0x34, 0x56, 0x78]))];
        yield return [new ExecutionTestCase("sh $at, 0x1000($zero)", (0x1000, [0xcd, 0xef, 0x56, 0x78]))];
        yield return [new ExecutionTestCase("sw $at, 0x1000($zero)", (0x1000, [0x89, 0xab, 0xcd, 0xef]))];
    }

    private static IEnumerable<object[]> GetJumpBranchInstructionTests(MipsVersion version)
    {
        // Jump
        yield return [new ExecutionTestCase("j 1000") { ExpectedPC = 1000 }];
        yield return [new ExecutionTestCase("jal 1000", GPRegister.ReturnAddress, 4) { ExpectedPC = 1000 }];
        yield return [new ExecutionTestCase("jr $t4") { ExpectedPC = 40 }];
        yield return [new ExecutionTestCase("jalr $t4", GPRegister.ReturnAddress, 4) { ExpectedPC = 40 }];

        // Branch Equality
        yield return [new ExecutionTestCase("beq $t2, $t3, 80") { ExpectedPC = 4 }];
        yield return [new ExecutionTestCase("beq $t1, $t1, 80") { ExpectedPC = 84 }];
        yield return [new ExecutionTestCase("bne $t1, $t1, 80") { ExpectedPC = 4 }];
        yield return [new ExecutionTestCase("bne $t3, $t2, 80") { ExpectedPC = 84 }];

        // Branch Compare
        yield return [new ExecutionTestCase("blez $s1, 80") { ExpectedPC = 4 }];
        yield return [new ExecutionTestCase("blez $s0, 80") { ExpectedPC = 84 }];
        yield return [new ExecutionTestCase("blez $s5, 80") { ExpectedPC = 84 }];
        yield return [new ExecutionTestCase("bgtz $s1, 80") { ExpectedPC = 84 }];
        yield return [new ExecutionTestCase("bgtz $s0, 80") { ExpectedPC = 4 }];
        yield return [new ExecutionTestCase("bgtz $s5, 80") { ExpectedPC = 4 }];
        yield return [new ExecutionTestCase("bltz $s1, 80") { ExpectedPC = 4 }];
        yield return [new ExecutionTestCase("bltz $s0, 80") { ExpectedPC = 4 }];
        yield return [new ExecutionTestCase("bltz $s5, 80") { ExpectedPC = 84 }];
        yield return [new ExecutionTestCase("bgez $s1, 80") { ExpectedPC = 84 }];
        yield return [new ExecutionTestCase("bgez $s0, 80") { ExpectedPC = 84 }];
        yield return [new ExecutionTestCase("bgez $s5, 80") { ExpectedPC = 4 }];
    }

    private static IEnumerable<object[]> GetCompareInstructionTests(MipsVersion version)
    {
        // Unsigned
        yield return [new ExecutionTestCase("sltu $v0, $t2, $t3", 1)];
        yield return [new ExecutionTestCase("sltu $v0, $t3, $t2", (uint)0)];
        yield return [new ExecutionTestCase("sltu $v0, $t1, $t1", (uint)0)];
        yield return [new ExecutionTestCase("sltiu $v0, $t2, 30", 1)];
        yield return [new ExecutionTestCase("sltiu $v0, $t3, 20", (uint)0)];
        yield return [new ExecutionTestCase("sltiu $v0, $t1, 10", (uint)0)];

        // Signed (without signs)
        yield return [new ExecutionTestCase("slt $v0, $t2, $t3", 1)];
        yield return [new ExecutionTestCase("slt $v0, $t3, $t2", (uint)0)];
        yield return [new ExecutionTestCase("slt $v0, $t1, $t1", (uint)0)];
        yield return [new ExecutionTestCase("slti $v0, $t2, 30", 1)];
        yield return [new ExecutionTestCase("slti $v0, $t3, 20", (uint)0)];
        yield return [new ExecutionTestCase("slti $v0, $t1, 10", (uint)0)];

        // Signed (with signs)
        yield return [new ExecutionTestCase("slt $v0, $t7, $t6", 1)];
        yield return [new ExecutionTestCase("slt $v0, $t6, $t7", (uint)0)];
        yield return [new ExecutionTestCase("slt $v0, $t5, $t5", (uint)0)];
        yield return [new ExecutionTestCase("slti $v0, $t7, -20", 1)];
        yield return [new ExecutionTestCase("slti $v0, $t6, -30", (uint)0)];
        yield return [new ExecutionTestCase("slti $v0, $t5, -10", (uint)0)];
    }

    private static IEnumerable<object[]> GetTrapInstructionTests(MipsVersion version)
    {
        if (version >= MipsVersion.MipsII)
        {
            // Equality
            yield return [new ExecutionTestCase("teq $t2, $t3", MipsTrap.None)];
            yield return [new ExecutionTestCase("teq $t1, $t1", MipsTrap.Trap)];
            yield return [new ExecutionTestCase("tne $t1, $t1", MipsTrap.None)];
            yield return [new ExecutionTestCase("tne $t3, $t2", MipsTrap.Trap)];

            // Unsigned
            yield return [new ExecutionTestCase("tltu $t3, $t2", MipsTrap.None)];
            yield return [new ExecutionTestCase("tltu $t2, $t3", MipsTrap.Trap)];
            yield return [new ExecutionTestCase("tltu $t1, $t1", MipsTrap.None)];
            yield return [new ExecutionTestCase("tgeu $t2, $t3", MipsTrap.None)];
            yield return [new ExecutionTestCase("tgeu $t3, $t2", MipsTrap.Trap)];
            yield return [new ExecutionTestCase("tgeu $t1, $t1", MipsTrap.Trap)];

            // Signed (without signs)
            yield return [new ExecutionTestCase("tlt $t3, $t2", MipsTrap.None)];
            yield return [new ExecutionTestCase("tlt $t2, $t3", MipsTrap.Trap)];
            yield return [new ExecutionTestCase("tlt $t1, $t1", MipsTrap.None)];
            yield return [new ExecutionTestCase("tge $t2, $t3", MipsTrap.None)];
            yield return [new ExecutionTestCase("tge $t3, $t2", MipsTrap.Trap)];
            yield return [new ExecutionTestCase("tge $t1, $t1", MipsTrap.Trap)];

            // Signed (with signs)
            yield return [new ExecutionTestCase("tlt $t6, $t7", MipsTrap.None)];
            yield return [new ExecutionTestCase("tlt $t7, $t6", MipsTrap.Trap)];
            yield return [new ExecutionTestCase("tlt $t5, $t5", MipsTrap.None)];
            yield return [new ExecutionTestCase("tge $t7, $t6", MipsTrap.None)];
            yield return [new ExecutionTestCase("tge $t6, $t7", MipsTrap.Trap)];
            yield return [new ExecutionTestCase("tge $t5, $t5", MipsTrap.Trap)];
        }
    }

    private static IEnumerable<object[]> GetUncategorizedRegisterOnlyInstructionTests(MipsVersion version)
    {
        // lui
        yield return [new ExecutionTestCase("lui $v0, 0x1234", 0x12340000)];

        if (version is < MipsVersion.Mips_R6)
        {
            // Move from/to high and low registers
            yield return [new ExecutionTestCase("mtlo $k0", (0x1234, K0))];
            yield return [new ExecutionTestCase("mthi $k1", (K1, 0x5678))];
            yield return [new ExecutionTestCase("mflo $v0", 0x5678)];
            yield return [new ExecutionTestCase("mfhi $v0", 0x1234)];
        }

        if (version is >= MipsVersion.MipsIV)
        {
            // movz/movn
            yield return [new ExecutionTestCase("movz $k0, $k1, $t0", GPRegister.Kernel0, K1)];
            yield return [new ExecutionTestCase("movz $k0, $k1, $t1", GPRegister.Zero)];
            yield return [new ExecutionTestCase("movn $k0, $k1, $t0", GPRegister.Zero)];
            yield return [new ExecutionTestCase("movn $k0, $k1, $t1", GPRegister.Kernel0, K1)];
        }

        if (version is >= MipsVersion.Mips_R1)
        {
            // Niche bit-manipulation
            // TODO: ext, ins, seb, seh, wsbh, wshd
            yield return [new ExecutionTestCase("clz $v0, $k0", (uint)BitOperations.LeadingZeroCount(K0))];
            yield return [new ExecutionTestCase("clo $v0, $k0", (uint)BitOperations.LeadingZeroCount(~K0))];
        }
    }

    private static IEnumerable<object[]> GetSystemInstructionTests(MipsVersion version)
    {
        yield return [new ExecutionTestCase("syscall", MipsTrap.Syscall)];
        yield return [new ExecutionTestCase("break", MipsTrap.Breakpoint)];

        // Exception Return
        yield return [new ExecutionTestCase("eret", MipsTrap.ReservedInstruction)];
        yield return [new ExecutionTestCase("eret", SideEffect.WriteCoProc0)
            {
                Status = new StatusRegister
                {
                    ExceptionLevel = true
                }
            }];

        // Enable Interrupts
        yield return [new ExecutionTestCase("ei", MipsTrap.ReservedInstruction)];
        yield return [new ExecutionTestCase("ei", SideEffect.WriteCoProc0)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
        yield return [new ExecutionTestCase("ei $v0", GPRegister.ReturnValue0)
            {
                ExpectedSideEffect = SideEffect.WriteCoProc0,
                PrivilegeMode = PrivilegeMode.Kernel
            }];

        // Disable Interrupts
        yield return [new ExecutionTestCase("di", MipsTrap.ReservedInstruction)];
        yield return [new ExecutionTestCase("di", SideEffect.WriteCoProc0)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
        yield return [new ExecutionTestCase("di $v1", GPRegister.ReturnValue1)
            {
                ExpectedSideEffect = SideEffect.WriteCoProc0,
                PrivilegeMode = PrivilegeMode.Kernel
            }];
    }

    private static IEnumerable<object[]> GetCoProcMoveInstructionTest(MipsVersion version)
    {
        // CoProcessor 1
        yield return [new ExecutionTestCase("mtc1 $t2, $f16", FloatRegister.F16, 20)];
        yield return [new ExecutionTestCase("mfc1 $v0, $f0", GPRegister.ReturnValue0, 2)];
    }

    private static IEnumerable<object[]> GetFloatArithmeticInstructionTests(MipsVersion version)
    {
        // Single
        yield return [new ExecutionTestCase("add.S $f16, $f8, $f9", FloatRegister.F16, 10.5f + 2.5f)];
        yield return [new ExecutionTestCase("sub.S $f16, $f8, $f9", FloatRegister.F16, 10.5f - 2.5f)];
        yield return [new ExecutionTestCase("mul.S $f16, $f8, $f9", FloatRegister.F16, 10.5f * 2.5f)];
        yield return [new ExecutionTestCase("div.S $f16, $f8, $f9", FloatRegister.F16, 10.5f / 2.5f)];
        yield return [new ExecutionTestCase("abs.S $f16, $f7", FloatRegister.F16, 2f)];
        yield return [new ExecutionTestCase("neg.S $f16, $f5", FloatRegister.F16, -2f)];
        yield return [new ExecutionTestCase("sqrt.S $f16, $f8", FloatRegister.F16, MathF.Sqrt(10.5f))];
        yield return [new ExecutionTestCase("recip.S $f16, $f9", FloatRegister.F16, float.ReciprocalEstimate(2.5f))];

        // Double
        yield return [new ExecutionTestCase("add.D $f16, $f12, $f14", FloatRegister.F16, 2d + 0.5d)];
        yield return [new ExecutionTestCase("sub.D $f16, $f12, $f14", FloatRegister.F16, 2d - 0.5d)];
        yield return [new ExecutionTestCase("mul.D $f16, $f12, $f14", FloatRegister.F16, 2d * 0.5d)];
        yield return [new ExecutionTestCase("div.D $f16, $f12, $f14", FloatRegister.F16, 2d / 0.5d)];
        yield return [new ExecutionTestCase("abs.D $f16, $f16", FloatRegister.F16, 2d)];
        yield return [new ExecutionTestCase("neg.D $f16, $f12", FloatRegister.F16, -2d)];
        yield return [new ExecutionTestCase("sqrt.D $f16, $f12", FloatRegister.F16, Math.Sqrt(2d))];
        yield return [new ExecutionTestCase("recip.D $f16, $f12", FloatRegister.F16, double.ReciprocalEstimate(2d))];
    }

    private static IEnumerable<object[]> GetFloatConvertInstructionTests(MipsVersion version)
    {
        // From Single 
        yield return [new ExecutionTestCase("cvt.D.S $f16, $f5", FloatRegister.F16, 2d)];     // To Double
        yield return [new ExecutionTestCase("cvt.W.S $f16, $f5", FloatRegister.F16, 2)];      // To Word
        yield return [new ExecutionTestCase("cvt.L.S $f16, $f5", FloatRegister.F16, 2L)];     // To Long

        // From Double
        yield return [new ExecutionTestCase("cvt.S.D $f16, $f12", FloatRegister.F16, 2f)];    // To Single
        yield return [new ExecutionTestCase("cvt.W.D $f16, $f12", FloatRegister.F16, 2)];     // To Word
        yield return [new ExecutionTestCase("cvt.L.D $f16, $f12", FloatRegister.F16, 2L)];    // To Long

        // From Word 
        yield return [new ExecutionTestCase("cvt.S.W $f16, $f0", FloatRegister.F16, 2f)];     // To Single
        yield return [new ExecutionTestCase("cvt.D.W $f16, $f0", FloatRegister.F16, 2d)];     // To Double

        // From Long
        yield return [new ExecutionTestCase("cvt.S.L $f16, $f0", FloatRegister.F16, 2f)];     // To Single
        yield return [new ExecutionTestCase("cvt.D.L $f16, $f0", FloatRegister.F16, 2d)];     // To Double
    }

    private static IEnumerable<object[]> GetFloatRoundInstructionTests(MipsVersion version)
    {
        // Round
        yield return [new ExecutionTestCase("round.W.S $f16, $f10", FloatRegister.F16, 1)];
        yield return [new ExecutionTestCase("round.W.D $f16, $f18", FloatRegister.F16, 3)];
        yield return [new ExecutionTestCase("round.L.S $f16, $f10", FloatRegister.F16, 1L)];
        yield return [new ExecutionTestCase("round.L.D $f16, $f18", FloatRegister.F16, 3L)];

        // Ceiling
        yield return [new ExecutionTestCase("ceil.W.S $f16, $f10", FloatRegister.F16, 2)];
        yield return [new ExecutionTestCase("ceil.W.D $f16, $f18", FloatRegister.F16, 4)];
        yield return [new ExecutionTestCase("ceil.L.S $f16, $f10", FloatRegister.F16, 2L)];
        yield return [new ExecutionTestCase("ceil.L.D $f16, $f18", FloatRegister.F16, 4L)];

        // Floor
        yield return [new ExecutionTestCase("floor.W.S $f16, $f10", FloatRegister.F16, 1)];
        yield return [new ExecutionTestCase("floor.W.D $f16, $f18", FloatRegister.F16, 3)];
        yield return [new ExecutionTestCase("floor.L.S $f16, $f10", FloatRegister.F16, 1L)];
        yield return [new ExecutionTestCase("floor.L.D $f16, $f18", FloatRegister.F16, 3L)];
    }
}
