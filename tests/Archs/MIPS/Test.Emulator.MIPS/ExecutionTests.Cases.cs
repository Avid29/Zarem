// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Numerics;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models.Enum;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Extensions;
using Microsoft.Testing.Extensions;

namespace Test.Emulator.MIPS;

public partial class ExecutionTests
{
    public static IEnumerable<object[]> InstructionTestList_Mips1
        => GetVersionTests<uint, int, ulong>(MipsVersion.MipsI);
    
    public static IEnumerable<object[]> InstructionTestList_Mips2
        => GetVersionTests<uint, int, ulong>(MipsVersion.MipsII);
    
    public static IEnumerable<object[]> InstructionTestList_Mips3
        => GetVersionTests<ulong, long, UInt128>(MipsVersion.MipsIII);
    
    public static IEnumerable<object[]> InstructionTestList_Mips3_32Bit
        => GetVersionTests<uint, int, ulong>(MipsVersion.MipsIII_32Bit);
    
    public static IEnumerable<object[]> InstructionTestList_Mips4
        => GetVersionTests<ulong, long, UInt128>(MipsVersion.MipsIV);
    
    public static IEnumerable<object[]> InstructionTestList_Mips4_32Bit
        => GetVersionTests<uint, int, ulong>(MipsVersion.MipsIV_32Bit);
    
    public static IEnumerable<object[]> InstructionTestList_Mips5
        => GetVersionTests<ulong, long, UInt128>(MipsVersion.MipsV);
    
    public static IEnumerable<object[]> InstructionTestList_Mips5_32Bit
        => GetVersionTests<uint, int, ulong>(MipsVersion.MipsV_32Bit);
    
    public static IEnumerable<object[]> InstructionTestList_Mips32R1
        => GetVersionTests<uint, int, ulong>(MipsVersion.Mips32R1);
    
    public static IEnumerable<object[]> InstructionTestList_Mips64R1
        => GetVersionTests<ulong, long, UInt128>(MipsVersion.Mips64R1);
    
    public static IEnumerable<object[]> InstructionTestList_Mips32R2
        => GetVersionTests<uint, int, ulong>(MipsVersion.Mips32R2);

    public static IEnumerable<object[]> InstructionTestList_Mips64R2
        => GetVersionTests<ulong, long, UInt128>(MipsVersion.Mips64R2);

    private static IEnumerable<object[]> GetVersionTests<T, TSigned, TLong>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
        where TLong : unmanaged, IBinaryInteger<TLong>, IUnsignedNumber<TLong>, IMinMaxValue<TLong>
    {
        foreach (var test in GetArithmeticInstructionTests<T, TSigned, TLong>(version))
            yield return test;

        foreach (var test in GetLogicalInstructionTests<T, TSigned>(version))
            yield return test;

        foreach (var test in GetMemoryInstructionTests<T, TSigned>(version))
            yield return test;

        foreach (var test in GetJumpBranchInstructionTests<T, TSigned>(version))
            yield return test;

        foreach (var test in GetCompareInstructionTests<T, TSigned>(version))
            yield return test;

        foreach (var test in GetTrapInstructionTests<T, TSigned>(version))
            yield return test;

        foreach (var test in GetUncategorizedInstructionTests<T, TSigned>(version))
            yield return test;

        foreach (var test in GetSystemInstructionTests<T, TSigned>(version))
            yield return test;

        foreach (var test in GetCoProcMoveInstructionTest<T, TSigned>(version))
            yield return test;

        foreach (var test in GetFloatArithmeticInstructionTests<T, TSigned>(version))
            yield return test;

        foreach (var test in GetFloatConvertInstructionTests<T, TSigned>(version))
            yield return test;

        foreach (var test in GetFloatRoundInstructionTests<T, TSigned>(version))
            yield return test;
    }

    private static IEnumerable<object[]> GetArithmeticInstructionTests<T, TSigned, TLong>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
        where TLong : unmanaged, IBinaryInteger<TLong>, IUnsignedNumber<TLong>, IMinMaxValue<TLong>
    {
        // Unsigned
        yield return [new ExecutionTestCase<T, TSigned>("addu $v0, $t2, $t1", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T, TSigned>("addiu $v0, $t2, 10", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T, TSigned>("subu $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
        yield return [new ExecutionTestCase<T, TSigned>("multu $t3, $t2", Split<T, TLong>(TLong.CreateTruncating(30 * 20)))];
        yield return [new ExecutionTestCase<T, TSigned>("divu $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];

        // Signed (without signs)
        yield return [new ExecutionTestCase<T, TSigned>("add $v0, $t2, $t1", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T, TSigned>("addi $v0, $t2, 10", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T, TSigned>("sub $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
        yield return [new ExecutionTestCase<T, TSigned>("mult $t3, $t2", Split<T, TLong>(TLong.CreateTruncating(30 * 20)))];
        yield return [new ExecutionTestCase<T, TSigned>("div $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];
        yield return [new ExecutionTestCase<T, TSigned>("sra $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
        yield return [new ExecutionTestCase<T, TSigned>("srav $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];

        // Signed (with signs)
        unchecked
        {
            yield return [new ExecutionTestCase<T, TSigned>("add $v0, $t3, $t5", T.CreateTruncating(30 + (-10)))];
            yield return [new ExecutionTestCase<T, TSigned>("addi $v0, $t3, -10", T.CreateTruncating(30 + (-10)))];
            yield return [new ExecutionTestCase<T, TSigned>("sub $v0, $t2, $t5", T.CreateTruncating(20 - (-10)))];
            yield return [new ExecutionTestCase<T, TSigned>("mult $t3, $t6", Split<T, TLong>(TLong.CreateTruncating(30 * -20)))];
            yield return [new ExecutionTestCase<T, TSigned>("div $t3, $t6", (T.CreateTruncating(30 % -20), T.CreateTruncating(30 / -20)))];
        }

        // Overflowing
        unchecked
        {
            // Unsigned (should overflow without trapping)
            yield return [new ExecutionTestCase<T, TSigned>("addu $v0, $a2, $s1", T.Zero)];
            yield return [new ExecutionTestCase<T, TSigned>("addiu $v0, $a2, 1", T.Zero)];
            yield return [new ExecutionTestCase<T, TSigned>("subu $v0, $a3, $s1", T.MinValue - T.One)];
            yield return [new ExecutionTestCase<T, TSigned>("multu $a2, $a2", Split<T, TLong>(TLong.CreateTruncating(T.MaxValue) * TLong.CreateTruncating(T.MaxValue)))];
            yield return [new ExecutionTestCase<T, TSigned>("divu $a2, $a2", (T.MaxValue % T.MaxValue, T.MaxValue / T.MaxValue))];

            // Note:
            // "mul" does not trap on overflow. We expect the low 32 bits of the result to be written back, and the high 32 bits to be discarded.
            // "mult" also does not trap on overflow, but instead writes the full 64-bit result into the high and low registers.
            // "div" does not trap on overflow either. The behavior is undefined if the quotient is too large to fit in 32 bits.
            // In practice, we will just take the low 32 bits of the quotient and discard the high 32 bits, and write the remainder to the high register.

            // Signed (without signs)
            yield return [new ExecutionTestCase<T, TSigned>("add $v0, $a0, $s1", MipsTrap.ArithmeticOverflow)];     // max + 1
            yield return [new ExecutionTestCase<T, TSigned>("addi $v0, $a0, 1", MipsTrap.ArithmeticOverflow)];      // max + 1
            yield return [new ExecutionTestCase<T, TSigned>("sub $v0, $a1, $s1", MipsTrap.ArithmeticOverflow)];     // min - 1
            yield return [new ExecutionTestCase<T, TSigned>("mult $a0, $a0", Split<T, TLong>(TLong.CreateTruncating(TLong.CreateTruncating(TSigned.MaxValue) * TLong.CreateTruncating(TSigned.MaxValue))))];    // max * max
            yield return [new ExecutionTestCase<T, TSigned>("div $a0, $a0", (T.CreateTruncating(TSigned.MaxValue % TSigned.MaxValue), T.CreateTruncating(TSigned.MaxValue / TSigned.MaxValue)))];

            // Signed (with signs)
            yield return [new ExecutionTestCase<T, TSigned>("add $v0, $a1, $s5", MipsTrap.ArithmeticOverflow)];     // min + (-1)
            yield return [new ExecutionTestCase<T, TSigned>("addi $v0, $a1, -1", MipsTrap.ArithmeticOverflow)];     // min + (-1)
            yield return [new ExecutionTestCase<T, TSigned>("sub $v0, $a0, $s5", MipsTrap.ArithmeticOverflow)];     // max - (-1)
            yield return [new ExecutionTestCase<T, TSigned>("mult $a1, $a1", Split<T, TLong>(TLong.CreateTruncating(TSigned.MinValue) * TLong.CreateTruncating(TSigned.MinValue)))];    // min * min
            yield return [new ExecutionTestCase<T, TSigned>("div $a1, $a1", (T.CreateTruncating(TSigned.MinValue % TSigned.MinValue), T.CreateTruncating(TSigned.MinValue / TSigned.MinValue)))];
        }

        // Division by zero. Undefined behavior, but NOT a trap! (Shouldn't crash the emulator either)
        yield return [new ExecutionTestCase<T, TSigned>("divu $t3, $zero", MipsTrap.None)];
        yield return [new ExecutionTestCase<T, TSigned>("div $t3, $zero", MipsTrap.None)];

        if (version is >= MipsVersion.Mips_R1)
        {
            // GPR Multiply
            yield return [new ExecutionTestCase<T, TSigned>("mul $v0, $t3, $t2", T.CreateTruncating(30 * 20))];
            yield return [new ExecutionTestCase<T, TSigned>("mul $v0, $t3, $t6", T.CreateTruncating(30 * -20))];
            yield return [new ExecutionTestCase<T, TSigned>("mul $v0, $a0, $a0", T.CreateTruncating(unchecked(TSigned.MaxValue * TSigned.MaxValue)))];     // max * max
            yield return [new ExecutionTestCase<T, TSigned>("mul $v0, $a1, $a1", T.CreateTruncating(unchecked(TSigned.MinValue * TSigned.MinValue)))];     // min * min
        }

        if (version is >= MipsVersion.Mips_R1 and < MipsVersion.Mips_R6)
        {
            // Multiply and Add/Subtract
            yield return [new ExecutionTestCase<T, TSigned>("maddu $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 + (30 * 20))))];
            yield return [new ExecutionTestCase<T, TSigned>("madd $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 + (30 * 20))))];
            yield return [new ExecutionTestCase<T, TSigned>("msubu $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 - (30 * 20))))];
            yield return [new ExecutionTestCase<T, TSigned>("msub $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 - (30 * 20))))];
        }

        // Not arithmetic, but fixed width
        if (version is >= MipsVersion.Mips_R1)
        {
            // Niche bit-manipulation
            // TODO: ext, ins, seb, seh, wsbh, wshd
            yield return [new ExecutionTestCase<T, TSigned>("clz $v0, $k0", T.CreateTruncating(BitOperations.LeadingZeroCount(K0)))];
            yield return [new ExecutionTestCase<T, TSigned>("clo $v0, $k0", T.CreateTruncating(BitOperations.LeadingZeroCount(~K0)))];
        }
    }

    private static IEnumerable<object[]> GetLogicalInstructionTests<T, TSigned>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
    {
        yield return [new ExecutionTestCase<T, TSigned>("and $v0, $k0, $k1", T.CreateTruncating(K0 & K1))];
        yield return [new ExecutionTestCase<T, TSigned>("andi $v0, $k0, 0xd16", T.CreateTruncating(K0 & K1))];
        yield return [new ExecutionTestCase<T, TSigned>("or $v0, $k0, $k1", T.CreateTruncating(K0 | K1))];
        yield return [new ExecutionTestCase<T, TSigned>("ori $v0, $k0, 0xd16", T.CreateTruncating(K0 | K1))];
        yield return [new ExecutionTestCase<T, TSigned>("xor $v0, $k0, $k1", T.CreateTruncating(K0 ^ K1))];
        yield return [new ExecutionTestCase<T, TSigned>("xori $v0, $k0, 0xd16", T.CreateTruncating(K0 ^ K1))];
        yield return [new ExecutionTestCase<T, TSigned>("nor $v0, $k0, $k1", T.CreateTruncating(~(K0 | K1)))];
        yield return [new ExecutionTestCase<T, TSigned>("sll $v0, $t8, 4", T.CreateTruncating(101 << 4))];
        yield return [new ExecutionTestCase<T, TSigned>("srl $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
        yield return [new ExecutionTestCase<T, TSigned>("sllv $v0, $t8, $s4", T.CreateTruncating(101 << 4))];
        yield return [new ExecutionTestCase<T, TSigned>("srlv $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];
    }

    private static IEnumerable<object[]> GetMemoryInstructionTests<T, TSigned>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
    {
        // Load
        yield return [new ExecutionTestCase<T, TSigned>("lb $v0, 0x1000($zero)", T.CreateTruncating(0x12))];
        yield return [new ExecutionTestCase<T, TSigned>("lh $v0, 0x1000($zero)", T.CreateTruncating(0x1234))];
        yield return [new ExecutionTestCase<T, TSigned>("lw $v0, 0x1000($zero)", T.CreateTruncating(0x1234_5678))];

        // TODO: Load unsigned/signed with sign

        // Store
        yield return [new ExecutionTestCase<T, TSigned>("sb $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0xef, 0x34, 0x56, 0x78]))];
        yield return [new ExecutionTestCase<T, TSigned>("sh $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0xcd, 0xef, 0x56, 0x78]))];
        yield return [new ExecutionTestCase<T, TSigned>("sw $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0x89, 0xab, 0xcd, 0xef]))];
    }

    private static IEnumerable<object[]> GetJumpBranchInstructionTests<T, TSigned>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
    {
        // Jump
        yield return [new ExecutionTestCase<T, TSigned>("j 1000") { ExpectedPC = T.CreateTruncating(1000) }];
        yield return [new ExecutionTestCase<T, TSigned>("jal 1000", GPRegister.ReturnAddress, T.CreateTruncating(4)) { ExpectedPC = T.CreateTruncating(1000) }];
        yield return [new ExecutionTestCase<T, TSigned>("jr $t4") { ExpectedPC = T.CreateTruncating(40) }];
        yield return [new ExecutionTestCase<T, TSigned>("jalr $t4", GPRegister.ReturnAddress, T.CreateTruncating(4)) { ExpectedPC = T.CreateTruncating(40) }];

        // Branch Equality
        yield return [new ExecutionTestCase<T, TSigned>("beq $t2, $t3, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T, TSigned>("beq $t1, $t1, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T, TSigned>("bne $t1, $t1, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T, TSigned>("bne $t3, $t2, 80") { ExpectedPC = T.CreateTruncating(84) }];

        // Branch Compare
        yield return [new ExecutionTestCase<T, TSigned>("blez $s1, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T, TSigned>("blez $s0, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T, TSigned>("blez $s5, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T, TSigned>("bgtz $s1, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T, TSigned>("bgtz $s0, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T, TSigned>("bgtz $s5, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T, TSigned>("bltz $s1, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T, TSigned>("bltz $s0, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T, TSigned>("bltz $s5, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T, TSigned>("bgez $s1, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T, TSigned>("bgez $s0, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T, TSigned>("bgez $s5, 80") { ExpectedPC = T.CreateTruncating(4) }];
    }

    private static IEnumerable<object[]> GetCompareInstructionTests<T, TSigned>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
    {
        // Unsigned
        yield return [new ExecutionTestCase<T, TSigned>("sltu $v0, $t2, $t3", T.One)];
        yield return [new ExecutionTestCase<T, TSigned>("sltu $v0, $t3, $t2", T.Zero)];
        yield return [new ExecutionTestCase<T, TSigned>("sltu $v0, $t1, $t1", T.Zero)];
        yield return [new ExecutionTestCase<T, TSigned>("sltiu $v0, $t2, 30", T.One)];
        yield return [new ExecutionTestCase<T, TSigned>("sltiu $v0, $t3, 20", T.Zero)];
        yield return [new ExecutionTestCase<T, TSigned>("sltiu $v0, $t1, 10", T.Zero)];

        // Signed (without signs)
        yield return [new ExecutionTestCase<T, TSigned>("slt $v0, $t2, $t3", T.One)];
        yield return [new ExecutionTestCase<T, TSigned>("slt $v0, $t3, $t2", T.Zero)];
        yield return [new ExecutionTestCase<T, TSigned>("slt $v0, $t1, $t1", T.Zero)];
        yield return [new ExecutionTestCase<T, TSigned>("slti $v0, $t2, 30", T.One)];
        yield return [new ExecutionTestCase<T, TSigned>("slti $v0, $t3, 20", T.Zero)];
        yield return [new ExecutionTestCase<T, TSigned>("slti $v0, $t1, 10", T.Zero)];

        // Signed (with signs)
        yield return [new ExecutionTestCase<T, TSigned>("slt $v0, $t7, $t6", T.One)];
        yield return [new ExecutionTestCase<T, TSigned>("slt $v0, $t6, $t7", T.Zero)];
        yield return [new ExecutionTestCase<T, TSigned>("slt $v0, $t5, $t5", T.Zero)];
        yield return [new ExecutionTestCase<T, TSigned>("slti $v0, $t7, -20", T.One)];
        yield return [new ExecutionTestCase<T, TSigned>("slti $v0, $t6, -30", T.Zero)];
        yield return [new ExecutionTestCase<T, TSigned>("slti $v0, $t5, -10", T.Zero)];
    }

    private static IEnumerable<object[]> GetTrapInstructionTests<T, TSigned>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
    {
        if (version >= MipsVersion.MipsII)
        {
            // Equality
            yield return [new ExecutionTestCase<T, TSigned>("teq $t2, $t3", MipsTrap.None)];
            yield return [new ExecutionTestCase<T, TSigned>("teq $t1, $t1", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T, TSigned>("tne $t1, $t1", MipsTrap.None)];
            yield return [new ExecutionTestCase<T, TSigned>("tne $t3, $t2", MipsTrap.Trap)];

            // Unsigned
            yield return [new ExecutionTestCase<T, TSigned>("tltu $t3, $t2", MipsTrap.None)];
            yield return [new ExecutionTestCase<T, TSigned>("tltu $t2, $t3", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T, TSigned>("tltu $t1, $t1", MipsTrap.None)];
            yield return [new ExecutionTestCase<T, TSigned>("tgeu $t2, $t3", MipsTrap.None)];
            yield return [new ExecutionTestCase<T, TSigned>("tgeu $t3, $t2", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T, TSigned>("tgeu $t1, $t1", MipsTrap.Trap)];

            // Signed (without signs)
            yield return [new ExecutionTestCase<T, TSigned>("tlt $t3, $t2", MipsTrap.None)];
            yield return [new ExecutionTestCase<T, TSigned>("tlt $t2, $t3", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T, TSigned>("tlt $t1, $t1", MipsTrap.None)];
            yield return [new ExecutionTestCase<T, TSigned>("tge $t2, $t3", MipsTrap.None)];
            yield return [new ExecutionTestCase<T, TSigned>("tge $t3, $t2", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T, TSigned>("tge $t1, $t1", MipsTrap.Trap)];

            // Signed (with signs)
            yield return [new ExecutionTestCase<T, TSigned>("tlt $t6, $t7", MipsTrap.None)];
            yield return [new ExecutionTestCase<T, TSigned>("tlt $t7, $t6", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T, TSigned>("tlt $t5, $t5", MipsTrap.None)];
            yield return [new ExecutionTestCase<T, TSigned>("tge $t7, $t6", MipsTrap.None)];
            yield return [new ExecutionTestCase<T, TSigned>("tge $t6, $t7", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T, TSigned>("tge $t5, $t5", MipsTrap.Trap)];
        }
    }

    private static IEnumerable<object[]> GetUncategorizedInstructionTests<T, TSigned>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
    {
        // lui
        yield return [new ExecutionTestCase<T, TSigned>("lui $v0, 0x1234", T.CreateTruncating(0x12340000))];

        if (version is < MipsVersion.Mips_R6)
        {
            // Move from/to high and low registers
            yield return [new ExecutionTestCase<T, TSigned>("mtlo $k0", (T.CreateTruncating(0x1234), T.CreateTruncating(K0)))];
            yield return [new ExecutionTestCase<T, TSigned>("mthi $k1", (T.CreateTruncating(K1), T.CreateTruncating(0x5678)))];
            yield return [new ExecutionTestCase<T, TSigned>("mflo $v0", T.CreateTruncating(0x5678))];
            yield return [new ExecutionTestCase<T, TSigned>("mfhi $v0", T.CreateTruncating(0x1234))];
        }

        if (version is >= MipsVersion.MipsIV)
        {
            // movz/movn
            yield return [new ExecutionTestCase<T, TSigned>("movz $k0, $k1, $t0", GPRegister.Kernel0, T.CreateTruncating(K1))];
            yield return [new ExecutionTestCase<T, TSigned>("movz $k0, $k1, $t1", GPRegister.Zero)];
            yield return [new ExecutionTestCase<T, TSigned>("movn $k0, $k1, $t0", GPRegister.Zero)];
            yield return [new ExecutionTestCase<T, TSigned>("movn $k0, $k1, $t1", GPRegister.Kernel0, T.CreateTruncating(K1))];
        }
    }

    private static IEnumerable<object[]> GetSystemInstructionTests<T, TSigned>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
    {
        yield return [new ExecutionTestCase<T, TSigned>("syscall", MipsTrap.Syscall)];
        yield return [new ExecutionTestCase<T, TSigned>("break", MipsTrap.Breakpoint)];

        // Exception Return
        yield return [new ExecutionTestCase<T, TSigned>("eret", MipsTrap.ReservedInstruction)];
        yield return [new ExecutionTestCase<T, TSigned>("eret", SideEffect.WriteCoProc0)
            {
                Status = new StatusRegister
                {
                    ExceptionLevel = true
                }
            }];

        // Enable Interrupts
        yield return [new ExecutionTestCase<T, TSigned>("ei", MipsTrap.ReservedInstruction)];
        yield return [new ExecutionTestCase<T, TSigned>("ei", SideEffect.WriteCoProc0)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
        yield return [new ExecutionTestCase<T, TSigned>("ei $v0", GPRegister.ReturnValue0)
            {
                ExpectedSideEffect = SideEffect.WriteCoProc0,
                PrivilegeMode = PrivilegeMode.Kernel
            }];

        // Disable Interrupts
        yield return [new ExecutionTestCase<T, TSigned>("di", MipsTrap.ReservedInstruction)];
        yield return [new ExecutionTestCase<T, TSigned>("di", SideEffect.WriteCoProc0)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
        yield return [new ExecutionTestCase<T, TSigned>("di $v1", GPRegister.ReturnValue1)
            {
                ExpectedSideEffect = SideEffect.WriteCoProc0,
                PrivilegeMode = PrivilegeMode.Kernel
            }];
    }

    private static IEnumerable<object[]> GetCoProcMoveInstructionTest<T, TSigned>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
    {
        // CoProcessor 1
        yield return [new ExecutionTestCase<T, TSigned>("mtc1 $t2, $f16", FloatRegister.F16, 20)];
        yield return [new ExecutionTestCase<T, TSigned>("mfc1 $v0, $f0", GPRegister.ReturnValue0, T.CreateTruncating(2))];
    }

    private static IEnumerable<object[]> GetFloatArithmeticInstructionTests<T, TSigned>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
    {
        // Single
        yield return [new ExecutionTestCase<T, TSigned>("add.S $f16, $f8, $f9", FloatRegister.F16, 10.5f + 2.5f)];
        yield return [new ExecutionTestCase<T, TSigned>("sub.S $f16, $f8, $f9", FloatRegister.F16, 10.5f - 2.5f)];
        yield return [new ExecutionTestCase<T, TSigned>("mul.S $f16, $f8, $f9", FloatRegister.F16, 10.5f * 2.5f)];
        yield return [new ExecutionTestCase<T, TSigned>("div.S $f16, $f8, $f9", FloatRegister.F16, 10.5f / 2.5f)];
        yield return [new ExecutionTestCase<T, TSigned>("abs.S $f16, $f7", FloatRegister.F16, 2f)];
        yield return [new ExecutionTestCase<T, TSigned>("neg.S $f16, $f5", FloatRegister.F16, -2f)];
        yield return [new ExecutionTestCase<T, TSigned>("sqrt.S $f16, $f8", FloatRegister.F16, MathF.Sqrt(10.5f))];
        yield return [new ExecutionTestCase<T, TSigned>("recip.S $f16, $f9", FloatRegister.F16, float.ReciprocalEstimate(2.5f))];

        // Double
        yield return [new ExecutionTestCase<T, TSigned>("add.D $f16, $f12, $f14", FloatRegister.F16, 2d + 0.5d)];
        yield return [new ExecutionTestCase<T, TSigned>("sub.D $f16, $f12, $f14", FloatRegister.F16, 2d - 0.5d)];
        yield return [new ExecutionTestCase<T, TSigned>("mul.D $f16, $f12, $f14", FloatRegister.F16, 2d * 0.5d)];
        yield return [new ExecutionTestCase<T, TSigned>("div.D $f16, $f12, $f14", FloatRegister.F16, 2d / 0.5d)];
        yield return [new ExecutionTestCase<T, TSigned>("abs.D $f16, $f16", FloatRegister.F16, 2d)];
        yield return [new ExecutionTestCase<T, TSigned>("neg.D $f16, $f12", FloatRegister.F16, -2d)];
        yield return [new ExecutionTestCase<T, TSigned>("sqrt.D $f16, $f12", FloatRegister.F16, Math.Sqrt(2d))];
        yield return [new ExecutionTestCase<T, TSigned>("recip.D $f16, $f12", FloatRegister.F16, double.ReciprocalEstimate(2d))];
    }

    private static IEnumerable<object[]> GetFloatConvertInstructionTests<T, TSigned>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
    {
        // From Single 
        yield return [new ExecutionTestCase<T, TSigned>("cvt.D.S $f16, $f5", FloatRegister.F16, 2d)];     // To Double
        yield return [new ExecutionTestCase<T, TSigned>("cvt.W.S $f16, $f5", FloatRegister.F16, 2)];      // To Word
        yield return [new ExecutionTestCase<T, TSigned>("cvt.L.S $f16, $f5", FloatRegister.F16, 2L)];     // To Long

        // From Double
        yield return [new ExecutionTestCase<T, TSigned>("cvt.S.D $f16, $f12", FloatRegister.F16, 2f)];    // To Single
        yield return [new ExecutionTestCase<T, TSigned>("cvt.W.D $f16, $f12", FloatRegister.F16, 2)];     // To Word
        yield return [new ExecutionTestCase<T, TSigned>("cvt.L.D $f16, $f12", FloatRegister.F16, 2L)];    // To Long

        // From Word 
        yield return [new ExecutionTestCase<T, TSigned>("cvt.S.W $f16, $f0", FloatRegister.F16, 2f)];     // To Single
        yield return [new ExecutionTestCase<T, TSigned>("cvt.D.W $f16, $f0", FloatRegister.F16, 2d)];     // To Double

        // From Long
        yield return [new ExecutionTestCase<T, TSigned>("cvt.S.L $f16, $f0", FloatRegister.F16, 2f)];     // To Single
        yield return [new ExecutionTestCase<T, TSigned>("cvt.D.L $f16, $f0", FloatRegister.F16, 2d)];     // To Double
    }

    private static IEnumerable<object[]> GetFloatRoundInstructionTests<T, TSigned>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
    {
        // Round
        yield return [new ExecutionTestCase<T, TSigned>("round.W.S $f16, $f10", FloatRegister.F16, 1)];
        yield return [new ExecutionTestCase<T, TSigned>("round.W.D $f16, $f18", FloatRegister.F16, 3)];
        yield return [new ExecutionTestCase<T, TSigned>("round.L.S $f16, $f10", FloatRegister.F16, 1L)];
        yield return [new ExecutionTestCase<T, TSigned>("round.L.D $f16, $f18", FloatRegister.F16, 3L)];

        // Ceiling
        yield return [new ExecutionTestCase<T, TSigned>("ceil.W.S $f16, $f10", FloatRegister.F16, 2)];
        yield return [new ExecutionTestCase<T, TSigned>("ceil.W.D $f16, $f18", FloatRegister.F16, 4)];
        yield return [new ExecutionTestCase<T, TSigned>("ceil.L.S $f16, $f10", FloatRegister.F16, 2L)];
        yield return [new ExecutionTestCase<T, TSigned>("ceil.L.D $f16, $f18", FloatRegister.F16, 4L)];

        // Floor
        yield return [new ExecutionTestCase<T, TSigned>("floor.W.S $f16, $f10", FloatRegister.F16, 1)];
        yield return [new ExecutionTestCase<T, TSigned>("floor.W.D $f16, $f18", FloatRegister.F16, 3)];
        yield return [new ExecutionTestCase<T, TSigned>("floor.L.S $f16, $f10", FloatRegister.F16, 1L)];
        yield return [new ExecutionTestCase<T, TSigned>("floor.L.D $f16, $f18", FloatRegister.F16, 3L)];
    }

    private unsafe static (T, T) Split<T, TLong>(TLong value)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TLong : unmanaged, IBinaryInteger<TLong>, IUnsignedNumber<TLong>
        => (T.CreateTruncating(value >> sizeof(T) * 8), T.CreateTruncating(value));
}
