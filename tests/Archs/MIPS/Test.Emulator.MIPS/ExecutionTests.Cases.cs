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

        foreach (var test in GetLogicalInstructionTests<T>(version))
            yield return test;

        foreach (var test in GetMemoryInstructionTests<T>(version))
            yield return test;

        foreach (var test in GetJumpBranchInstructionTests<T>(version))
            yield return test;

        foreach (var test in GetCompareInstructionTests<T>(version))
            yield return test;

        foreach (var test in GetTrapInstructionTests<T>(version))
            yield return test;

        foreach (var test in GetUncategorizedInstructionTests<T>(version))
            yield return test;

        foreach (var test in GetSystemInstructionTests<T>(version))
            yield return test;

        foreach (var test in GetCoProcMoveInstructionTest<T>(version))
            yield return test;

        foreach (var test in GetFloatArithmeticInstructionTests<T>(version))
            yield return test;

        foreach (var test in GetFloatConvertInstructionTests<T>(version))
            yield return test;

        foreach (var test in GetFloatRoundInstructionTests<T>(version))
            yield return test;
    }

    private static IEnumerable<object[]> GetArithmeticInstructionTests<T, TSigned, TLong>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Unsigned
        yield return [new ExecutionTestCase<T>("addu $v0, $t2, $t1", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T>("addiu $v0, $t2, 10", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T>("subu $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
        yield return [new ExecutionTestCase<T>("multu $t3, $t2", Split<T, ulong>(30 * 20))];
        yield return [new ExecutionTestCase<T>("divu $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];

        // Signed (without signs)
        yield return [new ExecutionTestCase<T>("add $v0, $t2, $t1", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T>("addi $v0, $t2, 10", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T>("sub $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
        yield return [new ExecutionTestCase<T>("mult $t3, $t2", Split<T, ulong>(30 * 20))];
        yield return [new ExecutionTestCase<T>("div $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];
        yield return [new ExecutionTestCase<T>("sra $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
        yield return [new ExecutionTestCase<T>("srav $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];

        // Signed (with signs)
        unchecked
        {
            yield return [new ExecutionTestCase<T>("add $v0, $t3, $t5", T.CreateTruncating(30 + (-10)))];
            yield return [new ExecutionTestCase<T>("addi $v0, $t3, -10", T.CreateTruncating(30 + (-10)))];
            yield return [new ExecutionTestCase<T>("sub $v0, $t2, $t5", T.CreateTruncating(20 - (-10)))];
            yield return [new ExecutionTestCase<T>("mult $t3, $t6", Split<T, ulong>((ulong)(30 * -20)))];
            yield return [new ExecutionTestCase<T>("div $t3, $t6", (T.CreateTruncating((uint)(30 % -20)), T.CreateTruncating((uint)(30 / -20))))];
        }

        // Overflowing
        unchecked
        {
            // Unsigned (should overflow without trapping)
            yield return [new ExecutionTestCase<T>("addu $v0, $a2, $s1", T.CreateTruncating(uint.MaxValue + 1))];
            yield return [new ExecutionTestCase<T>("addiu $v0, $a2, 1", T.CreateTruncating(uint.MaxValue + 1))];
            yield return [new ExecutionTestCase<T>("subu $v0, $a3, $s1", T.CreateTruncating(uint.MinValue - 1))];
            yield return [new ExecutionTestCase<T>("multu $a2, $a2", Split<T, ulong>((ulong)uint.MaxValue * uint.MaxValue))];
            yield return [new ExecutionTestCase<T>("divu $a2, $a2", (T.CreateTruncating(uint.MaxValue % uint.MaxValue), T.CreateTruncating(uint.MaxValue / uint.MaxValue)))];

            // Note:
            // "mul" does not trap on overflow. We expect the low 32 bits of the result to be written back, and the high 32 bits to be discarded.
            // "mult" also does not trap on overflow, but instead writes the full 64-bit result into the high and low registers.
            // "div" does not trap on overflow either. The behavior is undefined if the quotient is too large to fit in 32 bits.
            // In practice, we will just take the low 32 bits of the quotient and discard the high 32 bits, and write the remainder to the high register.

            // Signed (without signs)
            yield return [new ExecutionTestCase<T>("add $v0, $a0, $s1", MipsTrap.ArithmeticOverflow)];                  // max + 1
            yield return [new ExecutionTestCase<T>("addi $v0, $a0, 1", MipsTrap.ArithmeticOverflow)];                   // max + 1
            yield return [new ExecutionTestCase<T>("sub $v0, $a1, $s1", MipsTrap.ArithmeticOverflow)];                  // min - 1
            yield return [new ExecutionTestCase<T>("mult $a0, $a0", Split<T, ulong>((ulong)int.MaxValue * int.MaxValue))];     // max * max
            yield return [new ExecutionTestCase<T>("div $a0, $a0", (T.CreateTruncating((uint)(int.MaxValue % int.MaxValue)), T.CreateTruncating((uint)(int.MaxValue / int.MaxValue))))];

            // Signed (with signs)
            yield return [new ExecutionTestCase<T>("add $v0, $a1, $s5", MipsTrap.ArithmeticOverflow)];     // min + (-1)
            yield return [new ExecutionTestCase<T>("addi $v0, $a1, -1", MipsTrap.ArithmeticOverflow)];     // min + (-1)
            yield return [new ExecutionTestCase<T>("sub $v0, $a0, $s5", MipsTrap.ArithmeticOverflow)];     // max - (-1)
            yield return [new ExecutionTestCase<T>("mult $a1, $a1", Split<T, ulong>((long)int.MinValue * int.MinValue))];    // min * min
            yield return [new ExecutionTestCase<T>("div $a1, $a1", (T.CreateTruncating((uint)(int.MinValue % int.MinValue)), T.CreateTruncating((uint)(int.MinValue / int.MinValue))))];
        }

        // Division by zero. Undefined behavior, but NOT a trap! (Shouldn't crash the emulator either)
        yield return [new ExecutionTestCase<T>("divu $t3, $zero", MipsTrap.None)];
        yield return [new ExecutionTestCase<T>("div $t3, $zero", MipsTrap.None)];

        if (version is >= MipsVersion.Mips_R1)
        {
            // GPR Multiply
            yield return [new ExecutionTestCase<T>("mul $v0, $t3, $t2", T.CreateTruncating(30 * 20))];
            yield return [new ExecutionTestCase<T>("mul $v0, $t3, $t6", T.CreateTruncating(unchecked((uint)(30 * -20))))];
            yield return [new ExecutionTestCase<T>("mul $v0, $a0, $a0", T.CreateTruncating((uint)unchecked(int.MaxValue * int.MaxValue)))];     // max * max
            yield return [new ExecutionTestCase<T>("mul $v0, $a1, $a1", T.CreateTruncating((uint)unchecked(int.MinValue * int.MinValue)))];     // min * min
        }

        if (version is >= MipsVersion.Mips_R1 and < MipsVersion.Mips_R6)
        {
            // Multiply and Add/Subtract
            yield return [new ExecutionTestCase<T>("maddu $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 + (30 * 20))))];
            yield return [new ExecutionTestCase<T>("madd $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 + (30 * 20))))];
            yield return [new ExecutionTestCase<T>("msubu $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 - (30 * 20))))];
            yield return [new ExecutionTestCase<T>("msub $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 - (30 * 20))))];
        }

        // Not arithmetic, but fixed width
        if (version is >= MipsVersion.Mips_R1)
        {
            // Niche bit-manipulation
            // TODO: ext, ins, seb, seh, wsbh, wshd
            yield return [new ExecutionTestCase<T>("clz $v0, $k0", T.CreateTruncating(BitOperations.LeadingZeroCount(K0)))];
            yield return [new ExecutionTestCase<T>("clo $v0, $k0", T.CreateTruncating(BitOperations.LeadingZeroCount(~K0)))];
        }
    }

    private static IEnumerable<object[]> GetLogicalInstructionTests<T>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        yield return [new ExecutionTestCase<T>("and $v0, $k0, $k1", T.CreateTruncating(K0 & K1))];
        yield return [new ExecutionTestCase<T>("andi $v0, $k0, 0xd16", T.CreateTruncating(K0 & K1))];
        yield return [new ExecutionTestCase<T>("or $v0, $k0, $k1", T.CreateTruncating(K0 | K1))];
        yield return [new ExecutionTestCase<T>("ori $v0, $k0, 0xd16", T.CreateTruncating(K0 | K1))];
        yield return [new ExecutionTestCase<T>("xor $v0, $k0, $k1", T.CreateTruncating(K0 ^ K1))];
        yield return [new ExecutionTestCase<T>("xori $v0, $k0, 0xd16", T.CreateTruncating(K0 ^ K1))];
        yield return [new ExecutionTestCase<T>("nor $v0, $k0, $k1", ~T.CreateTruncating(K0 | K1))];
        yield return [new ExecutionTestCase<T>("sll $v0, $t8, 4", T.CreateTruncating(101 << 4))];
        yield return [new ExecutionTestCase<T>("srl $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
        yield return [new ExecutionTestCase<T>("sllv $v0, $t8, $s4", T.CreateTruncating(101 << 4))];
        yield return [new ExecutionTestCase<T>("srlv $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];
    }

    private static IEnumerable<object[]> GetMemoryInstructionTests<T>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Load
        yield return [new ExecutionTestCase<T>("lb $v0, 0x1000($zero)", T.CreateTruncating(0x12))];
        yield return [new ExecutionTestCase<T>("lh $v0, 0x1000($zero)", T.CreateTruncating(0x1234))];
        yield return [new ExecutionTestCase<T>("lw $v0, 0x1000($zero)", T.CreateTruncating(0x1234_5678))];

        // TODO: Load unsigned/signed with sign

        // Store
        yield return [new ExecutionTestCase<T>("sb $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0xef, 0x34, 0x56, 0x78]))];
        yield return [new ExecutionTestCase<T>("sh $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0xcd, 0xef, 0x56, 0x78]))];
        yield return [new ExecutionTestCase<T>("sw $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0x89, 0xab, 0xcd, 0xef]))];
    }

    private static IEnumerable<object[]> GetJumpBranchInstructionTests<T>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Jump
        yield return [new ExecutionTestCase<T>("j 1000") { ExpectedPC = T.CreateTruncating(1000) }];
        yield return [new ExecutionTestCase<T>("jal 1000", GPRegister.ReturnAddress, T.CreateTruncating(4)) { ExpectedPC = T.CreateTruncating(1000) }];
        yield return [new ExecutionTestCase<T>("jr $t4") { ExpectedPC = T.CreateTruncating(40) }];
        yield return [new ExecutionTestCase<T>("jalr $t4", GPRegister.ReturnAddress, T.CreateTruncating(4)) { ExpectedPC = T.CreateTruncating(40) }];

        // Branch Equality
        yield return [new ExecutionTestCase<T>("beq $t2, $t3, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T>("beq $t1, $t1, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>("bne $t1, $t1, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T>("bne $t3, $t2, 80") { ExpectedPC = T.CreateTruncating(84) }];

        // Branch Compare
        yield return [new ExecutionTestCase<T>("blez $s1, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T>("blez $s0, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>("blez $s5, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>("bgtz $s1, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>("bgtz $s0, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T>("bgtz $s5, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T>("bltz $s1, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T>("bltz $s0, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T>("bltz $s5, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>("bgez $s1, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>("bgez $s0, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>("bgez $s5, 80") { ExpectedPC = T.CreateTruncating(4) }];
    }

    private static IEnumerable<object[]> GetCompareInstructionTests<T>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Unsigned
        yield return [new ExecutionTestCase<T>("sltu $v0, $t2, $t3", T.One)];
        yield return [new ExecutionTestCase<T>("sltu $v0, $t3, $t2", T.Zero)];
        yield return [new ExecutionTestCase<T>("sltu $v0, $t1, $t1", T.Zero)];
        yield return [new ExecutionTestCase<T>("sltiu $v0, $t2, 30", T.One)];
        yield return [new ExecutionTestCase<T>("sltiu $v0, $t3, 20", T.Zero)];
        yield return [new ExecutionTestCase<T>("sltiu $v0, $t1, 10", T.Zero)];

        // Signed (without signs)
        yield return [new ExecutionTestCase<T>("slt $v0, $t2, $t3", T.One)];
        yield return [new ExecutionTestCase<T>("slt $v0, $t3, $t2", T.Zero)];
        yield return [new ExecutionTestCase<T>("slt $v0, $t1, $t1", T.Zero)];
        yield return [new ExecutionTestCase<T>("slti $v0, $t2, 30", T.One)];
        yield return [new ExecutionTestCase<T>("slti $v0, $t3, 20", T.Zero)];
        yield return [new ExecutionTestCase<T>("slti $v0, $t1, 10", T.Zero)];

        // Signed (with signs)
        yield return [new ExecutionTestCase<T>("slt $v0, $t7, $t6", T.One)];
        yield return [new ExecutionTestCase<T>("slt $v0, $t6, $t7", T.Zero)];
        yield return [new ExecutionTestCase<T>("slt $v0, $t5, $t5", T.Zero)];
        yield return [new ExecutionTestCase<T>("slti $v0, $t7, -20", T.One)];
        yield return [new ExecutionTestCase<T>("slti $v0, $t6, -30", T.Zero)];
        yield return [new ExecutionTestCase<T>("slti $v0, $t5, -10", T.Zero)];
    }

    private static IEnumerable<object[]> GetTrapInstructionTests<T>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        if (version >= MipsVersion.MipsII)
        {
            // Equality
            yield return [new ExecutionTestCase<T>("teq $t2, $t3", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>("teq $t1, $t1", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>("tne $t1, $t1", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>("tne $t3, $t2", MipsTrap.Trap)];

            // Unsigned
            yield return [new ExecutionTestCase<T>("tltu $t3, $t2", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>("tltu $t2, $t3", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>("tltu $t1, $t1", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>("tgeu $t2, $t3", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>("tgeu $t3, $t2", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>("tgeu $t1, $t1", MipsTrap.Trap)];

            // Signed (without signs)
            yield return [new ExecutionTestCase<T>("tlt $t3, $t2", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>("tlt $t2, $t3", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>("tlt $t1, $t1", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>("tge $t2, $t3", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>("tge $t3, $t2", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>("tge $t1, $t1", MipsTrap.Trap)];

            // Signed (with signs)
            yield return [new ExecutionTestCase<T>("tlt $t6, $t7", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>("tlt $t7, $t6", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>("tlt $t5, $t5", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>("tge $t7, $t6", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>("tge $t6, $t7", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>("tge $t5, $t5", MipsTrap.Trap)];
        }
    }

    private static IEnumerable<object[]> GetUncategorizedInstructionTests<T>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // lui
        yield return [new ExecutionTestCase<T>("lui $v0, 0x1234", T.CreateTruncating(0x12340000))];

        if (version is < MipsVersion.Mips_R6)
        {
            // Move from/to high and low registers
            yield return [new ExecutionTestCase<T>("mtlo $k0", (T.CreateTruncating(0x1234), T.CreateTruncating(K0)))];
            yield return [new ExecutionTestCase<T>("mthi $k1", (T.CreateTruncating(K1), T.CreateTruncating(0x5678)))];
            yield return [new ExecutionTestCase<T>("mflo $v0", T.CreateTruncating(0x5678))];
            yield return [new ExecutionTestCase<T>("mfhi $v0", T.CreateTruncating(0x1234))];
        }

        if (version is >= MipsVersion.MipsIV)
        {
            // movz/movn
            yield return [new ExecutionTestCase<T>("movz $k0, $k1, $t0", GPRegister.Kernel0, T.CreateTruncating(K1))];
            yield return [new ExecutionTestCase<T>("movz $k0, $k1, $t1", GPRegister.Zero)];
            yield return [new ExecutionTestCase<T>("movn $k0, $k1, $t0", GPRegister.Zero)];
            yield return [new ExecutionTestCase<T>("movn $k0, $k1, $t1", GPRegister.Kernel0, T.CreateTruncating(K1))];
        }
    }

    private static IEnumerable<object[]> GetSystemInstructionTests<T>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        yield return [new ExecutionTestCase<T>("syscall", MipsTrap.Syscall)];
        yield return [new ExecutionTestCase<T>("break", MipsTrap.Breakpoint)];

        // Exception Return
        yield return [new ExecutionTestCase<T>("eret", MipsTrap.ReservedInstruction)];
        yield return [new ExecutionTestCase<T>("eret", SideEffect.WriteCoProc0)
            {
                Status = new StatusRegister
                {
                    ExceptionLevel = true
                }
            }];

        // Enable Interrupts
        yield return [new ExecutionTestCase<T>("ei", MipsTrap.ReservedInstruction)];
        yield return [new ExecutionTestCase<T>("ei", SideEffect.WriteCoProc0)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
        yield return [new ExecutionTestCase<T>("ei $v0", GPRegister.ReturnValue0)
            {
                ExpectedSideEffect = SideEffect.WriteCoProc0,
                PrivilegeMode = PrivilegeMode.Kernel
            }];

        // Disable Interrupts
        yield return [new ExecutionTestCase<T>("di", MipsTrap.ReservedInstruction)];
        yield return [new ExecutionTestCase<T>("di", SideEffect.WriteCoProc0)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
        yield return [new ExecutionTestCase<T>("di $v1", GPRegister.ReturnValue1)
            {
                ExpectedSideEffect = SideEffect.WriteCoProc0,
                PrivilegeMode = PrivilegeMode.Kernel
            }];
    }

    private static IEnumerable<object[]> GetCoProcMoveInstructionTest<T>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // CoProcessor 1
        yield return [new ExecutionTestCase<T>("mtc1 $t2, $f16", FloatRegister.F16, 20)];
        yield return [new ExecutionTestCase<T>("mfc1 $v0, $f0", GPRegister.ReturnValue0, T.CreateTruncating(2))];
    }

    private static IEnumerable<object[]> GetFloatArithmeticInstructionTests<T>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Single
        yield return [new ExecutionTestCase<T>("add.S $f16, $f8, $f9", FloatRegister.F16, 10.5f + 2.5f)];
        yield return [new ExecutionTestCase<T>("sub.S $f16, $f8, $f9", FloatRegister.F16, 10.5f - 2.5f)];
        yield return [new ExecutionTestCase<T>("mul.S $f16, $f8, $f9", FloatRegister.F16, 10.5f * 2.5f)];
        yield return [new ExecutionTestCase<T>("div.S $f16, $f8, $f9", FloatRegister.F16, 10.5f / 2.5f)];
        yield return [new ExecutionTestCase<T>("abs.S $f16, $f7", FloatRegister.F16, 2f)];
        yield return [new ExecutionTestCase<T>("neg.S $f16, $f5", FloatRegister.F16, -2f)];
        yield return [new ExecutionTestCase<T>("sqrt.S $f16, $f8", FloatRegister.F16, MathF.Sqrt(10.5f))];
        yield return [new ExecutionTestCase<T>("recip.S $f16, $f9", FloatRegister.F16, float.ReciprocalEstimate(2.5f))];

        // Double
        yield return [new ExecutionTestCase<T>("add.D $f16, $f12, $f14", FloatRegister.F16, 2d + 0.5d)];
        yield return [new ExecutionTestCase<T>("sub.D $f16, $f12, $f14", FloatRegister.F16, 2d - 0.5d)];
        yield return [new ExecutionTestCase<T>("mul.D $f16, $f12, $f14", FloatRegister.F16, 2d * 0.5d)];
        yield return [new ExecutionTestCase<T>("div.D $f16, $f12, $f14", FloatRegister.F16, 2d / 0.5d)];
        yield return [new ExecutionTestCase<T>("abs.D $f16, $f16", FloatRegister.F16, 2d)];
        yield return [new ExecutionTestCase<T>("neg.D $f16, $f12", FloatRegister.F16, -2d)];
        yield return [new ExecutionTestCase<T>("sqrt.D $f16, $f12", FloatRegister.F16, Math.Sqrt(2d))];
        yield return [new ExecutionTestCase<T>("recip.D $f16, $f12", FloatRegister.F16, double.ReciprocalEstimate(2d))];
    }

    private static IEnumerable<object[]> GetFloatConvertInstructionTests<T>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // From Single 
        yield return [new ExecutionTestCase<T>("cvt.D.S $f16, $f5", FloatRegister.F16, 2d)];     // To Double
        yield return [new ExecutionTestCase<T>("cvt.W.S $f16, $f5", FloatRegister.F16, 2)];      // To Word
        yield return [new ExecutionTestCase<T>("cvt.L.S $f16, $f5", FloatRegister.F16, 2L)];     // To Long

        // From Double
        yield return [new ExecutionTestCase<T>("cvt.S.D $f16, $f12", FloatRegister.F16, 2f)];    // To Single
        yield return [new ExecutionTestCase<T>("cvt.W.D $f16, $f12", FloatRegister.F16, 2)];     // To Word
        yield return [new ExecutionTestCase<T>("cvt.L.D $f16, $f12", FloatRegister.F16, 2L)];    // To Long

        // From Word 
        yield return [new ExecutionTestCase<T>("cvt.S.W $f16, $f0", FloatRegister.F16, 2f)];     // To Single
        yield return [new ExecutionTestCase<T>("cvt.D.W $f16, $f0", FloatRegister.F16, 2d)];     // To Double

        // From Long
        yield return [new ExecutionTestCase<T>("cvt.S.L $f16, $f0", FloatRegister.F16, 2f)];     // To Single
        yield return [new ExecutionTestCase<T>("cvt.D.L $f16, $f0", FloatRegister.F16, 2d)];     // To Double
    }

    private static IEnumerable<object[]> GetFloatRoundInstructionTests<T>(MipsVersion version)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Round
        yield return [new ExecutionTestCase<T>("round.W.S $f16, $f10", FloatRegister.F16, 1)];
        yield return [new ExecutionTestCase<T>("round.W.D $f16, $f18", FloatRegister.F16, 3)];
        yield return [new ExecutionTestCase<T>("round.L.S $f16, $f10", FloatRegister.F16, 1L)];
        yield return [new ExecutionTestCase<T>("round.L.D $f16, $f18", FloatRegister.F16, 3L)];

        // Ceiling
        yield return [new ExecutionTestCase<T>("ceil.W.S $f16, $f10", FloatRegister.F16, 2)];
        yield return [new ExecutionTestCase<T>("ceil.W.D $f16, $f18", FloatRegister.F16, 4)];
        yield return [new ExecutionTestCase<T>("ceil.L.S $f16, $f10", FloatRegister.F16, 2L)];
        yield return [new ExecutionTestCase<T>("ceil.L.D $f16, $f18", FloatRegister.F16, 4L)];

        // Floor
        yield return [new ExecutionTestCase<T>("floor.W.S $f16, $f10", FloatRegister.F16, 1)];
        yield return [new ExecutionTestCase<T>("floor.W.D $f16, $f18", FloatRegister.F16, 3)];
        yield return [new ExecutionTestCase<T>("floor.L.S $f16, $f10", FloatRegister.F16, 1L)];
        yield return [new ExecutionTestCase<T>("floor.L.D $f16, $f18", FloatRegister.F16, 3L)];
    }

    private unsafe static (T, T) Split<T, TLong>(TLong value)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TLong : unmanaged, IBinaryInteger<TLong>, IUnsignedNumber<TLong>
    {
        var size = sizeof(TLong) * 4; // Half the size of TLong in bits
        var mask = (TLong.One << (sizeof(TLong) * 4)) - TLong.One;
        return (T.CreateTruncating(value >> size), T.CreateTruncating(value & mask));
    }
}
