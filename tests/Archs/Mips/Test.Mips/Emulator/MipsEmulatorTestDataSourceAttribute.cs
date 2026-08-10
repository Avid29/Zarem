// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Test.Archs.Emulator;
using Zarem.Emulator.Config.Enums;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Emulator.Machine.Registers.CoProcessor0;
using Zarem.Mips.Models.Instructions.Enums.Registers;
using Zarem.Mips.Models.Versioning;
using Zarem.Mips.Models.Versioning.Enums;

namespace Test.Mips.Emulator;

[AttributeUsage(AttributeTargets.Method)]
public class MipsEmulatorTestDataSourceAttribute : EmulatorTestDataSourceAttribute<MipsEmulatorTestCase, MipsEmulatorConfig>
{
    public const uint K0 = MipsEmulatorTests.K0;
    public const uint K1 = MipsEmulatorTests.K1;

    private readonly MipsVersionInfo _versionInfo;
    private readonly ExecutionMode _mode;

    public MipsEmulatorTestDataSourceAttribute(string versionStr, ExecutionMode mode)
    {
        _versionInfo = MipsVersionInfo.Parse(versionStr);
        _mode = mode;
    }

    public override IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        var config = new MipsEmulatorConfig(_versionInfo)
        {
            ExecutionMode = _mode,
        };

        bool is64Bit = _versionInfo.Is64Bit;
        bool has64BitFloats = _versionInfo.Generation is >= MipsGeneration.MipsIII;

        return (is64Bit, has64BitFloats) switch
        {
            (true, true) => GetVersionTests<ulong, ulong>(config),
            (false, true) => GetVersionTests<uint, ulong>(config),
            (false, false) => GetVersionTests<uint, uint>(config),
            _ => throw new NotSupportedException($"Unsupported configuration: {config.VersionInfo}"),
        };
    }

    public override string? GetDisplayName(MethodInfo methodInfo, MipsEmulatorTestCase[] data)
    {
        var str = base.GetDisplayName(methodInfo, data);

        var test = data[0];
        var config = test.Config;
        if (config?.DisableDelaySlots is true)
        {
            str += " (Delay Slots Disabled)";
        }
        if (config?.VersionInfo.Generation >= MipsGeneration.MipsIII && test.Status.FloatingPoint64BitMode is false)
        {
            str += " (Legacy Paired Floating-Points)";
        }

        return str;
    }

    private static IEnumerable<object[]> GetVersionTests<T, TFloat>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        var noDelayConfig = new MipsEmulatorConfig(config.VersionInfo)
        {
            TrapHost = config.TrapHost,
            ExecutionMode = config.ExecutionMode,
            DisableDelaySlots = true
        };

        return GetArithmeticInstructionTests<T, TFloat>(config)
            .Concat(GetLogicalInstructionTests<T, TFloat>(config))
            .Concat(GetMemoryInstructionTests<T, TFloat>(config))
            .Concat(GetJumpBranchInstructionTests<T, TFloat>(config))
            .Concat(GetJumpBranchInstructionTests<T, TFloat>(noDelayConfig))
            .Concat(GetCompareInstructionTests<T, TFloat>(config))
            .Concat(GetTrapInstructionTests<T, TFloat>(config))
            .Concat(GetUncategorizedInstructionTests<T, TFloat>(config))
            .Concat(GetSystemInstructionTests<T, TFloat>(config))
            .Concat(GetCoProcMoveInstructionTest<T, TFloat>(config))
            .Concat(GetFloatInstructionTests<T, TFloat>(config))
            .Concat(GetFloatInstructionTests<T, TFloat>(config, true));
    }

    private static IEnumerable<object[]> GetArithmeticInstructionTests<T, TFloat>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        // Unsigned
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "addu $v0, $t2, $t1", T.CreateTruncating(30))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "addiu $v0, $t2, 10", T.CreateTruncating(30))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "subu $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "multu $t3, $t2", Split<T, ulong>(30 * 20))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "divu $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];

        // Signed (without signs)
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "add $v0, $t2, $t1", T.CreateTruncating(30))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "addi $v0, $t2, 10", T.CreateTruncating(30))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sub $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mult $t3, $t2", Split<T, long>(30 * 20))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "div $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sra $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "srav $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];

        // Signed (with signs)
        unchecked
        {
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "add $v0, $t3, $t5", T.CreateTruncating(30 + (-10)))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "add $v0, $t5, $t3", T.CreateTruncating((-10) + 30))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "addi $v0, $t3, -10", T.CreateTruncating(30 + (-10)))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "addi $v0, $t7, 10", T.CreateTruncating(-30 + 10))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sub $v0, $t2, $t5", T.CreateTruncating(20 - (-10)))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sub $v0, $t5, $t2", T.CreateTruncating(-10 - 20))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mult $t3, $t6", Split<T, long>(30 * -20))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "div $t3, $t6", (T.CreateTruncating(30 % -20), T.CreateTruncating(30 / -20)))];
        }

        // Overflowing
        unchecked
        {
            // Unsigned (should overflow without trapping)
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "addu $v0, $a2, $s1", T.CreateTruncating(uint.MaxValue + 1))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "addiu $v0, $a2, 1", T.CreateTruncating(uint.MaxValue + 1))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "subu $v0, $a3, $s1", T.CreateTruncating(uint.MinValue - 1))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "multu $a2, $a2", Split<T, ulong>((ulong)uint.MaxValue * uint.MaxValue))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "divu $a2, $a2", (T.CreateTruncating(uint.MaxValue % uint.MaxValue), T.CreateTruncating(uint.MaxValue / uint.MaxValue)))];

            // Note:
            // "mul" does not trap on overflow. We expect the low 32 bits of the result to be written back, and the high 32 bits to be discarded.
            // "mult" also does not trap on overflow, but instead writes the full 64-bit result into the high and low registers.
            // "div" does not trap on overflow either. The behavior is undefined if the quotient is too large to fit in 32 bits.
            // In practice, we will just take the low 32 bits of the quotient and discard the high 32 bits, and write the remainder to the high register.

            // Signed (without signs)
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "add $v0, $a0, $s1", MipsTrap.ArithmeticOverflow)];                  // max + 1
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "addi $v0, $a0, 1", MipsTrap.ArithmeticOverflow)];                   // max + 1
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sub $v0, $a1, $s1", MipsTrap.ArithmeticOverflow)];                  // min - 1
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mult $a0, $a0", Split<T, long>((long)int.MaxValue * int.MaxValue))];     // max * max
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "div $a0, $a0", (T.CreateTruncating((uint)(int.MaxValue % int.MaxValue)), T.CreateTruncating((uint)(int.MaxValue / int.MaxValue))))];

            // Signed (with signs)
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "add $v0, $a1, $s5", MipsTrap.ArithmeticOverflow)];     // min + (-1)
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "addi $v0, $a1, -1", MipsTrap.ArithmeticOverflow)];     // min + (-1)
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sub $v0, $a0, $s5", MipsTrap.ArithmeticOverflow)];     // max - (-1)
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mult $a1, $a1", Split<T, long>((long)int.MinValue * int.MinValue))];    // min * min
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "div $a1, $a1", (T.CreateTruncating((uint)(int.MinValue % int.MinValue)), T.CreateTruncating((uint)(int.MinValue / int.MinValue))))];
        }

        // Division by zero. Undefined behavior, but NOT a trap! (Shouldn't crash the emulator either)
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "divu $t3, $zero", MipsTrap.None)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "div $t3, $zero", MipsTrap.None)];

        if (config.VersionInfo.Generation is >= MipsGeneration.R1)
        {
            // GPR Multiply
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mul $v0, $t3, $t2", T.CreateTruncating(30 * 20))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mul $v0, $t3, $t6", T.CreateTruncating(unchecked(30 * -20)))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mul $v0, $a0, $a0", T.CreateTruncating(unchecked(int.MaxValue * int.MaxValue)))];     // max * max
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mul $v0, $a1, $a1", T.CreateTruncating(unchecked(int.MinValue * int.MinValue)))];     // min * min
        }

        if (config.VersionInfo.Generation is >= MipsGeneration.R1 and < MipsGeneration.R6)
        {
            // Multiply and Add/Subtract
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "maddu $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 + (30 * 20))))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "madd $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 + (30 * 20))))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "msubu $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 - (30 * 20))))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "msub $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 - (30 * 20))))];
        }

        // Not arithmetic, but fixed width
        if (config.VersionInfo.Generation is >= MipsGeneration.R1)
        {
            // Niche bit-manipulation
            // TODO: ext, ins, seb, seh, wsbh, wshd
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "clz $v0, $k0", T.CreateTruncating(BitOperations.LeadingZeroCount(K0)))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "clo $v0, $k0", T.CreateTruncating(BitOperations.LeadingZeroCount(~K0)))];
        }

        if (config.VersionInfo.Generation is >= MipsGeneration.MipsIII && config.VersionInfo.Is64Bit)
        {
            // Unsigned
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "daddu $v0, $t2, $t1", T.CreateTruncating(30))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "daddiu $v0, $t2, 10", T.CreateTruncating(30))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dsubu $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dmultu $t3, $t2", Split<T, UInt128>(30 * 20))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "ddivu $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];

            // Signed (without signs)
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dadd $v0, $t2, $t1", T.CreateTruncating(30))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "daddi $v0, $t2, 10", T.CreateTruncating(30))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dsub $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dmult $t3, $t2", Split<T, UInt128>(30 * 20))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "ddiv $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dsra $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dsrav $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];

            // Signed (with signs)
            unchecked
            {
                yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dadd $v0, $t3, $t5", T.CreateTruncating(30 + (-10)))];
                yield return [new MipsEmulatorTestCase<T, TFloat>(config, "daddi $v0, $t3, -10", T.CreateTruncating(30 + (-10)))];
                yield return [new MipsEmulatorTestCase<T, TFloat>(config, "daddi $v0, $t5, 30", T.CreateTruncating((-10) + 30))];
                yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dsub $v0, $t2, $t5", T.CreateTruncating(20 - (-10)))];
                yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dsub $v0, $t5, $t2", T.CreateTruncating((-10) - 20))];
                yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dmult $t3, $t6", Split<T, UInt128>((UInt128)(30 * -20)))];
                yield return [new MipsEmulatorTestCase<T, TFloat>(config, "ddiv $t3, $t6", (T.CreateTruncating(30 % -20), T.CreateTruncating(30 / -20)))];
            }
        }
    }

    private static IEnumerable<object[]> GetLogicalInstructionTests<T, TFloat>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "and $v0, $k0, $k1", T.CreateTruncating(K0 & K1))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "andi $v0, $k0, 0xd16", T.CreateTruncating(K0 & K1))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "or $v0, $k0, $k1", T.CreateTruncating(K0 | K1))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "ori $v0, $k0, 0xd16", T.CreateTruncating(K0 | K1))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "xor $v0, $k0, $k1", T.CreateTruncating(K0 ^ K1))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "xori $v0, $k0, 0xd16", T.CreateTruncating(K0 ^ K1))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "nor $v0, $k0, $k1", ~T.CreateTruncating(K0 | K1))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sll $v0, $t8, 4", T.CreateTruncating(101 << 4))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "srl $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sllv $v0, $t8, $s4", T.CreateTruncating(101 << 4))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "srlv $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];

        if (config.VersionInfo.Generation is >= MipsGeneration.MipsIII && config.VersionInfo.Is64Bit)
        {
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dsll $v0, $t8, 4", T.CreateTruncating(101 << 4))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dsrl $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dsllv $v0, $t8, $s4", T.CreateTruncating(101 << 4))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "dsrlv $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];
        }
    }

    private static IEnumerable<object[]> GetMemoryInstructionTests<T, TFloat>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        // Load
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "lb $v0, 0x1000($zero)", T.CreateTruncating(0x12))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "lh $v0, 0x1000($zero)", T.CreateTruncating(0x1234))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "lw $v0, 0x1000($zero)", T.CreateTruncating(0x1234_5678))];

        // TODO: Load unsigned/signed with sign

        // Store
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sb $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0xef, 0x34, 0x56, 0x78]))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sh $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0xcd, 0xef, 0x56, 0x78]))];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sw $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0x89, 0xab, 0xcd, 0xef]))];

        // Protected load/store
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "lw $v0, 0x1000($gp)", MipsTrap.AddressErrorLoad)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sw $at, 0x1000($gp)", MipsTrap.AddressErrorStore)];

        // TODO: TLB Miss load/store
    }

    private static IEnumerable<object[]> GetJumpBranchInstructionTests<T, TFloat>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        var startAddress = T.Zero;
        var linkAddress = T.CreateTruncating(config.DisableDelaySlots ? 4 : 8) + startAddress;
        var noBranchAddress = T.CreateTruncating(config.ExecutionMode is ExecutionMode.JustInTime && !config.DisableDelaySlots ? 8 : 4) + startAddress;
        var branchAddress = T.CreateTruncating(84) + startAddress;

        // Jump
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "j 1000") { ExpectedPC = T.CreateTruncating(1000) }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "jr $t4") { ExpectedPC = T.CreateTruncating(40) }];

        // Jump And Link
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "jal 1000", MipsGpRegister.ReturnAddress, linkAddress) { ExpectedPC = T.CreateTruncating(1000) }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "jalr $t4", MipsGpRegister.ReturnAddress, linkAddress) { ExpectedPC = T.CreateTruncating(40) }];

        // Branch Equality: True
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "beq $t1, $t1, 80") { ExpectedPC = branchAddress }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bne $t3, $t2, 80") { ExpectedPC = branchAddress }];

        // Branch Equality: False
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "beq $t2, $t3, 80") { ExpectedPC = noBranchAddress }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bne $t1, $t1, 80") { ExpectedPC = noBranchAddress }];

        // Branch Compare: True
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "blez $s0, 80") { ExpectedPC = branchAddress }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "blez $s5, 80") { ExpectedPC = branchAddress }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bgtz $s1, 80") { ExpectedPC = branchAddress }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bltz $s5, 80") { ExpectedPC = branchAddress }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bgez $s1, 80") { ExpectedPC = branchAddress }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bgez $s0, 80") { ExpectedPC = branchAddress }];

        // Branch Compare: False
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "blez $s1, 80") { ExpectedPC = noBranchAddress }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bgtz $s0, 80") { ExpectedPC = noBranchAddress }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bgtz $s5, 80") { ExpectedPC = noBranchAddress }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bltz $s1, 80") { ExpectedPC = noBranchAddress }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bltz $s0, 80") { ExpectedPC = noBranchAddress }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bgez $s5, 80") { ExpectedPC = noBranchAddress }];

        // Branch Likely
        if (config.VersionInfo.Generation is >= MipsGeneration.MipsII and < MipsGeneration.R6)
        {
            // If branch likely fails, it must skip the delay slot (PC + 8)
            var likelyFailAddress = T.CreateTruncating(config.DisableDelaySlots ? 4 : 8) + startAddress;

            // Branch Equality: True
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "beql $t1, $t1, 80") { ExpectedPC = branchAddress }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bnel $t3, $t2, 80") { ExpectedPC = branchAddress }];

            // Branch Equality: False
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "beql $t2, $t3, 80") { ExpectedPC = likelyFailAddress }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bnel $t1, $t1, 80") { ExpectedPC = likelyFailAddress }];

            // Branch Compare: True
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "blezl $s0, 80") { ExpectedPC = branchAddress }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "blezl $s5, 80") { ExpectedPC = branchAddress }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bgtzl $s1, 80") { ExpectedPC = branchAddress }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bltzl $s5, 80") { ExpectedPC = branchAddress }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bgezl $s1, 80") { ExpectedPC = branchAddress }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bgezl $s0, 80") { ExpectedPC = branchAddress }];

            // Branch Compare: False
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "blezl $s1, 80") { ExpectedPC = likelyFailAddress }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bgtzl $s0, 80") { ExpectedPC = likelyFailAddress }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bgtzl $s5, 80") { ExpectedPC = likelyFailAddress }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bltzl $s1, 80") { ExpectedPC = likelyFailAddress }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bltzl $s0, 80") { ExpectedPC = likelyFailAddress }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "bgezl $s5, 80") { ExpectedPC = likelyFailAddress }];
        }
    }

    private static IEnumerable<object[]> GetCompareInstructionTests<T, TFloat>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        // Unsigned
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sltu $v0, $t2, $t3", T.One)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sltu $v0, $t3, $t2", T.Zero)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sltu $v0, $t1, $t1", T.Zero)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sltiu $v0, $t2, 30", T.One)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sltiu $v0, $t3, 20", T.Zero)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sltiu $v0, $t1, 10", T.Zero)];

        // Signed (without signs)
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "slt $v0, $t2, $t3", T.One)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "slt $v0, $t3, $t2", T.Zero)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "slt $v0, $t1, $t1", T.Zero)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "slti $v0, $t2, 30", T.One)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "slti $v0, $t3, 20", T.Zero)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "slti $v0, $t1, 10", T.Zero)];

        // Signed (with signs)
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "slt $v0, $t7, $t6", T.One)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "slt $v0, $t6, $t7", T.Zero)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "slt $v0, $t5, $t5", T.Zero)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "slti $v0, $t7, -20", T.One)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "slti $v0, $t6, -30", T.Zero)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "slti $v0, $t5, -10", T.Zero)];
    }

    private static IEnumerable<object[]> GetTrapInstructionTests<T, TFloat>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        if (config.VersionInfo.Generation >= MipsGeneration.MipsII)
        {
            // Equality
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "teq $t2, $t3", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "teq $t1, $t1", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tne $t1, $t1", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tne $t3, $t2", MipsTrap.Trap)];

            // Unsigned
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tltu $t3, $t2", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tltu $t2, $t3", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tltu $t1, $t1", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tgeu $t2, $t3", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tgeu $t3, $t2", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tgeu $t1, $t1", MipsTrap.Trap)];

            // Signed (without signs)
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tlt $t3, $t2", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tlt $t2, $t3", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tlt $t1, $t1", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tge $t2, $t3", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tge $t3, $t2", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tge $t1, $t1", MipsTrap.Trap)];

            // Signed (with signs)
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tlt $t6, $t7", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tlt $t7, $t6", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tlt $t5, $t5", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tge $t7, $t6", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tge $t6, $t7", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tge $t5, $t5", MipsTrap.Trap)];
        }

        // Trap immediate
        if (config.VersionInfo.Generation is >= MipsGeneration.MipsII and < MipsGeneration.R6)
        {
            // Equality
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "teqi $t2, 30", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "teqi $t1, 10", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tnei $t1, 10", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tnei $t3, 20", MipsTrap.Trap)];

            // Unsigned
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tltiu $t3, 20", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tltiu $t2, 30", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tltiu $t1, 10", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tgeiu $t2, 30", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tgeiu $t3, 20", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tgeiu $t1, 10", MipsTrap.Trap)];

            // Signed (without signs)
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tlti $t3, 20", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tlti $t2, 30", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tlti $t1, 10", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tgei $t2, 30", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tgei $t3, 20", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tgei $t1, 10", MipsTrap.Trap)];

            // Signed (with signs)
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tlti $t6, -30", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tlti $t7, -20", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tlti $t5, -10", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tgei $t7, -20", MipsTrap.None)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tgei $t6, -30", MipsTrap.Trap)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "tgei $t5, -10", MipsTrap.Trap)];
        }
    }

    private static IEnumerable<object[]> GetUncategorizedInstructionTests<T, TFloat>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        // lui
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "lui $v0, 0x1234", T.CreateTruncating(0x12340000))];

        if (config.VersionInfo.Generation is < MipsGeneration.R6)
        {
            // Move from/to high and low registers
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mtlo $k0", (T.CreateTruncating(0x1234), T.CreateTruncating(K0)))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mthi $k1", (T.CreateTruncating(K1), T.CreateTruncating(0x5678)))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mflo $v0", T.CreateTruncating(0x5678))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mfhi $v0", T.CreateTruncating(0x1234))];
        }

        if (config.VersionInfo.Generation is >= MipsGeneration.MipsIV)
        {
            // movz/movn
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "movz $k0, $k1, $t0", MipsGpRegister.Kernel0, T.CreateTruncating(K1))];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "movz $k0, $k1, $t1", MipsGpRegister.Zero)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "movn $k0, $k1, $t0", MipsGpRegister.Zero)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "movn $k0, $k1, $t1", MipsGpRegister.Kernel0, T.CreateTruncating(K1))];
        }
    }

    private static IEnumerable<object[]> GetSystemInstructionTests<T, TFloat>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        // Syscall and break
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "syscall", MipsTrap.Syscall)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "break", MipsTrap.Breakpoint)];

        // TODO: JIT CoProcessor0 instructions
        if (config.ExecutionMode is ExecutionMode.JustInTime)
            yield break;

        if (config.VersionInfo.Generation is >= MipsGeneration.MipsII)
        {
            // Exception Return
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "eret", MipsTrap.ReservedInstruction)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "eret", MipsSideEffect.WriteCoProc0)
            {
                Status = new StatusRegister
                {
                    ExceptionLevel = true
                }
            }];
        }

        if (config.VersionInfo.Generation is >= MipsGeneration.R2)
        {
            // Enable Interrupts
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "ei", MipsTrap.ReservedInstruction)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "ei", MipsSideEffect.WriteCoProc0)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "ei $v0", MipsGpRegister.ReturnValue0)
            {
                ExpectedSideEffect = MipsSideEffect.WriteCoProc0,
                PrivilegeMode = PrivilegeMode.Kernel
            }];

            // Disable Interrupts
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "di", MipsTrap.ReservedInstruction)];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "di", MipsSideEffect.WriteCoProc0)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "di $v1", MipsGpRegister.ReturnValue1)
            {
                ExpectedSideEffect = MipsSideEffect.WriteCoProc0,
                PrivilegeMode = PrivilegeMode.Kernel
            }];
        }
    }

    private static IEnumerable<object[]> GetCoProcMoveInstructionTest<T, TFloat>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        // CoProcessor 1
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mtc1 $t2, $f16", MipsFloatRegister.F16, 20)];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mfc1 $v0, $f0", MipsGpRegister.ReturnValue0, T.CreateTruncating(2))];
    }

    private static IEnumerable<object[]> GetFloatInstructionTests<T, TFloat>(MipsEmulatorConfig config, bool legacy = false)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        // Legacy floating point instructions are only available on MIPS III and later,
        // so skip these tests if the configuration is legacy and the generation is less than MIPS III.
        if (config.VersionInfo.Generation is < MipsGeneration.MipsIII && legacy)
            return [];

        return GetFloatArithmeticInstructionTests<T, TFloat>(config, legacy)
            .Concat(GetFloatConvertInstructionTests<T, TFloat>(config, legacy))
            .Concat(GetFloatMoveInstructionTests<T, TFloat>(config, legacy))
            .Concat(GetFloatRoundInstructionTests<T, TFloat>(config, legacy));
    }

    private static IEnumerable<object[]> GetFloatArithmeticInstructionTests<T, TFloat>(MipsEmulatorConfig config, bool legacy)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        // Single
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "add.S $f16, $f8, $f9", MipsFloatRegister.F16, 10.5f + 2.5f) { UseLegacyPairedFloatRegisters = legacy }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sub.S $f16, $f8, $f9", MipsFloatRegister.F16, 10.5f - 2.5f) { UseLegacyPairedFloatRegisters = legacy }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mul.S $f16, $f8, $f9", MipsFloatRegister.F16, 10.5f * 2.5f) { UseLegacyPairedFloatRegisters = legacy }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "div.S $f16, $f8, $f9", MipsFloatRegister.F16, 10.5f / 2.5f) { UseLegacyPairedFloatRegisters = legacy }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "abs.S $f16, $f7", MipsFloatRegister.F16, 2f) { UseLegacyPairedFloatRegisters = legacy }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "neg.S $f16, $f5", MipsFloatRegister.F16, -2f) { UseLegacyPairedFloatRegisters = legacy }];

        // Double
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "add.D $f16, $f12, $f14", MipsFloatRegister.F16, 2d + 0.5d) { UseLegacyPairedFloatRegisters = legacy }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sub.D $f16, $f12, $f14", MipsFloatRegister.F16, 2d - 0.5d) { UseLegacyPairedFloatRegisters = legacy }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mul.D $f16, $f12, $f14", MipsFloatRegister.F16, 2d * 0.5d) { UseLegacyPairedFloatRegisters = legacy }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "div.D $f16, $f12, $f14", MipsFloatRegister.F16, 2d / 0.5d) { UseLegacyPairedFloatRegisters = legacy }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "abs.D $f16, $f16", MipsFloatRegister.F16, 2d) { UseLegacyPairedFloatRegisters = legacy }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "neg.D $f16, $f12", MipsFloatRegister.F16, -2d) { UseLegacyPairedFloatRegisters = legacy }];

        if (config.VersionInfo.Generation is >= MipsGeneration.MipsII)
        {
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sqrt.S $f16, $f8", MipsFloatRegister.F16, MathF.Sqrt(10.5f)) { UseLegacyPairedFloatRegisters = legacy }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "sqrt.D $f16, $f12", MipsFloatRegister.F16, Math.Sqrt(2d)) { UseLegacyPairedFloatRegisters = legacy }];
        }

        if (config.VersionInfo.Generation is >= MipsGeneration.MipsIV)
        {
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "recip.S $f16, $f9", MipsFloatRegister.F16, float.ReciprocalEstimate(2.5f)) { UseLegacyPairedFloatRegisters = legacy }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "recip.D $f16, $f12", MipsFloatRegister.F16, double.ReciprocalEstimate(2d)) { UseLegacyPairedFloatRegisters = legacy }];
        }

        if (config.VersionInfo.Generation is >= MipsGeneration.R2)
        {
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "rsqrt.S $f16, $f9", MipsFloatRegister.F16, float.ReciprocalSqrtEstimate(2.5f)) { UseLegacyPairedFloatRegisters = legacy }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "rsqrt.D $f16, $f12", MipsFloatRegister.F16, double.ReciprocalSqrtEstimate(2d)) { UseLegacyPairedFloatRegisters = legacy }];
        }
    }

    private static IEnumerable<object[]> GetFloatConvertInstructionTests<T, TFloat>(MipsEmulatorConfig config, bool legacy)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        // From Single
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "cvt.D.S $f16, $f5", MipsFloatRegister.F16, 2d) { UseLegacyPairedFloatRegisters = legacy }];     // To Double
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "cvt.W.S $f16, $f5", MipsFloatRegister.F16, 2) { UseLegacyPairedFloatRegisters = legacy }];      // To Word

        // From Double
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "cvt.S.D $f16, $f12", MipsFloatRegister.F16, 2f) { UseLegacyPairedFloatRegisters = legacy }];    // To Single
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "cvt.W.D $f16, $f12", MipsFloatRegister.F16, 2) { UseLegacyPairedFloatRegisters = legacy }];     // To Word

        // From Word 
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "cvt.S.W $f16, $f0", MipsFloatRegister.F16, 2f) { UseLegacyPairedFloatRegisters = legacy }];     // To Single
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "cvt.D.W $f16, $f0", MipsFloatRegister.F16, 2d) { UseLegacyPairedFloatRegisters = legacy }];     // To Double

        if (config.VersionInfo.Generation is >= MipsGeneration.MipsIII && config.VersionInfo.Is64Bit)
        {
            // To long
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "cvt.L.S $f16, $f5", MipsFloatRegister.F16, 2L) { UseLegacyPairedFloatRegisters = legacy }];     // From Single
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "cvt.L.D $f16, $f12", MipsFloatRegister.F16, 2L) { UseLegacyPairedFloatRegisters = legacy }];    // From Double

            // From Long
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "cvt.S.L $f16, $f0", MipsFloatRegister.F16, 2f) { UseLegacyPairedFloatRegisters = legacy }];     // To Single
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "cvt.D.L $f16, $f0", MipsFloatRegister.F16, 2d) { UseLegacyPairedFloatRegisters = legacy }];     // To Double
        }
    }

    private static IEnumerable<object[]> GetFloatMoveInstructionTests<T, TFloat>(MipsEmulatorConfig config, bool legacy)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mov.S $f16, $f10", MipsFloatRegister.F16, 1.25f) { UseLegacyPairedFloatRegisters = legacy }];
        yield return [new MipsEmulatorTestCase<T, TFloat>(config, "mov.D $f16, $f18", MipsFloatRegister.F16, Math.PI) { UseLegacyPairedFloatRegisters = legacy }];
    }

    private static IEnumerable<object[]> GetFloatRoundInstructionTests<T, TFloat>(MipsEmulatorConfig config, bool legacy)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        if (config.VersionInfo.Generation is >= MipsGeneration.MipsII)
        {
            // Round
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "round.W.S $f16, $f10", MipsFloatRegister.F16, 1) { UseLegacyPairedFloatRegisters = legacy }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "round.W.D $f16, $f18", MipsFloatRegister.F16, 3) { UseLegacyPairedFloatRegisters = legacy }];

            // Ceiling
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "ceil.W.S $f16, $f10", MipsFloatRegister.F16, 2) { UseLegacyPairedFloatRegisters = legacy }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "ceil.W.D $f16, $f18", MipsFloatRegister.F16, 4) { UseLegacyPairedFloatRegisters = legacy }];

            // Floor
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "floor.W.S $f16, $f10", MipsFloatRegister.F16, 1) { UseLegacyPairedFloatRegisters = legacy }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "floor.W.D $f16, $f18", MipsFloatRegister.F16, 3) { UseLegacyPairedFloatRegisters = legacy }];
        }

        if (config.VersionInfo.Generation is >= MipsGeneration.MipsIII && config.VersionInfo.Is64Bit)
        {
            // Long
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "round.L.S $f16, $f10", MipsFloatRegister.F16, 1L) { UseLegacyPairedFloatRegisters = legacy }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "round.L.D $f16, $f18", MipsFloatRegister.F16, 3L) { UseLegacyPairedFloatRegisters = legacy }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "ceil.L.S $f16, $f10", MipsFloatRegister.F16, 2L) { UseLegacyPairedFloatRegisters = legacy }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "ceil.L.D $f16, $f18", MipsFloatRegister.F16, 4L) { UseLegacyPairedFloatRegisters = legacy }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "floor.L.S $f16, $f10", MipsFloatRegister.F16, 1L) { UseLegacyPairedFloatRegisters = legacy }];
            yield return [new MipsEmulatorTestCase<T, TFloat>(config, "floor.L.D $f16, $f18", MipsFloatRegister.F16, 3L) { UseLegacyPairedFloatRegisters = legacy }];
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
