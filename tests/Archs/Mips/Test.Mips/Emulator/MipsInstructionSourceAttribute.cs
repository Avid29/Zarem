// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Test.Archs.Emulator;
using Zarem.Emulator.Config;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Registers;
using Zarem.Extensions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Registers;

namespace Test.MIPS.Emulator;

[AttributeUsage(AttributeTargets.Method)]
public class MipsInstructionSourceAttribute : InstructionSourceAttribute<MipsEmulatorConfig>
{
    public const uint K0 = MipsExecutionTests.K0;
    public const uint K1 = MipsExecutionTests.K1;

    private readonly MipsVersion _version;
    private readonly ExecutionMode _mode;

    public MipsInstructionSourceAttribute(MipsVersion version, ExecutionMode mode)
    {
        _version = version;
        _mode = mode;
    }

    public override IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        var config = new MipsEmulatorConfig(_version)
        {
            ExecutionMode = _mode,
        };

        return _version.Is64Bit()
            ? GetVersionTests<ulong, long, UInt128>(config)
            : GetVersionTests<uint, int, ulong>(config);
    }

    public override string? GetDisplayName(MethodInfo methodInfo, object?[]? data)
    {
        var obj = data?[0];
        if (obj is null)
        {
            return string.Empty;
        }

        dynamic run = obj;
        var str = $"{run?.Input}"; // Short name since the method name handles the context

        if (run?.Config is MipsEmulatorConfig { DisableDelaySlots: true })
        {
            str += " (Delay Slots Disabled)";
        }

        return str;
    }

    private static IEnumerable<object[]> GetVersionTests<T, TSigned, TLong>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
        where TLong : unmanaged, IBinaryInteger<TLong>, IUnsignedNumber<TLong>, IMinMaxValue<TLong>
    {
        var noDelayConfig = new MipsEmulatorConfig(config.Version)
        {
            TrapHost = config.TrapHost,
            ExecutionMode = config.ExecutionMode,
            DisableDelaySlots = true
        };

        return GetArithmeticInstructionTests<T, TSigned, TLong>(config)
            .Concat(GetLogicalInstructionTests<T>(config))
            .Concat(GetMemoryInstructionTests<T>(config))
            .Concat(GetJumpBranchInstructionTests<T>(config))
            .Concat(GetJumpBranchInstructionTests<T>(noDelayConfig))
            .Concat(GetCompareInstructionTests<T>(config))
            .Concat(GetTrapInstructionTests<T>(config))
            .Concat(GetUncategorizedInstructionTests<T>(config))
            .Concat(GetSystemInstructionTests<T>(config))
            .Concat(GetCoProcMoveInstructionTest<T>(config))
            .Concat(GetFloatArithmeticInstructionTests<T>(config))
            .Concat(GetFloatConvertInstructionTests<T>(config))
            .Concat(GetFloatRoundInstructionTests<T>(config))
            .Concat(GetFloatMoveInstructionTests<T>(config));
    }

    private static IEnumerable<object[]> GetArithmeticInstructionTests<T, TSigned, TLong>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
    {
        // Unsigned
        yield return [new MipsEmulatorTestCase<T>(config, "addu $v0, $t2, $t1", T.CreateTruncating(30))];
        yield return [new MipsEmulatorTestCase<T>(config, "addiu $v0, $t2, 10", T.CreateTruncating(30))];
        yield return [new MipsEmulatorTestCase<T>(config, "subu $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
        yield return [new MipsEmulatorTestCase<T>(config, "multu $t3, $t2", Split<T, ulong>(30 * 20))];
        yield return [new MipsEmulatorTestCase<T>(config, "divu $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];

        // Signed (without signs)
        yield return [new MipsEmulatorTestCase<T>(config, "add $v0, $t2, $t1", T.CreateTruncating(30))];
        yield return [new MipsEmulatorTestCase<T>(config, "addi $v0, $t2, 10", T.CreateTruncating(30))];
        yield return [new MipsEmulatorTestCase<T>(config, "sub $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
        yield return [new MipsEmulatorTestCase<T>(config, "mult $t3, $t2", Split<T, long>(30 * 20))];
        yield return [new MipsEmulatorTestCase<T>(config, "div $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];
        yield return [new MipsEmulatorTestCase<T>(config, "sra $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
        yield return [new MipsEmulatorTestCase<T>(config, "srav $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];

        // Signed (with signs)
        unchecked
        {
            yield return [new MipsEmulatorTestCase<T>(config, "add $v0, $t3, $t5", T.CreateTruncating(30 + (-10)))];
            yield return [new MipsEmulatorTestCase<T>(config, "add $v0, $t5, $t3", T.CreateTruncating((-10) + 30))];
            yield return [new MipsEmulatorTestCase<T>(config, "addi $v0, $t3, -10", T.CreateTruncating(30 + (-10)))];
            yield return [new MipsEmulatorTestCase<T>(config, "addi $v0, $t7, 10", T.CreateTruncating(-30 + 10))];
            yield return [new MipsEmulatorTestCase<T>(config, "sub $v0, $t2, $t5", T.CreateTruncating(20 - (-10)))];
            yield return [new MipsEmulatorTestCase<T>(config, "sub $v0, $t5, $t2", T.CreateTruncating(-10 - 20))];
            yield return [new MipsEmulatorTestCase<T>(config, "mult $t3, $t6", Split<T, long>(30 * -20))];
            yield return [new MipsEmulatorTestCase<T>(config, "div $t3, $t6", (T.CreateTruncating(30 % -20), T.CreateTruncating(30 / -20)))];
        }

        // Overflowing
        unchecked
        {
            // Unsigned (should overflow without trapping)
            yield return [new MipsEmulatorTestCase<T>(config, "addu $v0, $a2, $s1", T.CreateTruncating(uint.MaxValue + 1))];
            yield return [new MipsEmulatorTestCase<T>(config, "addiu $v0, $a2, 1", T.CreateTruncating(uint.MaxValue + 1))];
            yield return [new MipsEmulatorTestCase<T>(config, "subu $v0, $a3, $s1", T.CreateTruncating(uint.MinValue - 1))];
            yield return [new MipsEmulatorTestCase<T>(config, "multu $a2, $a2", Split<T, ulong>((ulong)uint.MaxValue * uint.MaxValue))];
            yield return [new MipsEmulatorTestCase<T>(config, "divu $a2, $a2", (T.CreateTruncating(uint.MaxValue % uint.MaxValue), T.CreateTruncating(uint.MaxValue / uint.MaxValue)))];

            // Note:
            // "mul" does not trap on overflow. We expect the low 32 bits of the result to be written back, and the high 32 bits to be discarded.
            // "mult" also does not trap on overflow, but instead writes the full 64-bit result into the high and low registers.
            // "div" does not trap on overflow either. The behavior is undefined if the quotient is too large to fit in 32 bits.
            // In practice, we will just take the low 32 bits of the quotient and discard the high 32 bits, and write the remainder to the high register.

            // Signed (without signs)
            yield return [new MipsEmulatorTestCase<T>(config, "add $v0, $a0, $s1", MipsTrap.ArithmeticOverflow)];                  // max + 1
            yield return [new MipsEmulatorTestCase<T>(config, "addi $v0, $a0, 1", MipsTrap.ArithmeticOverflow)];                   // max + 1
            yield return [new MipsEmulatorTestCase<T>(config, "sub $v0, $a1, $s1", MipsTrap.ArithmeticOverflow)];                  // min - 1
            yield return [new MipsEmulatorTestCase<T>(config, "mult $a0, $a0", Split<T, long>((long)int.MaxValue * int.MaxValue))];     // max * max
            yield return [new MipsEmulatorTestCase<T>(config, "div $a0, $a0", (T.CreateTruncating((uint)(int.MaxValue % int.MaxValue)), T.CreateTruncating((uint)(int.MaxValue / int.MaxValue))))];

            // Signed (with signs)
            yield return [new MipsEmulatorTestCase<T>(config, "add $v0, $a1, $s5", MipsTrap.ArithmeticOverflow)];     // min + (-1)
            yield return [new MipsEmulatorTestCase<T>(config, "addi $v0, $a1, -1", MipsTrap.ArithmeticOverflow)];     // min + (-1)
            yield return [new MipsEmulatorTestCase<T>(config, "sub $v0, $a0, $s5", MipsTrap.ArithmeticOverflow)];     // max - (-1)
            yield return [new MipsEmulatorTestCase<T>(config, "mult $a1, $a1", Split<T, long>((long)int.MinValue * int.MinValue))];    // min * min
            yield return [new MipsEmulatorTestCase<T>(config, "div $a1, $a1", (T.CreateTruncating((uint)(int.MinValue % int.MinValue)), T.CreateTruncating((uint)(int.MinValue / int.MinValue))))];
        }

        // Division by zero. Undefined behavior, but NOT a trap! (Shouldn't crash the emulator either)
        yield return [new MipsEmulatorTestCase<T>(config, "divu $t3, $zero", MipsTrap.None)];
        yield return [new MipsEmulatorTestCase<T>(config, "div $t3, $zero", MipsTrap.None)];

        if (config.Version is >= MipsVersion.Mips_R1)
        {
            // GPR Multiply
            yield return [new MipsEmulatorTestCase<T>(config, "mul $v0, $t3, $t2", T.CreateTruncating(30 * 20))];
            yield return [new MipsEmulatorTestCase<T>(config, "mul $v0, $t3, $t6", T.CreateTruncating(unchecked(30 * -20)))];
            yield return [new MipsEmulatorTestCase<T>(config, "mul $v0, $a0, $a0", T.CreateTruncating(unchecked(int.MaxValue * int.MaxValue)))];     // max * max
            yield return [new MipsEmulatorTestCase<T>(config, "mul $v0, $a1, $a1", T.CreateTruncating(unchecked(int.MinValue * int.MinValue)))];     // min * min
        }

        if (config.Version is >= MipsVersion.Mips_R1 and < MipsVersion.Mips_R6)
        {
            // Multiply and Add/Subtract
            yield return [new MipsEmulatorTestCase<T>(config, "maddu $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 + (30 * 20))))];
            yield return [new MipsEmulatorTestCase<T>(config, "madd $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 + (30 * 20))))];
            yield return [new MipsEmulatorTestCase<T>(config, "msubu $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 - (30 * 20))))];
            yield return [new MipsEmulatorTestCase<T>(config, "msub $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 - (30 * 20))))];
        }

        // Not arithmetic, but fixed width
        if (config.Version is >= MipsVersion.Mips_R1)
        {
            // Niche bit-manipulation
            // TODO: ext, ins, seb, seh, wsbh, wshd
            yield return [new MipsEmulatorTestCase<T>(config, "clz $v0, $k0", T.CreateTruncating(BitOperations.LeadingZeroCount(K0)))];
            yield return [new MipsEmulatorTestCase<T>(config, "clo $v0, $k0", T.CreateTruncating(BitOperations.LeadingZeroCount(~K0)))];
        }

        if (config.Version is >= MipsVersion.MipsIII && config.Version.Is64Bit())
        {
            // Unsigned
            yield return [new MipsEmulatorTestCase<T>(config, "daddu $v0, $t2, $t1", T.CreateTruncating(30))];
            yield return [new MipsEmulatorTestCase<T>(config, "daddiu $v0, $t2, 10", T.CreateTruncating(30))];
            yield return [new MipsEmulatorTestCase<T>(config, "dsubu $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
            yield return [new MipsEmulatorTestCase<T>(config, "dmultu $t3, $t2", Split<T, UInt128>(30 * 20))];
            yield return [new MipsEmulatorTestCase<T>(config, "ddivu $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];

            // Signed (without signs)
            yield return [new MipsEmulatorTestCase<T>(config, "dadd $v0, $t2, $t1", T.CreateTruncating(30))];
            yield return [new MipsEmulatorTestCase<T>(config, "daddi $v0, $t2, 10", T.CreateTruncating(30))];
            yield return [new MipsEmulatorTestCase<T>(config, "dsub $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
            yield return [new MipsEmulatorTestCase<T>(config, "dmult $t3, $t2", Split<T, UInt128>(30 * 20))];
            yield return [new MipsEmulatorTestCase<T>(config, "ddiv $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];
            yield return [new MipsEmulatorTestCase<T>(config, "dsra $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
            yield return [new MipsEmulatorTestCase<T>(config, "dsrav $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];

            // Signed (with signs)
            unchecked
            {
                yield return [new MipsEmulatorTestCase<T>(config, "dadd $v0, $t3, $t5", T.CreateTruncating(30 + (-10)))];
                yield return [new MipsEmulatorTestCase<T>(config, "daddi $v0, $t3, -10", T.CreateTruncating(30 + (-10)))];
                yield return [new MipsEmulatorTestCase<T>(config, "daddi $v0, $t5, 30", T.CreateTruncating((-10) + 30))];
                yield return [new MipsEmulatorTestCase<T>(config, "dsub $v0, $t2, $t5", T.CreateTruncating(20 - (-10)))];
                yield return [new MipsEmulatorTestCase<T>(config, "dsub $v0, $t5, $t2", T.CreateTruncating((-10) - 20))];
                yield return [new MipsEmulatorTestCase<T>(config, "dmult $t3, $t6", Split<T, UInt128>((UInt128)(30 * -20)))];
                yield return [new MipsEmulatorTestCase<T>(config, "ddiv $t3, $t6", (T.CreateTruncating(30 % -20), T.CreateTruncating(30 / -20)))];
            }
        }
    }

    private static IEnumerable<object[]> GetLogicalInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        yield return [new MipsEmulatorTestCase<T>(config, "and $v0, $k0, $k1", T.CreateTruncating(K0 & K1))];
        yield return [new MipsEmulatorTestCase<T>(config, "andi $v0, $k0, 0xd16", T.CreateTruncating(K0 & K1))];
        yield return [new MipsEmulatorTestCase<T>(config, "or $v0, $k0, $k1", T.CreateTruncating(K0 | K1))];
        yield return [new MipsEmulatorTestCase<T>(config, "ori $v0, $k0, 0xd16", T.CreateTruncating(K0 | K1))];
        yield return [new MipsEmulatorTestCase<T>(config, "xor $v0, $k0, $k1", T.CreateTruncating(K0 ^ K1))];
        yield return [new MipsEmulatorTestCase<T>(config, "xori $v0, $k0, 0xd16", T.CreateTruncating(K0 ^ K1))];
        yield return [new MipsEmulatorTestCase<T>(config, "nor $v0, $k0, $k1", ~T.CreateTruncating(K0 | K1))];
        yield return [new MipsEmulatorTestCase<T>(config, "sll $v0, $t8, 4", T.CreateTruncating(101 << 4))];
        yield return [new MipsEmulatorTestCase<T>(config, "srl $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
        yield return [new MipsEmulatorTestCase<T>(config, "sllv $v0, $t8, $s4", T.CreateTruncating(101 << 4))];
        yield return [new MipsEmulatorTestCase<T>(config, "srlv $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];

        if (config.Version is >= MipsVersion.MipsIII && config.Version.Is64Bit())
        {
            yield return [new MipsEmulatorTestCase<T>(config, "dsll $v0, $t8, 4", T.CreateTruncating(101 << 4))];
            yield return [new MipsEmulatorTestCase<T>(config, "dsrl $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
            yield return [new MipsEmulatorTestCase<T>(config, "dsllv $v0, $t8, $s4", T.CreateTruncating(101 << 4))];
            yield return [new MipsEmulatorTestCase<T>(config, "dsrlv $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];
        }
    }

    private static IEnumerable<object[]> GetMemoryInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Load
        yield return [new MipsEmulatorTestCase<T>(config, "lb $v0, 0x1000($zero)", T.CreateTruncating(0x12))];
        yield return [new MipsEmulatorTestCase<T>(config, "lh $v0, 0x1000($zero)", T.CreateTruncating(0x1234))];
        yield return [new MipsEmulatorTestCase<T>(config, "lw $v0, 0x1000($zero)", T.CreateTruncating(0x1234_5678))];

        // TODO: Load unsigned/signed with sign

        // Store
        yield return [new MipsEmulatorTestCase<T>(config, "sb $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0xef, 0x34, 0x56, 0x78]))];
        yield return [new MipsEmulatorTestCase<T>(config, "sh $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0xcd, 0xef, 0x56, 0x78]))];
        yield return [new MipsEmulatorTestCase<T>(config, "sw $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0x89, 0xab, 0xcd, 0xef]))];
    }

    private static IEnumerable<object[]> GetJumpBranchInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        var linkAddress = T.CreateTruncating(config.DisableDelaySlots ? 4 : 8);
        var noBranchAddress = T.CreateTruncating(config.ExecutionMode is ExecutionMode.JustInTime && !config.DisableDelaySlots ? 8 : 4);
        var branchAddress = T.CreateTruncating(84);

        // Jump
        yield return [new MipsEmulatorTestCase<T>(config, "j 1000") { ExpectedPC = T.CreateTruncating(1000) }];
        yield return [new MipsEmulatorTestCase<T>(config, "jr $t4") { ExpectedPC = T.CreateTruncating(40) }];

        // Jump And Link
        yield return [new MipsEmulatorTestCase<T>(config, "jal 1000", MipsGpRegister.ReturnAddress, linkAddress) { ExpectedPC = T.CreateTruncating(1000) }];
        yield return [new MipsEmulatorTestCase<T>(config, "jalr $t4", MipsGpRegister.ReturnAddress, linkAddress) { ExpectedPC = T.CreateTruncating(40) }];

        // Branch Equality: True
        yield return [new MipsEmulatorTestCase<T>(config, "beq $t1, $t1, 80") { ExpectedPC = branchAddress }];
        yield return [new MipsEmulatorTestCase<T>(config, "bne $t3, $t2, 80") { ExpectedPC = branchAddress }];

        // Branch Equality: False
        yield return [new MipsEmulatorTestCase<T>(config, "beq $t2, $t3, 80") { ExpectedPC = noBranchAddress }];
        yield return [new MipsEmulatorTestCase<T>(config, "bne $t1, $t1, 80") { ExpectedPC = noBranchAddress }];

        // Branch Compare: True
        yield return [new MipsEmulatorTestCase<T>(config, "blez $s0, 80") { ExpectedPC = branchAddress }];
        yield return [new MipsEmulatorTestCase<T>(config, "blez $s5, 80") { ExpectedPC = branchAddress }];
        yield return [new MipsEmulatorTestCase<T>(config, "bgtz $s1, 80") { ExpectedPC = branchAddress }];
        yield return [new MipsEmulatorTestCase<T>(config, "bltz $s5, 80") { ExpectedPC = branchAddress }];
        yield return [new MipsEmulatorTestCase<T>(config, "bgez $s1, 80") { ExpectedPC = branchAddress }];
        yield return [new MipsEmulatorTestCase<T>(config, "bgez $s0, 80") { ExpectedPC = branchAddress }];

        // Branch Compare: False
        yield return [new MipsEmulatorTestCase<T>(config, "blez $s1, 80") { ExpectedPC = noBranchAddress }];
        yield return [new MipsEmulatorTestCase<T>(config, "bgtz $s0, 80") { ExpectedPC = noBranchAddress }];
        yield return [new MipsEmulatorTestCase<T>(config, "bgtz $s5, 80") { ExpectedPC = noBranchAddress }];
        yield return [new MipsEmulatorTestCase<T>(config, "bltz $s1, 80") { ExpectedPC = noBranchAddress }];
        yield return [new MipsEmulatorTestCase<T>(config, "bltz $s0, 80") { ExpectedPC = noBranchAddress }];
        yield return [new MipsEmulatorTestCase<T>(config, "bgez $s5, 80") { ExpectedPC = noBranchAddress }];

        // Branch Likely
        if (config.Version is >= MipsVersion.MipsII and < MipsVersion.Mips_R6)
        {
            // If branch likely fails, it must skip the delay slot (PC + 8)
            var likelyFailAddress = config.DisableDelaySlots ? T.CreateTruncating(4) : T.CreateTruncating(8);

            // Branch Equality: True
            yield return [new MipsEmulatorTestCase<T>(config, "beql $t1, $t1, 80") { ExpectedPC = branchAddress }];
            yield return [new MipsEmulatorTestCase<T>(config, "bnel $t3, $t2, 80") { ExpectedPC = branchAddress }];

            // Branch Equality: False
            yield return [new MipsEmulatorTestCase<T>(config, "beql $t2, $t3, 80") { ExpectedPC = likelyFailAddress }];
            yield return [new MipsEmulatorTestCase<T>(config, "bnel $t1, $t1, 80") { ExpectedPC = likelyFailAddress }];

            // Branch Compare: True
            yield return [new MipsEmulatorTestCase<T>(config, "blezl $s0, 80") { ExpectedPC = branchAddress }];
            yield return [new MipsEmulatorTestCase<T>(config, "blezl $s5, 80") { ExpectedPC = branchAddress }];
            yield return [new MipsEmulatorTestCase<T>(config, "bgtzl $s1, 80") { ExpectedPC = branchAddress }];
            yield return [new MipsEmulatorTestCase<T>(config, "bltzl $s5, 80") { ExpectedPC = branchAddress }];
            yield return [new MipsEmulatorTestCase<T>(config, "bgezl $s1, 80") { ExpectedPC = branchAddress }];
            yield return [new MipsEmulatorTestCase<T>(config, "bgezl $s0, 80") { ExpectedPC = branchAddress }];

            // Branch Compare: False
            yield return [new MipsEmulatorTestCase<T>(config, "blezl $s1, 80") { ExpectedPC = likelyFailAddress }];
            yield return [new MipsEmulatorTestCase<T>(config, "bgtzl $s0, 80") { ExpectedPC = likelyFailAddress }];
            yield return [new MipsEmulatorTestCase<T>(config, "bgtzl $s5, 80") { ExpectedPC = likelyFailAddress }];
            yield return [new MipsEmulatorTestCase<T>(config, "bltzl $s1, 80") { ExpectedPC = likelyFailAddress }];
            yield return [new MipsEmulatorTestCase<T>(config, "bltzl $s0, 80") { ExpectedPC = likelyFailAddress }];
            yield return [new MipsEmulatorTestCase<T>(config, "bgezl $s5, 80") { ExpectedPC = likelyFailAddress }];
        }
    }

    private static IEnumerable<object[]> GetCompareInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Unsigned
        yield return [new MipsEmulatorTestCase<T>(config, "sltu $v0, $t2, $t3", T.One)];
        yield return [new MipsEmulatorTestCase<T>(config, "sltu $v0, $t3, $t2", T.Zero)];
        yield return [new MipsEmulatorTestCase<T>(config, "sltu $v0, $t1, $t1", T.Zero)];
        yield return [new MipsEmulatorTestCase<T>(config, "sltiu $v0, $t2, 30", T.One)];
        yield return [new MipsEmulatorTestCase<T>(config, "sltiu $v0, $t3, 20", T.Zero)];
        yield return [new MipsEmulatorTestCase<T>(config, "sltiu $v0, $t1, 10", T.Zero)];

        // Signed (without signs)
        yield return [new MipsEmulatorTestCase<T>(config, "slt $v0, $t2, $t3", T.One)];
        yield return [new MipsEmulatorTestCase<T>(config, "slt $v0, $t3, $t2", T.Zero)];
        yield return [new MipsEmulatorTestCase<T>(config, "slt $v0, $t1, $t1", T.Zero)];
        yield return [new MipsEmulatorTestCase<T>(config, "slti $v0, $t2, 30", T.One)];
        yield return [new MipsEmulatorTestCase<T>(config, "slti $v0, $t3, 20", T.Zero)];
        yield return [new MipsEmulatorTestCase<T>(config, "slti $v0, $t1, 10", T.Zero)];

        // Signed (with signs)
        yield return [new MipsEmulatorTestCase<T>(config, "slt $v0, $t7, $t6", T.One)];
        yield return [new MipsEmulatorTestCase<T>(config, "slt $v0, $t6, $t7", T.Zero)];
        yield return [new MipsEmulatorTestCase<T>(config, "slt $v0, $t5, $t5", T.Zero)];
        yield return [new MipsEmulatorTestCase<T>(config, "slti $v0, $t7, -20", T.One)];
        yield return [new MipsEmulatorTestCase<T>(config, "slti $v0, $t6, -30", T.Zero)];
        yield return [new MipsEmulatorTestCase<T>(config, "slti $v0, $t5, -10", T.Zero)];
    }

    private static IEnumerable<object[]> GetTrapInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        if (config.Version >= MipsVersion.MipsII)
        {
            // Equality
            yield return [new MipsEmulatorTestCase<T>(config, "teq $t2, $t3", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "teq $t1, $t1", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tne $t1, $t1", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tne $t3, $t2", MipsTrap.Trap)];

            // Unsigned
            yield return [new MipsEmulatorTestCase<T>(config, "tltu $t3, $t2", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tltu $t2, $t3", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tltu $t1, $t1", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tgeu $t2, $t3", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tgeu $t3, $t2", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tgeu $t1, $t1", MipsTrap.Trap)];

            // Signed (without signs)
            yield return [new MipsEmulatorTestCase<T>(config, "tlt $t3, $t2", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tlt $t2, $t3", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tlt $t1, $t1", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tge $t2, $t3", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tge $t3, $t2", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tge $t1, $t1", MipsTrap.Trap)];

            // Signed (with signs)
            yield return [new MipsEmulatorTestCase<T>(config, "tlt $t6, $t7", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tlt $t7, $t6", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tlt $t5, $t5", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tge $t7, $t6", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tge $t6, $t7", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tge $t5, $t5", MipsTrap.Trap)];
        }

        // Trap immediate
        if (config.Version is >= MipsVersion.MipsII and < MipsVersion.Mips_R6)
        {
            // Equality
            yield return [new MipsEmulatorTestCase<T>(config, "teqi $t2, 30", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "teqi $t1, 10", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tnei $t1, 10", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tnei $t3, 20", MipsTrap.Trap)];

            // Unsigned
            yield return [new MipsEmulatorTestCase<T>(config, "tltiu $t3, 20", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tltiu $t2, 30", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tltiu $t1, 10", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tgeiu $t2, 30", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tgeiu $t3, 20", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tgeiu $t1, 10", MipsTrap.Trap)];

            // Signed (without signs)
            yield return [new MipsEmulatorTestCase<T>(config, "tlti $t3, 20", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tlti $t2, 30", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tlti $t1, 10", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tgei $t2, 30", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tgei $t3, 20", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tgei $t1, 10", MipsTrap.Trap)];

            // Signed (with signs)
            yield return [new MipsEmulatorTestCase<T>(config, "tlti $t6, -30", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tlti $t7, -20", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tlti $t5, -10", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tgei $t7, -20", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T>(config, "tgei $t6, -30", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T>(config, "tgei $t5, -10", MipsTrap.Trap)];
        }
    }

    private static IEnumerable<object[]> GetUncategorizedInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // lui
        yield return [new MipsEmulatorTestCase<T>(config, "lui $v0, 0x1234", T.CreateTruncating(0x12340000))];

        if (config.Version is < MipsVersion.Mips_R6)
        {
            // Move from/to high and low registers
            yield return [new MipsEmulatorTestCase<T>(config, "mtlo $k0", (T.CreateTruncating(0x1234), T.CreateTruncating(K0)))];
            yield return [new MipsEmulatorTestCase<T>(config, "mthi $k1", (T.CreateTruncating(K1), T.CreateTruncating(0x5678)))];
            yield return [new MipsEmulatorTestCase<T>(config, "mflo $v0", T.CreateTruncating(0x5678))];
            yield return [new MipsEmulatorTestCase<T>(config, "mfhi $v0", T.CreateTruncating(0x1234))];
        }

        if (config.Version is >= MipsVersion.MipsIV)
        {
            // movz/movn
            yield return [new MipsEmulatorTestCase<T>(config, "movz $k0, $k1, $t0", MipsGpRegister.Kernel0, T.CreateTruncating(K1))];
            yield return [new MipsEmulatorTestCase<T>(config, "movz $k0, $k1, $t1", MipsGpRegister.Zero)];
            yield return [new MipsEmulatorTestCase<T>(config, "movn $k0, $k1, $t0", MipsGpRegister.Zero)];
            yield return [new MipsEmulatorTestCase<T>(config, "movn $k0, $k1, $t1", MipsGpRegister.Kernel0, T.CreateTruncating(K1))];
        }
    }

    private static IEnumerable<object[]> GetSystemInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Syscall and break
        yield return [new MipsEmulatorTestCase<T>(config, "syscall", MipsTrap.Syscall)];
        yield return [new MipsEmulatorTestCase<T>(config, "break", MipsTrap.Breakpoint)];

        // TODO: JIT CoProcessor0 instructions
        if (config.ExecutionMode is ExecutionMode.JustInTime)
            yield break;

        if (config.Version is >= MipsVersion.MipsII)
        {
            // Exception Return
            yield return [new MipsEmulatorTestCase<T>(config, "eret", MipsTrap.ReservedInstruction)];
            yield return [new MipsEmulatorTestCase<T>(config, "eret", MipsSideEffect.WriteCoProc0)
            {
                Status = new StatusRegister
                {
                    ExceptionLevel = true
                }
            }];
        }

        if (config.Version is >= MipsVersion.Mips_R2)
        {
            // Enable Interrupts
            yield return [new MipsEmulatorTestCase<T>(config, "ei", MipsTrap.ReservedInstruction)];
            yield return [new MipsEmulatorTestCase<T>(config, "ei", MipsSideEffect.WriteCoProc0)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
            yield return [new MipsEmulatorTestCase<T>(config, "ei $v0", MipsGpRegister.ReturnValue0)
            {
                ExpectedSideEffect = MipsSideEffect.WriteCoProc0,
                PrivilegeMode = PrivilegeMode.Kernel
            }];

            // Disable Interrupts
            yield return [new MipsEmulatorTestCase<T>(config, "di", MipsTrap.ReservedInstruction)];
            yield return [new MipsEmulatorTestCase<T>(config, "di", MipsSideEffect.WriteCoProc0)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
            yield return [new MipsEmulatorTestCase<T>(config, "di $v1", MipsGpRegister.ReturnValue1)
            {
                ExpectedSideEffect = MipsSideEffect.WriteCoProc0,
                PrivilegeMode = PrivilegeMode.Kernel
            }];
        }
    }

    private static IEnumerable<object[]> GetCoProcMoveInstructionTest<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // CoProcessor 1
        yield return [new MipsEmulatorTestCase<T>(config, "mtc1 $t2, $f16", MipsFloatRegister.F16, 20)];
        yield return [new MipsEmulatorTestCase<T>(config, "mfc1 $v0, $f0", MipsGpRegister.ReturnValue0, T.CreateTruncating(2))];
    }

    private static IEnumerable<object[]> GetFloatArithmeticInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Single
        yield return [new MipsEmulatorTestCase<T>(config, "add.S $f16, $f8, $f9", MipsFloatRegister.F16, 10.5f + 2.5f)];
        yield return [new MipsEmulatorTestCase<T>(config, "sub.S $f16, $f8, $f9", MipsFloatRegister.F16, 10.5f - 2.5f)];
        yield return [new MipsEmulatorTestCase<T>(config, "mul.S $f16, $f8, $f9", MipsFloatRegister.F16, 10.5f * 2.5f)];
        yield return [new MipsEmulatorTestCase<T>(config, "div.S $f16, $f8, $f9", MipsFloatRegister.F16, 10.5f / 2.5f)];
        yield return [new MipsEmulatorTestCase<T>(config, "abs.S $f16, $f7", MipsFloatRegister.F16, 2f)];
        yield return [new MipsEmulatorTestCase<T>(config, "neg.S $f16, $f5", MipsFloatRegister.F16, -2f)];

        // Double
        yield return [new MipsEmulatorTestCase<T>(config, "add.D $f16, $f12, $f14", MipsFloatRegister.F16, 2d + 0.5d)];
        yield return [new MipsEmulatorTestCase<T>(config, "sub.D $f16, $f12, $f14", MipsFloatRegister.F16, 2d - 0.5d)];
        yield return [new MipsEmulatorTestCase<T>(config, "mul.D $f16, $f12, $f14", MipsFloatRegister.F16, 2d * 0.5d)];
        yield return [new MipsEmulatorTestCase<T>(config, "div.D $f16, $f12, $f14", MipsFloatRegister.F16, 2d / 0.5d)];
        yield return [new MipsEmulatorTestCase<T>(config, "abs.D $f16, $f16", MipsFloatRegister.F16, 2d)];
        yield return [new MipsEmulatorTestCase<T>(config, "neg.D $f16, $f12", MipsFloatRegister.F16, -2d)];

        if (config.Version is >= MipsVersion.MipsII)
        {
            yield return [new MipsEmulatorTestCase<T>(config, "sqrt.S $f16, $f8", MipsFloatRegister.F16, MathF.Sqrt(10.5f))];
            yield return [new MipsEmulatorTestCase<T>(config, "sqrt.D $f16, $f12", MipsFloatRegister.F16, Math.Sqrt(2d))];
        }

        if (config.Version is >= MipsVersion.MipsIV)
        {
            yield return [new MipsEmulatorTestCase<T>(config, "recip.S $f16, $f9", MipsFloatRegister.F16, float.ReciprocalEstimate(2.5f))];
            yield return [new MipsEmulatorTestCase<T>(config, "recip.D $f16, $f12", MipsFloatRegister.F16, double.ReciprocalEstimate(2d))];
        }

        if (config.Version is >= MipsVersion.Mips_R2)
        {
            yield return [new MipsEmulatorTestCase<T>(config, "rsqrt.S $f16, $f9", MipsFloatRegister.F16, float.ReciprocalSqrtEstimate(2.5f))];
            yield return [new MipsEmulatorTestCase<T>(config, "rsqrt.D $f16, $f12", MipsFloatRegister.F16, double.ReciprocalSqrtEstimate(2d))];
        }
    }

    private static IEnumerable<object[]> GetFloatConvertInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // From Single
        yield return [new MipsEmulatorTestCase<T>(config, "cvt.D.S $f16, $f5", MipsFloatRegister.F16, 2d)];     // To Double
        yield return [new MipsEmulatorTestCase<T>(config, "cvt.W.S $f16, $f5", MipsFloatRegister.F16, 2)];      // To Word

        // From Double
        yield return [new MipsEmulatorTestCase<T>(config, "cvt.S.D $f16, $f12", MipsFloatRegister.F16, 2f)];    // To Single
        yield return [new MipsEmulatorTestCase<T>(config, "cvt.W.D $f16, $f12", MipsFloatRegister.F16, 2)];     // To Word

        // From Word 
        yield return [new MipsEmulatorTestCase<T>(config, "cvt.S.W $f16, $f0", MipsFloatRegister.F16, 2f)];     // To Single
        yield return [new MipsEmulatorTestCase<T>(config, "cvt.D.W $f16, $f0", MipsFloatRegister.F16, 2d)];     // To Double

        if (config.Version is >= MipsVersion.MipsIII && config.Version.Is64Bit())
        {
            // To long
            yield return [new MipsEmulatorTestCase<T>(config, "cvt.L.S $f16, $f5", MipsFloatRegister.F16, 2L)];     // From Single
            yield return [new MipsEmulatorTestCase<T>(config, "cvt.L.D $f16, $f12", MipsFloatRegister.F16, 2L)];    // From Double

            // From Long
            yield return [new MipsEmulatorTestCase<T>(config, "cvt.S.L $f16, $f0", MipsFloatRegister.F16, 2f)];     // To Single
            yield return [new MipsEmulatorTestCase<T>(config, "cvt.D.L $f16, $f0", MipsFloatRegister.F16, 2d)];     // To Double
        }
    }

    private static IEnumerable<object[]> GetFloatMoveInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        yield return [new MipsEmulatorTestCase<T>(config, "mov.S $f16, $f10", MipsFloatRegister.F16, 1.25f)];
        yield return [new MipsEmulatorTestCase<T>(config, "mov.D $f16, $f18", MipsFloatRegister.F16, Math.PI)];
    }

    private static IEnumerable<object[]> GetFloatRoundInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        if (config.Version is >= MipsVersion.MipsII)
        {
            // Round
            yield return [new MipsEmulatorTestCase<T>(config, "round.W.S $f16, $f10", MipsFloatRegister.F16, 1)];
            yield return [new MipsEmulatorTestCase<T>(config, "round.W.D $f16, $f18", MipsFloatRegister.F16, 3)];

            // Ceiling
            yield return [new MipsEmulatorTestCase<T>(config, "ceil.W.S $f16, $f10", MipsFloatRegister.F16, 2)];
            yield return [new MipsEmulatorTestCase<T>(config, "ceil.W.D $f16, $f18", MipsFloatRegister.F16, 4)];

            // Floor
            yield return [new MipsEmulatorTestCase<T>(config, "floor.W.S $f16, $f10", MipsFloatRegister.F16, 1)];
            yield return [new MipsEmulatorTestCase<T>(config, "floor.W.D $f16, $f18", MipsFloatRegister.F16, 3)];
        }

        if (config.Version is >= MipsVersion.MipsIII && config.Version.Is64Bit())
        {
            // Long
            yield return [new MipsEmulatorTestCase<T>(config, "round.L.S $f16, $f10", MipsFloatRegister.F16, 1L)];
            yield return [new MipsEmulatorTestCase<T>(config, "round.L.D $f16, $f18", MipsFloatRegister.F16, 3L)];
            yield return [new MipsEmulatorTestCase<T>(config, "ceil.L.S $f16, $f10", MipsFloatRegister.F16, 2L)];
            yield return [new MipsEmulatorTestCase<T>(config, "ceil.L.D $f16, $f18", MipsFloatRegister.F16, 4L)];
            yield return [new MipsEmulatorTestCase<T>(config, "floor.L.S $f16, $f10", MipsFloatRegister.F16, 1L)];
            yield return [new MipsEmulatorTestCase<T>(config, "floor.L.D $f16, $f18", MipsFloatRegister.F16, 3L)];
        }
    }

    private unsafe static (T, T) Split<T, TLong>(TLong value)
        where T : unmanaged, IBinaryInteger<T>
        where TLong : unmanaged, IBinaryInteger<TLong>
    {
        var size = sizeof(TLong) * 4; // Half the size of TLong in bits
        var mask = (TLong.One << (sizeof(TLong) * 4)) - TLong.One;
        var hi = T.CreateTruncating(value >> size);
        var low = T.CreateTruncating(value & mask);
        return (hi, low);
    }
}
