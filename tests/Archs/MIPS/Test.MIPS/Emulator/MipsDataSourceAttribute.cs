// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Zarem.Emulator.Config;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models.Enums;
using Zarem.Extensions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Registers;

namespace Test.MIPS.Emulator;

[AttributeUsage(AttributeTargets.Method)]
public class MipsDataSourceAttribute : Attribute, ITestDataSource
{
    public const uint K0 = ExecutionTests.K0;
    public const uint K1 = ExecutionTests.K1;

    private readonly MipsVersion _version;
    private readonly ExecutionMode _mode;

    public MipsDataSourceAttribute(MipsVersion version, ExecutionMode mode)
    {
        _version = version;
        _mode = mode;
    }

    public IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        var config = new MipsEmulatorConfig(_version)
        {
            ExecutionMode = _mode,
        };

        return _version.Is64Bit()
            ? GetVersionTests<ulong, long, UInt128>(config)
            : GetVersionTests<uint, int, ulong>(config);
    }

    public string? GetDisplayName(MethodInfo methodInfo, object?[]? data)
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
            .Concat(GetFloatRoundInstructionTests<T>(config));
    }

    private static IEnumerable<object[]> GetArithmeticInstructionTests<T, TSigned, TLong>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Unsigned
        yield return [new ExecutionTestCase<T>(config, "addu $v0, $t2, $t1", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T>(config, "addiu $v0, $t2, 10", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T>(config, "subu $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
        yield return [new ExecutionTestCase<T>(config, "multu $t3, $t2", Split<T, ulong>(30 * 20))];
        yield return [new ExecutionTestCase<T>(config, "divu $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];

        // Signed (without signs)
        yield return [new ExecutionTestCase<T>(config, "add $v0, $t2, $t1", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T>(config, "addi $v0, $t2, 10", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T>(config, "sub $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
        yield return [new ExecutionTestCase<T>(config, "mult $t3, $t2", Split<T, ulong>(30 * 20))];
        yield return [new ExecutionTestCase<T>(config, "div $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];
        yield return [new ExecutionTestCase<T>(config, "sra $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
        yield return [new ExecutionTestCase<T>(config, "srav $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];

        // Signed (with signs)
        unchecked
        {
            yield return [new ExecutionTestCase<T>(config, "add $v0, $t3, $t5", T.CreateTruncating(30 + (-10)))];
            yield return [new ExecutionTestCase<T>(config, "addi $v0, $t3, -10", T.CreateTruncating(30 + (-10)))];
            yield return [new ExecutionTestCase<T>(config, "sub $v0, $t2, $t5", T.CreateTruncating(20 - (-10)))];
            yield return [new ExecutionTestCase<T>(config, "mult $t3, $t6", Split<T, ulong>((ulong)(30 * -20)))];
            yield return [new ExecutionTestCase<T>(config, "div $t3, $t6", (T.CreateTruncating((uint)(30 % -20)), T.CreateTruncating((uint)(30 / -20))))];
        }

        // Overflowing
        unchecked
        {
            // Unsigned (should overflow without trapping)
            yield return [new ExecutionTestCase<T>(config, "addu $v0, $a2, $s1", T.CreateTruncating(uint.MaxValue + 1))];
            yield return [new ExecutionTestCase<T>(config, "addiu $v0, $a2, 1", T.CreateTruncating(uint.MaxValue + 1))];
            yield return [new ExecutionTestCase<T>(config, "subu $v0, $a3, $s1", T.CreateTruncating(uint.MinValue - 1))];
            yield return [new ExecutionTestCase<T>(config, "multu $a2, $a2", Split<T, ulong>((ulong)uint.MaxValue * uint.MaxValue))];
            yield return [new ExecutionTestCase<T>(config, "divu $a2, $a2", (T.CreateTruncating(uint.MaxValue % uint.MaxValue), T.CreateTruncating(uint.MaxValue / uint.MaxValue)))];

            // Note:
            // "mul" does not trap on overflow. We expect the low 32 bits of the result to be written back, and the high 32 bits to be discarded.
            // "mult" also does not trap on overflow, but instead writes the full 64-bit result into the high and low registers.
            // "div" does not trap on overflow either. The behavior is undefined if the quotient is too large to fit in 32 bits.
            // In practice, we will just take the low 32 bits of the quotient and discard the high 32 bits, and write the remainder to the high register.

            // Signed (without signs)
            yield return [new ExecutionTestCase<T>(config, "add $v0, $a0, $s1", MipsTrap.ArithmeticOverflow)];                  // max + 1
            yield return [new ExecutionTestCase<T>(config, "addi $v0, $a0, 1", MipsTrap.ArithmeticOverflow)];                   // max + 1
            yield return [new ExecutionTestCase<T>(config, "sub $v0, $a1, $s1", MipsTrap.ArithmeticOverflow)];                  // min - 1
            yield return [new ExecutionTestCase<T>(config, "mult $a0, $a0", Split<T, ulong>((ulong)int.MaxValue * int.MaxValue))];     // max * max
            yield return [new ExecutionTestCase<T>(config, "div $a0, $a0", (T.CreateTruncating((uint)(int.MaxValue % int.MaxValue)), T.CreateTruncating((uint)(int.MaxValue / int.MaxValue))))];

            // Signed (with signs)
            yield return [new ExecutionTestCase<T>(config, "add $v0, $a1, $s5", MipsTrap.ArithmeticOverflow)];     // min + (-1)
            yield return [new ExecutionTestCase<T>(config, "addi $v0, $a1, -1", MipsTrap.ArithmeticOverflow)];     // min + (-1)
            yield return [new ExecutionTestCase<T>(config, "sub $v0, $a0, $s5", MipsTrap.ArithmeticOverflow)];     // max - (-1)
            yield return [new ExecutionTestCase<T>(config, "mult $a1, $a1", Split<T, ulong>((long)int.MinValue * int.MinValue))];    // min * min
            yield return [new ExecutionTestCase<T>(config, "div $a1, $a1", (T.CreateTruncating((uint)(int.MinValue % int.MinValue)), T.CreateTruncating((uint)(int.MinValue / int.MinValue))))];
        }

        // Division by zero. Undefined behavior, but NOT a trap! (Shouldn't crash the emulator either)
        yield return [new ExecutionTestCase<T>(config, "divu $t3, $zero", MipsTrap.None)];
        yield return [new ExecutionTestCase<T>(config, "div $t3, $zero", MipsTrap.None)];

        if (config.Version is >= MipsVersion.Mips_R1)
        {
            // GPR Multiply
            yield return [new ExecutionTestCase<T>(config, "mul $v0, $t3, $t2", T.CreateTruncating(30 * 20))];
            yield return [new ExecutionTestCase<T>(config, "mul $v0, $t3, $t6", T.CreateTruncating(unchecked((uint)(30 * -20))))];
            yield return [new ExecutionTestCase<T>(config, "mul $v0, $a0, $a0", T.CreateTruncating((uint)unchecked(int.MaxValue * int.MaxValue)))];     // max * max
            yield return [new ExecutionTestCase<T>(config, "mul $v0, $a1, $a1", T.CreateTruncating((uint)unchecked(int.MinValue * int.MinValue)))];     // min * min
        }

        if (config.Version is >= MipsVersion.Mips_R1 and < MipsVersion.Mips_R6)
        {
            // Multiply and Add/Subtract
            yield return [new ExecutionTestCase<T>(config, "maddu $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 + (30 * 20))))];
            yield return [new ExecutionTestCase<T>(config, "madd $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 + (30 * 20))))];
            yield return [new ExecutionTestCase<T>(config, "msubu $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 - (30 * 20))))];
            yield return [new ExecutionTestCase<T>(config, "msub $t3, $t2", (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678 - (30 * 20))))];
        }

        // Not arithmetic, but fixed width
        if (config.Version is >= MipsVersion.Mips_R1)
        {
            // Niche bit-manipulation
            // TODO: ext, ins, seb, seh, wsbh, wshd
            yield return [new ExecutionTestCase<T>(config, "clz $v0, $k0", T.CreateTruncating(BitOperations.LeadingZeroCount(K0)))];
            yield return [new ExecutionTestCase<T>(config, "clo $v0, $k0", T.CreateTruncating(BitOperations.LeadingZeroCount(~K0)))];
        }

        if (config.Version is >= MipsVersion.MipsIII && config.Version.Is64Bit())
        {
            // Unsigned
            yield return [new ExecutionTestCase<T>(config, "daddu $v0, $t2, $t1", T.CreateTruncating(30))];
            yield return [new ExecutionTestCase<T>(config, "daddiu $v0, $t2, 10", T.CreateTruncating(30))];
            yield return [new ExecutionTestCase<T>(config, "dsubu $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
            yield return [new ExecutionTestCase<T>(config, "dmultu $t3, $t2", Split<T, UInt128>(30 * 20))];
            yield return [new ExecutionTestCase<T>(config, "ddivu $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];

            // Signed (without signs)
            yield return [new ExecutionTestCase<T>(config, "dadd $v0, $t2, $t1", T.CreateTruncating(30))];
            yield return [new ExecutionTestCase<T>(config, "daddi $v0, $t2, 10", T.CreateTruncating(30))];
            yield return [new ExecutionTestCase<T>(config, "dsub $v0, $t3, $t2", T.CreateTruncating(30 - 20))];
            yield return [new ExecutionTestCase<T>(config, "dmult $t3, $t2", Split<T, UInt128>(30 * 20))];
            yield return [new ExecutionTestCase<T>(config, "ddiv $t3, $t2", (T.CreateTruncating(30 % 20), T.CreateTruncating(30 / 20)))];
            yield return [new ExecutionTestCase<T>(config, "dsra $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
            yield return [new ExecutionTestCase<T>(config, "dsrav $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];

            // Signed (with signs)
            unchecked
            {
                yield return [new ExecutionTestCase<T>(config, "dadd $v0, $t3, $t5", T.CreateTruncating(30 + (-10)))];
                yield return [new ExecutionTestCase<T>(config, "daddi $v0, $t3, -10", T.CreateTruncating(30 + (-10)))];
                yield return [new ExecutionTestCase<T>(config, "dsub $v0, $t2, $t5", T.CreateTruncating(20 - (-10)))];
                yield return [new ExecutionTestCase<T>(config, "dmult $t3, $t6", Split<T, UInt128>((UInt128)(30 * -20)))];
                yield return [new ExecutionTestCase<T>(config, "ddiv $t3, $t6", (T.CreateTruncating((ulong)(30 % -20)), T.CreateTruncating((ulong)(30 / -20))))];
            }
        }
    }

    private static IEnumerable<object[]> GetLogicalInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        yield return [new ExecutionTestCase<T>(config, "and $v0, $k0, $k1", T.CreateTruncating(K0 & K1))];
        yield return [new ExecutionTestCase<T>(config, "andi $v0, $k0, 0xd16", T.CreateTruncating(K0 & K1))];
        yield return [new ExecutionTestCase<T>(config, "or $v0, $k0, $k1", T.CreateTruncating(K0 | K1))];
        yield return [new ExecutionTestCase<T>(config, "ori $v0, $k0, 0xd16", T.CreateTruncating(K0 | K1))];
        yield return [new ExecutionTestCase<T>(config, "xor $v0, $k0, $k1", T.CreateTruncating(K0 ^ K1))];
        yield return [new ExecutionTestCase<T>(config, "xori $v0, $k0, 0xd16", T.CreateTruncating(K0 ^ K1))];
        yield return [new ExecutionTestCase<T>(config, "nor $v0, $k0, $k1", ~T.CreateTruncating(K0 | K1))];
        yield return [new ExecutionTestCase<T>(config, "sll $v0, $t8, 4", T.CreateTruncating(101 << 4))];
        yield return [new ExecutionTestCase<T>(config, "srl $v0, $t8, 4", T.CreateTruncating(101 >> 4))];
        yield return [new ExecutionTestCase<T>(config, "sllv $v0, $t8, $s4", T.CreateTruncating(101 << 4))];
        yield return [new ExecutionTestCase<T>(config, "srlv $v0, $t8, $s4", T.CreateTruncating(101 >> 4))];
    }

    private static IEnumerable<object[]> GetMemoryInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Load
        yield return [new ExecutionTestCase<T>(config, "lb $v0, 0x1000($zero)", T.CreateTruncating(0x12))];
        yield return [new ExecutionTestCase<T>(config, "lh $v0, 0x1000($zero)", T.CreateTruncating(0x1234))];
        yield return [new ExecutionTestCase<T>(config, "lw $v0, 0x1000($zero)", T.CreateTruncating(0x1234_5678))];

        // TODO: Load unsigned/signed with sign

        // Store
        yield return [new ExecutionTestCase<T>(config, "sb $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0xef, 0x34, 0x56, 0x78]))];
        yield return [new ExecutionTestCase<T>(config, "sh $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0xcd, 0xef, 0x56, 0x78]))];
        yield return [new ExecutionTestCase<T>(config, "sw $at, 0x1000($zero)", (T.CreateTruncating(0x1000), [0x89, 0xab, 0xcd, 0xef]))];
    }

    private static IEnumerable<object[]> GetJumpBranchInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        var linkAddress = T.CreateTruncating(config.DisableDelaySlots ? 4 : 8);
        var noBranchAddress = T.CreateTruncating(config.ExecutionMode is ExecutionMode.JustInTime && !config.DisableDelaySlots ? 8 : 4);

        // Jump
        yield return [new ExecutionTestCase<T>(config, "j 1000") { ExpectedPC = T.CreateTruncating(1000) }];
        yield return [new ExecutionTestCase<T>(config, "jal 1000", MipsGpRegister.ReturnAddress, linkAddress) { ExpectedPC = T.CreateTruncating(1000) }];
        yield return [new ExecutionTestCase<T>(config, "jr $t4") { ExpectedPC = T.CreateTruncating(40) }];
        yield return [new ExecutionTestCase<T>(config, "jalr $t4", MipsGpRegister.ReturnAddress, linkAddress) { ExpectedPC = T.CreateTruncating(40) }];

        // Branch Equality
        yield return [new ExecutionTestCase<T>(config, "beq $t2, $t3, 80") { ExpectedPC = noBranchAddress }];
        yield return [new ExecutionTestCase<T>(config, "beq $t1, $t1, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>(config, "bne $t1, $t1, 80") { ExpectedPC = noBranchAddress }];
        yield return [new ExecutionTestCase<T>(config, "bne $t3, $t2, 80") { ExpectedPC = T.CreateTruncating(84) }];

        // Branch Compare
        yield return [new ExecutionTestCase<T>(config, "blez $s1, 80") { ExpectedPC = noBranchAddress }];
        yield return [new ExecutionTestCase<T>(config, "blez $s0, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>(config, "blez $s5, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>(config, "bgtz $s1, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>(config, "bgtz $s0, 80") { ExpectedPC = noBranchAddress }];
        yield return [new ExecutionTestCase<T>(config, "bgtz $s5, 80") { ExpectedPC = noBranchAddress }];
        yield return [new ExecutionTestCase<T>(config, "bltz $s1, 80") { ExpectedPC = noBranchAddress }];
        yield return [new ExecutionTestCase<T>(config, "bltz $s0, 80") { ExpectedPC = noBranchAddress }];
        yield return [new ExecutionTestCase<T>(config, "bltz $s5, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>(config, "bgez $s1, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>(config, "bgez $s0, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>(config, "bgez $s5, 80") { ExpectedPC = noBranchAddress }];
    }

    private static IEnumerable<object[]> GetCompareInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Unsigned
        yield return [new ExecutionTestCase<T>(config, "sltu $v0, $t2, $t3", T.One)];
        yield return [new ExecutionTestCase<T>(config, "sltu $v0, $t3, $t2", T.Zero)];
        yield return [new ExecutionTestCase<T>(config, "sltu $v0, $t1, $t1", T.Zero)];
        yield return [new ExecutionTestCase<T>(config, "sltiu $v0, $t2, 30", T.One)];
        yield return [new ExecutionTestCase<T>(config, "sltiu $v0, $t3, 20", T.Zero)];
        yield return [new ExecutionTestCase<T>(config, "sltiu $v0, $t1, 10", T.Zero)];

        // Signed (without signs)
        yield return [new ExecutionTestCase<T>(config, "slt $v0, $t2, $t3", T.One)];
        yield return [new ExecutionTestCase<T>(config, "slt $v0, $t3, $t2", T.Zero)];
        yield return [new ExecutionTestCase<T>(config, "slt $v0, $t1, $t1", T.Zero)];
        yield return [new ExecutionTestCase<T>(config, "slti $v0, $t2, 30", T.One)];
        yield return [new ExecutionTestCase<T>(config, "slti $v0, $t3, 20", T.Zero)];
        yield return [new ExecutionTestCase<T>(config, "slti $v0, $t1, 10", T.Zero)];

        // Signed (with signs)
        yield return [new ExecutionTestCase<T>(config, "slt $v0, $t7, $t6", T.One)];
        yield return [new ExecutionTestCase<T>(config, "slt $v0, $t6, $t7", T.Zero)];
        yield return [new ExecutionTestCase<T>(config, "slt $v0, $t5, $t5", T.Zero)];
        yield return [new ExecutionTestCase<T>(config, "slti $v0, $t7, -20", T.One)];
        yield return [new ExecutionTestCase<T>(config, "slti $v0, $t6, -30", T.Zero)];
        yield return [new ExecutionTestCase<T>(config, "slti $v0, $t5, -10", T.Zero)];
    }

    private static IEnumerable<object[]> GetTrapInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        if (config.Version >= MipsVersion.MipsII)
        {
            // Equality
            yield return [new ExecutionTestCase<T>(config, "teq $t2, $t3", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>(config, "teq $t1, $t1", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>(config, "tne $t1, $t1", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>(config, "tne $t3, $t2", MipsTrap.Trap)];

            // Unsigned
            yield return [new ExecutionTestCase<T>(config, "tltu $t3, $t2", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>(config, "tltu $t2, $t3", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>(config, "tltu $t1, $t1", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>(config, "tgeu $t2, $t3", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>(config, "tgeu $t3, $t2", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>(config, "tgeu $t1, $t1", MipsTrap.Trap)];

            // Signed (without signs)
            yield return [new ExecutionTestCase<T>(config, "tlt $t3, $t2", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>(config, "tlt $t2, $t3", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>(config, "tlt $t1, $t1", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>(config, "tge $t2, $t3", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>(config, "tge $t3, $t2", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>(config, "tge $t1, $t1", MipsTrap.Trap)];

            // Signed (with signs)
            yield return [new ExecutionTestCase<T>(config, "tlt $t6, $t7", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>(config, "tlt $t7, $t6", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>(config, "tlt $t5, $t5", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>(config, "tge $t7, $t6", MipsTrap.None)];
            yield return [new ExecutionTestCase<T>(config, "tge $t6, $t7", MipsTrap.Trap)];
            yield return [new ExecutionTestCase<T>(config, "tge $t5, $t5", MipsTrap.Trap)];
        }
    }

    private static IEnumerable<object[]> GetUncategorizedInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // lui
        yield return [new ExecutionTestCase<T>(config, "lui $v0, 0x1234", T.CreateTruncating(0x12340000))];

        if (config.Version is < MipsVersion.Mips_R6)
        {
            // Move from/to high and low registers
            yield return [new ExecutionTestCase<T>(config, "mtlo $k0", (T.CreateTruncating(0x1234), T.CreateTruncating(K0)))];
            yield return [new ExecutionTestCase<T>(config, "mthi $k1", (T.CreateTruncating(K1), T.CreateTruncating(0x5678)))];
            yield return [new ExecutionTestCase<T>(config, "mflo $v0", T.CreateTruncating(0x5678))];
            yield return [new ExecutionTestCase<T>(config, "mfhi $v0", T.CreateTruncating(0x1234))];
        }

        if (config.Version is >= MipsVersion.MipsIV)
        {
            // movz/movn
            yield return [new ExecutionTestCase<T>(config, "movz $k0, $k1, $t0", MipsGpRegister.Kernel0, T.CreateTruncating(K1))];
            yield return [new ExecutionTestCase<T>(config, "movz $k0, $k1, $t1", MipsGpRegister.Zero)];
            yield return [new ExecutionTestCase<T>(config, "movn $k0, $k1, $t0", MipsGpRegister.Zero)];
            yield return [new ExecutionTestCase<T>(config, "movn $k0, $k1, $t1", MipsGpRegister.Kernel0, T.CreateTruncating(K1))];
        }
    }

    private static IEnumerable<object[]> GetSystemInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        yield return [new ExecutionTestCase<T>(config, "syscall", MipsTrap.Syscall)];
        yield return [new ExecutionTestCase<T>(config, "break", MipsTrap.Breakpoint)];

        if (config.Version is >= MipsVersion.MipsII)
        {
            // Exception Return
            yield return [new ExecutionTestCase<T>(config, "eret", MipsTrap.ReservedInstruction)];
            yield return [new ExecutionTestCase<T>(config, "eret", SideEffect.WriteCoProc0)
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
            yield return [new ExecutionTestCase<T>(config, "ei", MipsTrap.ReservedInstruction)];
            yield return [new ExecutionTestCase<T>(config, "ei", SideEffect.WriteCoProc0)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
            yield return [new ExecutionTestCase<T>(config, "ei $v0", MipsGpRegister.ReturnValue0)
            {
                ExpectedSideEffect = SideEffect.WriteCoProc0,
                PrivilegeMode = PrivilegeMode.Kernel
            }];

            // Disable Interrupts
            yield return [new ExecutionTestCase<T>(config, "di", MipsTrap.ReservedInstruction)];
            yield return [new ExecutionTestCase<T>(config, "di", SideEffect.WriteCoProc0)
            {
                PrivilegeMode = PrivilegeMode.Kernel
            }];
            yield return [new ExecutionTestCase<T>(config, "di $v1", MipsGpRegister.ReturnValue1)
            {
                ExpectedSideEffect = SideEffect.WriteCoProc0,
                PrivilegeMode = PrivilegeMode.Kernel
            }];
        }
    }

    private static IEnumerable<object[]> GetCoProcMoveInstructionTest<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // CoProcessor 1
        yield return [new ExecutionTestCase<T>(config, "mtc1 $t2, $f16", MipsFloatRegister.F16, 20)];
        yield return [new ExecutionTestCase<T>(config, "mfc1 $v0, $f0", MipsGpRegister.ReturnValue0, T.CreateTruncating(2))];
    }

    private static IEnumerable<object[]> GetFloatArithmeticInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Single
        yield return [new ExecutionTestCase<T>(config, "add.S $f16, $f8, $f9", MipsFloatRegister.F16, 10.5f + 2.5f)];
        yield return [new ExecutionTestCase<T>(config, "sub.S $f16, $f8, $f9", MipsFloatRegister.F16, 10.5f - 2.5f)];
        yield return [new ExecutionTestCase<T>(config, "mul.S $f16, $f8, $f9", MipsFloatRegister.F16, 10.5f * 2.5f)];
        yield return [new ExecutionTestCase<T>(config, "div.S $f16, $f8, $f9", MipsFloatRegister.F16, 10.5f / 2.5f)];
        yield return [new ExecutionTestCase<T>(config, "abs.S $f16, $f7", MipsFloatRegister.F16, 2f)];
        yield return [new ExecutionTestCase<T>(config, "neg.S $f16, $f5", MipsFloatRegister.F16, -2f)];

        // Double
        yield return [new ExecutionTestCase<T>(config, "add.D $f16, $f12, $f14", MipsFloatRegister.F16, 2d + 0.5d)];
        yield return [new ExecutionTestCase<T>(config, "sub.D $f16, $f12, $f14", MipsFloatRegister.F16, 2d - 0.5d)];
        yield return [new ExecutionTestCase<T>(config, "mul.D $f16, $f12, $f14", MipsFloatRegister.F16, 2d * 0.5d)];
        yield return [new ExecutionTestCase<T>(config, "div.D $f16, $f12, $f14", MipsFloatRegister.F16, 2d / 0.5d)];
        yield return [new ExecutionTestCase<T>(config, "abs.D $f16, $f16", MipsFloatRegister.F16, 2d)];
        yield return [new ExecutionTestCase<T>(config, "neg.D $f16, $f12", MipsFloatRegister.F16, -2d)];

        if (config.Version is >= MipsVersion.MipsII)
        {
            yield return [new ExecutionTestCase<T>(config, "sqrt.S $f16, $f8", MipsFloatRegister.F16, MathF.Sqrt(10.5f))];
            yield return [new ExecutionTestCase<T>(config, "sqrt.D $f16, $f12", MipsFloatRegister.F16, Math.Sqrt(2d))];
        }

        if (config.Version is >= MipsVersion.MipsIV)
        {
            yield return [new ExecutionTestCase<T>(config, "recip.S $f16, $f9", MipsFloatRegister.F16, float.ReciprocalEstimate(2.5f))];
            yield return [new ExecutionTestCase<T>(config, "recip.D $f16, $f12", MipsFloatRegister.F16, double.ReciprocalEstimate(2d))];
        }

        if (config.Version is >= MipsVersion.Mips_R2)
        {
            yield return [new ExecutionTestCase<T>(config, "rsqrt.S $f16, $f9", MipsFloatRegister.F16, float.ReciprocalSqrtEstimate(2.5f))];
            yield return [new ExecutionTestCase<T>(config, "rsqrt.D $f16, $f12", MipsFloatRegister.F16, double.ReciprocalSqrtEstimate(2d))];
        }
    }

    private static IEnumerable<object[]> GetFloatConvertInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // From Single
        yield return [new ExecutionTestCase<T>(config, "cvt.D.S $f16, $f5", MipsFloatRegister.F16, 2d)];     // To Double
        yield return [new ExecutionTestCase<T>(config, "cvt.W.S $f16, $f5", MipsFloatRegister.F16, 2)];      // To Word

        // From Double
        yield return [new ExecutionTestCase<T>(config, "cvt.S.D $f16, $f12", MipsFloatRegister.F16, 2f)];    // To Single
        yield return [new ExecutionTestCase<T>(config, "cvt.W.D $f16, $f12", MipsFloatRegister.F16, 2)];     // To Word

        // From Word 
        yield return [new ExecutionTestCase<T>(config, "cvt.S.W $f16, $f0", MipsFloatRegister.F16, 2f)];     // To Single
        yield return [new ExecutionTestCase<T>(config, "cvt.D.W $f16, $f0", MipsFloatRegister.F16, 2d)];     // To Double

        if (config.Version is >= MipsVersion.MipsIII && config.Version.Is64Bit())
        {
            // To long
            yield return [new ExecutionTestCase<T>(config, "cvt.L.S $f16, $f5", MipsFloatRegister.F16, 2L)];     // From Single
            yield return [new ExecutionTestCase<T>(config, "cvt.L.D $f16, $f12", MipsFloatRegister.F16, 2L)];    // From Double

            // From Long
            yield return [new ExecutionTestCase<T>(config, "cvt.S.L $f16, $f0", MipsFloatRegister.F16, 2f)];     // To Single
            yield return [new ExecutionTestCase<T>(config, "cvt.D.L $f16, $f0", MipsFloatRegister.F16, 2d)];     // To Double
        }
    }

    private static IEnumerable<object[]> GetFloatRoundInstructionTests<T>(MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        if (config.Version is >= MipsVersion.MipsII)
        {
            // Round
            yield return [new ExecutionTestCase<T>(config, "round.W.S $f16, $f10", MipsFloatRegister.F16, 1)];
            yield return [new ExecutionTestCase<T>(config, "round.W.D $f16, $f18", MipsFloatRegister.F16, 3)];

            // Ceiling
            yield return [new ExecutionTestCase<T>(config, "ceil.W.S $f16, $f10", MipsFloatRegister.F16, 2)];
            yield return [new ExecutionTestCase<T>(config, "ceil.W.D $f16, $f18", MipsFloatRegister.F16, 4)];

            // Floor
            yield return [new ExecutionTestCase<T>(config, "floor.W.S $f16, $f10", MipsFloatRegister.F16, 1)];
            yield return [new ExecutionTestCase<T>(config, "floor.W.D $f16, $f18", MipsFloatRegister.F16, 3)];
        }

        if (config.Version is >= MipsVersion.MipsIII && config.Version.Is64Bit())
        {
            // Long
            yield return [new ExecutionTestCase<T>(config, "round.L.S $f16, $f10", MipsFloatRegister.F16, 1L)];
            yield return [new ExecutionTestCase<T>(config, "round.L.D $f16, $f18", MipsFloatRegister.F16, 3L)];
            yield return [new ExecutionTestCase<T>(config, "ceil.L.S $f16, $f10", MipsFloatRegister.F16, 2L)];
            yield return [new ExecutionTestCase<T>(config, "ceil.L.D $f16, $f18", MipsFloatRegister.F16, 4L)];
            yield return [new ExecutionTestCase<T>(config, "floor.L.S $f16, $f10", MipsFloatRegister.F16, 1L)];
            yield return [new ExecutionTestCase<T>(config, "floor.L.D $f16, $f18", MipsFloatRegister.F16, 3L)];
        }
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
