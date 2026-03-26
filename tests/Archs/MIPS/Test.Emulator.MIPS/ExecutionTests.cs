// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Numerics;
using Zarem.Assembler.Models;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization;
using Zarem.Emulator;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models.Enum;
using Zarem.Emulator.TrapHandlers;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Test.Emulator.MIPS;

[TestClass]
public class ExecutionTests
{
    private const uint K0 = 0xbd0;
    private const uint K1 = 0xd16;

    public sealed record ExecutionTestCase
    {
        public ExecutionTestCase(string input)
        {
            Input = input;

            PrivilegeMode = PrivilegeMode.User;

            unchecked
            {
                RegisterInitialization =
                    [
                        // Max/Min values to test edge cases, as well as some arbitrary non-edge-case values for good measure
                        // Stored in the argument registers
                        (GPRegister.Argument0, int.MaxValue),
                        (GPRegister.Argument1, (uint)int.MinValue),
                        (GPRegister.Argument2, uint.MaxValue),
                        (GPRegister.Argument3, uint.MinValue),

                        // Saved 1 - 4 are assigned to 1 through 4 respectively,
                        // while saved 5 and 6 are assigned to -1 and -2 (to test sign handling in arithmetic instructions)
                        (GPRegister.Saved1, 1),
                        (GPRegister.Saved2, 2),
                        (GPRegister.Saved3, 3),
                        (GPRegister.Saved4, 4),
                        (GPRegister.Saved5, (uint)-1),
                        (GPRegister.Saved6, (uint)-2),

                        // Temp 1 - 4 are assigned to 10, 20, 30, 40 respectively,
                        // while temp 5 and 6 are assigned to -10 and -20 (to test sign handling in arithmetic instructions)
                        (GPRegister.Temporary1, 10),
                        (GPRegister.Temporary2, 20),
                        (GPRegister.Temporary3, 30),
                        (GPRegister.Temporary4, 40),
                        (GPRegister.Temporary5, (uint)-10),
                        (GPRegister.Temporary6, (uint)-20),
                        (GPRegister.Temporary7, (uint)-30),

                        // Assign some arbitrary values to the rest of the registers as well, just in case
                        (GPRegister.Temporary8, 101),
                        (GPRegister.AssemblerTemporary, 0x89ab_cdef),
                        (GPRegister.Kernel0, K0),
                        (GPRegister.Kernel1, K1),

                        // Print integer
                        (GPRegister.ReturnValue0, 1),
                    ];

                FPRInitialization =
                    [
                        // F0 - F3: Simple Integers (for CVT.S.W or CVT.D.L tests)
                        (FloatRegister.F0, 2),
                        (FloatRegister.F1, 0),      // Note: F0 is 2 as a long or as a word
                        (FloatRegister.F2, 10),
                        (FloatRegister.F3, (uint)-10),

                        // F4 - F11: Small "Clean" Floats (Single Precision)
                        // Using values that have exact representations in binary
                        (FloatRegister.F4, BitConverter.SingleToUInt32Bits(1.0f)),
                        (FloatRegister.F5, BitConverter.SingleToUInt32Bits(2.0f)),
                        (FloatRegister.F6, BitConverter.SingleToUInt32Bits(0.5f)),
                        (FloatRegister.F7, BitConverter.SingleToUInt32Bits(-2.0f)),
                        (FloatRegister.F8, BitConverter.SingleToUInt32Bits(10.5f)),
                        (FloatRegister.F9, BitConverter.SingleToUInt32Bits(2.5f)),
                        (FloatRegister.F10, BitConverter.SingleToUInt32Bits(1.25f)),
                        (FloatRegister.F11, BitConverter.SingleToUInt32Bits(-0.75f)),

                        // F12 - F19: Double Precision Pairs (f12/13, f14/15, etc.)
                        // f12/f13 = 1.0, f14/f15 = 0.5, f16/f17 = -2.0, f18/f19 = PI (approx)
                        (FloatRegister.F12, (uint)(BitConverter.DoubleToUInt64Bits(2.0) & 0xFFFFFFFF)),
                        (FloatRegister.F13, (uint)(BitConverter.DoubleToUInt64Bits(2.0) >> 32)),

                        (FloatRegister.F14, (uint)(BitConverter.DoubleToUInt64Bits(0.5) & 0xFFFFFFFF)),
                        (FloatRegister.F15, (uint)(BitConverter.DoubleToUInt64Bits(0.5) >> 32)),

                        (FloatRegister.F16, (uint)(BitConverter.DoubleToUInt64Bits(-2.0) & 0xFFFFFFFF)),
                        (FloatRegister.F17, (uint)(BitConverter.DoubleToUInt64Bits(-2.0) >> 32)),

                        (FloatRegister.F18, (uint)(BitConverter.DoubleToUInt64Bits(Math.PI) & 0xFFFFFFFF)),
                        (FloatRegister.F19, (uint)(BitConverter.DoubleToUInt64Bits(Math.PI) >> 32)),

                        // F20 - F27: IEEE 754 Edge Cases (Single Precision)
                        (FloatRegister.F20, BitConverter.SingleToUInt32Bits(float.PositiveInfinity)),
                        (FloatRegister.F21, BitConverter.SingleToUInt32Bits(float.NegativeInfinity)),
                        (FloatRegister.F22, BitConverter.SingleToUInt32Bits(float.NaN)),
                        (FloatRegister.F23, BitConverter.SingleToUInt32Bits(0.0f)),
                        (FloatRegister.F24, BitConverter.SingleToUInt32Bits(-0.0f)),
                        (FloatRegister.F25, BitConverter.SingleToUInt32Bits(float.Epsilon)), // Subnormal/Tiny
                        (FloatRegister.F26, BitConverter.SingleToUInt32Bits(float.MaxValue)),
                        (FloatRegister.F27, BitConverter.SingleToUInt32Bits(float.MinValue)),

                        // F28 - F31: Large Integers (to test Rounding/Overflow traps)
                        (FloatRegister.F28, (uint)int.MaxValue),
                        (FloatRegister.F29, 0), // Upper bits for F28 if treated as Long
                        (FloatRegister.F30, (uint)int.MinValue),
                        (FloatRegister.F31, 0xFFFFFFFF) // All bits set
                    ];

                InitialHighLow = (0x1234, 0x5678);

                MemoryInitialization =
                    [(0x1000, [0x12, 0x34, 0x56, 0x78])];
            }
        }

        public ExecutionTestCase(string input, MipsTrap trap) : this(input)
        {
            ExpectedTrap = trap;
        }

        public ExecutionTestCase(string input, uint writeBack) : this(input)
        {
            ExpectedWriteBack = (GPRegister.ReturnValue0, writeBack);
        }

        public ExecutionTestCase(string input, GPRegister reg, uint? writeBack = null) : this(input)
        {
            ExpectedWriteBack = (reg, writeBack);
        }

        public ExecutionTestCase(string input, FloatRegister reg, float writeBack) : this(input, reg, BitConverter.SingleToInt32Bits(writeBack))
        {
        }

        public ExecutionTestCase(string input, FloatRegister reg, double writeBack) : this(input, reg, BitConverter.DoubleToInt64Bits(writeBack))
        {
        }

        public ExecutionTestCase(string input, FloatRegister reg, int writeBack) : this(input)
        {
            ExpectedWordFloatWriteBack = (reg, writeBack);
        }

        public ExecutionTestCase(string input, FloatRegister reg, long writeBack) : this(input)
        {
            ExpectedLongFloatWriteBack = (reg, writeBack);
        }

        public ExecutionTestCase(string input, (uint, byte[]) memory) : this(input)
        {
            ExpectedMemory = memory;
        }

        public ExecutionTestCase(string input, ulong highLow) : this(input)
        {
            ExpectedHighLow = ((uint)(highLow >> 32), (uint)highLow);
        }

        public ExecutionTestCase(string input, (uint, uint) highLow) : this(input)
        {
            ExpectedHighLow = highLow;
        }

        public ExecutionTestCase(string input, SideEffect sideEffects) : this(input)
        {
            ExpectedSideEffect = sideEffects;
        }

        public string Input { get; }

        public MipsTrap ExpectedTrap { get; init; } = MipsTrap.None;

        public (GPRegister Regiter, uint? Value)? ExpectedWriteBack { get; init; } = null;

        public (FloatRegister Register, int Value)? ExpectedWordFloatWriteBack { get; init; } = null;

        public (FloatRegister Register, long Value)? ExpectedLongFloatWriteBack { get; init; } = null;

        public uint? ExpectedPC { get; init; } = null;

        public SideEffect? ExpectedSideEffect { get; init; }

        public (uint Address, byte[] Data)? ExpectedMemory { get; init; }

        public (uint High, uint Low)? ExpectedHighLow { get; init; }

        public (GPRegister Register, uint Value)[] RegisterInitialization { get; init; } = [];

        public (FloatRegister Register, uint Value)[] FPRInitialization { get; init; } = [];

        public (uint Address, byte[] Data)[] MemoryInitialization { get; init; } = [];

        public (uint High, uint Low)? InitialHighLow { get; init; }

        public PrivilegeMode PrivilegeMode
        {
            get => Status.PrivilegeMode;
            init => Status = Status with { PrivilegeMode = value };
        }

        public StatusRegister Status { get; init; }
    }

    public static IEnumerable<object[]> ArithmeticInstructionTestsList
    {
        get
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
            yield return [new ExecutionTestCase("mul $v0, $t3, $t2", 30 * 20)];
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
                yield return [new ExecutionTestCase("mul $v0, $t3, $t6", (uint)(30 * -20))];
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
                yield return [new ExecutionTestCase("mul $v0, $a0, $a0", int.MaxValue * int.MaxValue)];             // max * max
                yield return [new ExecutionTestCase("mult $a0, $a0", (long)int.MaxValue * int.MaxValue)];           // max * max
                yield return [new ExecutionTestCase("div $a0, $a0", ((uint)((long)int.MaxValue % int.MaxValue), (uint)((long)int.MaxValue / int.MaxValue)))];

                // Signed (with signs)
                yield return [new ExecutionTestCase("add $v0, $a1, $s5", MipsTrap.ArithmeticOverflow)];             // min + (-1)
                yield return [new ExecutionTestCase("addi $v0, $a1, -1", MipsTrap.ArithmeticOverflow)];             // min + (-1)
                yield return [new ExecutionTestCase("sub $v0, $a0, $s5", MipsTrap.ArithmeticOverflow)];             // max - (-1)
                yield return [new ExecutionTestCase("mul $v0, $a1, $a1", (uint)(int.MinValue * int.MinValue))];     // min * min
                yield return [new ExecutionTestCase("mult $a1, $a1", (long)int.MinValue * int.MinValue)];           // min * min
                yield return [new ExecutionTestCase("div $a1, $a1", ((uint)((long)int.MinValue % int.MinValue), (uint)((long)int.MinValue / int.MinValue)))];
            }

            // Division by zero. Undefined behavior, but NOT a trap! (Shouldn't crash the emulator either)
            yield return [new ExecutionTestCase("divu $t3, $zero", MipsTrap.None)];
            yield return [new ExecutionTestCase("div $t3, $zero", MipsTrap.None)];

            // Multiply and Add/Subtract
            yield return [new ExecutionTestCase("maddu $t3, $t2", (0x1234, 0x5678 + (30 * 20)))];
            yield return [new ExecutionTestCase("madd $t3, $t2", (0x1234, 0x5678 + (30 * 20)))];
            yield return [new ExecutionTestCase("msubu $t3, $t2", (0x1234, 0x5678 - (30 * 20)))];
            yield return [new ExecutionTestCase("msub $t3, $t2", (0x1234, 0x5678 - (30 * 20)))];
        }
    }

    public static IEnumerable<object[]> LogicalInstructionTestsList
    {
        get
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
    }

    public static IEnumerable<object[]> MemoryInstructionTestsList
    {
        get
        {
            // Load
            yield return [new ExecutionTestCase("lb $v0, 0x1000($zero)", 0x12)];
            yield return [new ExecutionTestCase("lh $v0, 0x1000($zero)", 0x1234)];
            yield return [new ExecutionTestCase("lw $v0, 0x1000($zero)", 0x1234_5678)];

            // Store
            yield return [new ExecutionTestCase("sb $at, 0x1000($zero)", (0x1000, [0xef, 0x34, 0x56, 0x78]))];
            yield return [new ExecutionTestCase("sh $at, 0x1000($zero)", (0x1000, [0xcd, 0xef, 0x56, 0x78]))];
            yield return [new ExecutionTestCase("sw $at, 0x1000($zero)", (0x1000, [0x89, 0xab, 0xcd, 0xef]))];
        }
    }

    public static IEnumerable<object[]> JumpBranchInstructionTestsList
    {
        get
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
    }

    public static IEnumerable<object[]> CompareInstructionTestsList
    {
        get
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
    }

    public static IEnumerable<object[]> TrapInstructionTestsList
    {
        get
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

    public static IEnumerable<object[]> UncategorizedRegisterOnlyInstructionTestsList
    {
        get
        {
            // movz/movn
            yield return [new ExecutionTestCase("movz $k0, $k1, $t0", GPRegister.Kernel0, K1)];
            yield return [new ExecutionTestCase("movz $k0, $k1, $t1", GPRegister.Zero)];
            yield return [new ExecutionTestCase("movn $k0, $k1, $t0", GPRegister.Zero)];
            yield return [new ExecutionTestCase("movn $k0, $k1, $t1", GPRegister.Kernel0, K1)];

            // lui
            yield return [new ExecutionTestCase("lui $v0, 0x1234", 0x12340000)];

            // Move from/to high and low registers
            yield return [new ExecutionTestCase("mtlo $k0", (0x1234, K0))];
            yield return [new ExecutionTestCase("mthi $k1", (K1, 0x5678))];
            yield return [new ExecutionTestCase("mflo $v0", 0x5678)];
            yield return [new ExecutionTestCase("mfhi $v0", 0x1234)];

            // Niche bit-manipulation
            // TODO: ext, ins, seb, seh, wsbh, wshd
            yield return [new ExecutionTestCase("clz $v0, $k0", (uint)BitOperations.LeadingZeroCount(K0))];
            yield return [new ExecutionTestCase("clo $v0, $k0", (uint)BitOperations.LeadingZeroCount(~K0))];
        }
    }

    public static IEnumerable<object[]> SystemInstructionTestsList
    {
        get
        {
            yield return [new ExecutionTestCase("syscall", MipsTrap.Syscall)];
            yield return [new ExecutionTestCase("break", MipsTrap.Breakpoint)];

            // Exception Return
            yield return [new ExecutionTestCase("eret", MipsTrap.ReservedInstruction)];
            yield return [new ExecutionTestCase("eret", SideEffect.WriteCoProc)
            {
                Status = new StatusRegister
                {
                    ExceptionLevel = true
                }
            }];

            // Enable Interrupts
            yield return [new ExecutionTestCase("ei", MipsTrap.ReservedInstruction)];
            yield return [new ExecutionTestCase("ei", SideEffect.WriteCoProc)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
            yield return [new ExecutionTestCase("ei $v0", GPRegister.ReturnValue0)
            {
                ExpectedSideEffect = SideEffect.WriteCoProc,
                PrivilegeMode = PrivilegeMode.Kernel
            }];

            // Disable Interrupts
            yield return [new ExecutionTestCase("di", MipsTrap.ReservedInstruction)];
            yield return [new ExecutionTestCase("di", SideEffect.WriteCoProc)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
            yield return [new ExecutionTestCase("di $v1", GPRegister.ReturnValue1)
            {
                ExpectedSideEffect = SideEffect.WriteCoProc,
                PrivilegeMode = PrivilegeMode.Kernel
            }];
        }
    }

    public static IEnumerable<object[]> CoProcMoveInstructionTestList
    {
        get
        {
            // CoProcessor 1
            yield return [new ExecutionTestCase("mtc1 $t2, $f16", FloatRegister.F16, 20)];
            yield return [new ExecutionTestCase("mfc1 $v0, $f0", GPRegister.ReturnValue0, 2)];
        }
    }

    public static IEnumerable<object[]> FloatArithmeticInstructionTestsList
    {
        get
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
    }

    public static IEnumerable<object[]> FloatConvertInstructionTestsList
    {
        get
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
    }

    public static IEnumerable<object[]> FloatRoundInstructionTestsList
    {
        get
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

    [DataTestMethod]
    [DynamicData(nameof(ArithmeticInstructionTestsList))]
    public void ArithmeticInstructionTests(ExecutionTestCase @case) => RunTest(@case);

    [DataTestMethod]
    [DynamicData(nameof(LogicalInstructionTestsList))]
    public void LogicalInstructionTests(ExecutionTestCase @case) => RunTest(@case);

    [DataTestMethod]
    [DynamicData(nameof(MemoryInstructionTestsList))]
    public void MemoryInstructionTests(ExecutionTestCase @case) => RunTest(@case);

    [DataTestMethod]
    [DynamicData(nameof(JumpBranchInstructionTestsList))]
    public void JumpBranchInstructionTests(ExecutionTestCase @case) => RunTest(@case);

    [DataTestMethod]
    [DynamicData(nameof(JumpBranchInstructionTestsList))]
    public void JumpBranchNoDeplayInstructionTests(ExecutionTestCase @case) => RunTest(@case, false);

    [DataTestMethod]
    [DynamicData(nameof(CompareInstructionTestsList))]
    public void CompareInstructionTests(ExecutionTestCase @case) => RunTest(@case);

    [DataTestMethod]
    [DynamicData(nameof(TrapInstructionTestsList))]
    public void TrapInstructionTests(ExecutionTestCase @case) => RunTest(@case);

    [DataTestMethod]
    [DynamicData(nameof(UncategorizedRegisterOnlyInstructionTestsList))]
    public void UncategorizedRegisterOnlyInstructionTests(ExecutionTestCase @case) => RunTest(@case);

    [DataTestMethod]
    [DynamicData(nameof(SystemInstructionTestsList))]
    public void SystemInstructionTests(ExecutionTestCase @case) => RunTest(@case);

    [DataTestMethod]
    [DynamicData(nameof(CoProcMoveInstructionTestList))]
    public void CoProcMoveInstructionTest(ExecutionTestCase @case) => RunTest(@case);

    [DataTestMethod]
    [DynamicData(nameof(FloatArithmeticInstructionTestsList))]
    public void FloatArithmeticInstructionTests(ExecutionTestCase @case) => RunTest(@case);

    [DataTestMethod]
    [DynamicData(nameof(FloatConvertInstructionTestsList))]
    public void FloatConvertInstructionTests(ExecutionTestCase @case) => RunTest(@case);

    [DataTestMethod]
    [DynamicData(nameof(FloatRoundInstructionTestsList))]
    public void FloatRoundInstructionTests(ExecutionTestCase @case) => RunTest(@case);

    private static void RunTest(ExecutionTestCase @case, bool delaysSlots = true)
    {
        // The instruction parser is only used to convert the instruction string into an Instruction struct, so we can test the interpreter with it.
        var tokenized = Tokenizer.TokenizeLine(@case.Input)[0];
        var table = new InstructionTable(new());
        var parser = new MipsInstructionParser(new(), table, default, null, null);
        var parsed = parser.Parse(tokenized);
        if (parsed is null)
            Assert.Fail();

        // TODO: Psuedo instruction support
        var instruction = parsed.Realize()[0];
        var emulatorConfig = new MIPSEmulatorConfig()
        {
            DisableDelaySlots = !delaysSlots,
            TrapHost = new ZaremTrapHandler(),
        };
        var computer = new MipsComputer(emulatorConfig);
        var emulator = new Zaremulator(computer);

        // Initialize the status register
        computer.Processor.CoProcessor0.StatusRegister = @case.Status;

        // Initialize the register file with the provided values
        foreach (var (reg, value) in @case.RegisterInitialization)
            computer.Processor[reg] = value;

        foreach (var (reg, value) in @case.FPRInitialization)
        {
            computer.Processor.FloatProcessor[reg] = value;
        }

        // Initialize the high and low registers if specified in the test case
        if (@case.InitialHighLow.HasValue)
        {
            computer.Processor.Low = @case.InitialHighLow.Value.Low;
            computer.Processor.High = @case.InitialHighLow.Value.High;
        }

        // Initialize the memory, if specified in the test case
        foreach (var (address, data) in @case.MemoryInitialization)
            computer.Memory.Write(address, data);

        computer.Processor.Insert(instruction, out var execution, out var trap);

        // Ensure that the expected trap was raised (if any)
        Assert.AreEqual(@case.ExpectedTrap, trap);

        var writeback = @case.ExpectedWriteBack;
        if (writeback.HasValue)
        {
            // Ensure that the expected register was written to with the expected value
            Assert.AreEqual(writeback.Value.Regiter, execution.GPR);

            var writeBackValue = writeback.Value.Value;
            if (writeBackValue.HasValue)
            {
                Assert.AreEqual(writeBackValue.Value, computer.Processor.RegisterFile[execution.GPR]);
            }
        }
        else
        {
            // If no register check was provided, we at least want to make sure no register was written to (as that would be unexpected)
            Assert.AreEqual(GPRegister.Zero, execution.GPR);
        }

        var highLow = @case.ExpectedHighLow;
        if (highLow.HasValue)
        {
            Assert.AreEqual(highLow.Value.Low, computer.Processor.Low);
            Assert.AreEqual(highLow.Value.High, computer.Processor.High);
        }

        var expectedMemory = @case.ExpectedMemory;
        if (expectedMemory is not null)
        {
            var buffer = new byte[expectedMemory.Value.Data.Length];
            computer.Memory.Read(expectedMemory.Value.Address, buffer);
            CollectionAssert.AreEqual(expectedMemory.Value.Data, buffer);
        }

        var expectedFloatWord = @case.ExpectedWordFloatWriteBack;
        if (expectedFloatWord.HasValue)
        {
            Assert.AreEqual(expectedFloatWord.Value.Register, execution.FloatReg);
            Assert.AreEqual(expectedFloatWord.Value.Value, execution.FWordWriteBack);
            Assert.AreEqual(expectedFloatWord.Value.Value, computer.Processor.FloatProcessor.Words[execution.FloatReg]);
        }

        var expectedFloatLong = @case.ExpectedLongFloatWriteBack;
        if (expectedFloatLong.HasValue)
        {
            Assert.AreEqual(expectedFloatLong.Value.Register, execution.FloatReg);
            Assert.AreEqual(expectedFloatLong.Value.Value, execution.FLongWriteBack);
            Assert.AreEqual(expectedFloatLong.Value.Value, computer.Processor.FloatProcessor.Longs[execution.FloatReg]);
        }

        var expectedPC = @case.ExpectedPC;
        if (expectedPC is not null)
        {
            if (delaysSlots && execution.SideEffect is SideEffect.ProgramCounter)
            {
                // Assert the branch has not occured, then execute a NOP to apply the delayed branch
                Assert.AreEqual((uint)4, computer.Processor.ProgramCounter);
                computer.Processor.Insert(MipsInstruction.NOP, out _);
            }

            Assert.AreEqual(expectedPC.Value, computer.Processor.ProgramCounter);
        }
    }
}
