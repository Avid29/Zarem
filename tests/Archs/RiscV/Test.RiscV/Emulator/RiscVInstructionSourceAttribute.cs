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
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Versioning;
using Zarem.Models.Versioning.Enums;

namespace Test.RiscV.Emulator;

[AttributeUsage(AttributeTargets.Method)]
public class RiscVInstructionSourceAttribute : InstructionSourceAttribute<RiscVEmulatorConfig>
{
    public const uint K0 = RiscVExecutionTests.K0;
    public const uint K1 = RiscVExecutionTests.K1;

    private readonly RiscVVersionInfo _versionInfo;
    private readonly ExecutionMode _mode;

    public RiscVInstructionSourceAttribute(string versionStr, ExecutionMode mode)
    {
        _versionInfo = RiscVVersionInfo.Parse(versionStr);
        _mode = mode;
    }

    public override IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        var config = new RiscVEmulatorConfig(_versionInfo)
        {
            ExecutionMode = _mode,
        };

        return _versionInfo.Base switch
        {
            RiscVBaseVersion.RV32 => GetVersionTests<uint, int, ulong>(config),
            RiscVBaseVersion.RV64 => GetVersionTests<ulong, long, UInt128>(config),
            RiscVBaseVersion.RV128 => GetVersionTests<UInt128, Int128, UInt128>(config),
            _ => throw new NotImplementedException()
        };
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

        return str;
    }

    private static IEnumerable<object[]> GetVersionTests<T, TSigned, TLong>(RiscVEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
        where TLong : unmanaged, IBinaryInteger<TLong>, IUnsignedNumber<TLong>, IMinMaxValue<TLong>
    {
        return GetArithmeticInstructionTests<T, TSigned>(config)
            .Concat(GetMultiplicationInstructionTests<T, TSigned, TLong>(config))
            .Concat(GetLogicalInstructionTests<T>(config))
            .Concat(GetJumpBranchInstructionTests<T>(config))
            .Concat(GetMemoryInstructionTests<T>(config))
            .Concat(GetSystemInstructionTests<T>(config));
    }

    private static IEnumerable<object[]> GetArithmeticInstructionTests<T, TSigned>(RiscVEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
    {
        // Without signs
        yield return [new RiscVEmulatorTestCase<T>(config, "add a0, t1, t0", T.CreateTruncating(20 + 10))];
        yield return [new RiscVEmulatorTestCase<T>(config, "addi a0, t1, 10", T.CreateTruncating(20 + 10))];
        yield return [new RiscVEmulatorTestCase<T>(config, "sub a0, t2, t1", T.CreateTruncating(30 - 20))];
        yield return [new RiscVEmulatorTestCase<T>(config, "sra a0, s6, s3", T.CreateTruncating(TSigned.CreateTruncating(101)) >> 4)];
        yield return [new RiscVEmulatorTestCase<T>(config, "srai a0, s6, 4", T.CreateTruncating(TSigned.CreateTruncating(101)) >> 4)];

        // With signs
        unchecked
        {
            yield return [new RiscVEmulatorTestCase<T>(config, "add a0, t2, t4", T.CreateTruncating(30) + T.CreateTruncating(-10))];
            yield return [new RiscVEmulatorTestCase<T>(config, "addi a0, t2, -10", T.CreateTruncating(30) + T.CreateTruncating(-10))];
            yield return [new RiscVEmulatorTestCase<T>(config, "sub a0, t1, t4", T.CreateTruncating(20) - T.CreateTruncating(-10))];
        }

        // Overflowing
        unchecked
        {
            // Without signs
            yield return [new RiscVEmulatorTestCase<T>(config, "add a0, a0, s0", T.CreateTruncating(int.MaxValue) + T.One)];        // max + 1
            yield return [new RiscVEmulatorTestCase<T>(config, "addi a0, a0, 1", T.CreateTruncating(int.MaxValue) + T.One)];        // max + 1
            yield return [new RiscVEmulatorTestCase<T>(config, "sub a0, a1, s0", T.CreateTruncating(int.MinValue) - T.One)];        // min - 1

            // With signs
            yield return [new RiscVEmulatorTestCase<T>(config, "add a0, a1, s4", T.CreateTruncating(int.MinValue) + (-T.One))];     // min + (-1)
            yield return [new RiscVEmulatorTestCase<T>(config, "addi a0, a1, -1", T.CreateTruncating(int.MinValue) + (-T.One))];    // min + (-1)
            yield return [new RiscVEmulatorTestCase<T>(config, "sub a0, a0, s4", T.CreateTruncating(int.MaxValue) - (-T.One))];     // max - (-1)
        }

        if (config.VersionInfo.Base is >= RiscVBaseVersion.RV64)
        {
            // TODO: Explicit 32-bit instructions
        }

        if (config.VersionInfo.Base is >= RiscVBaseVersion.RV128)
        {
            // TODO: Explicit 64-bit instructions
        }
    }

    private static IEnumerable<object[]> GetMultiplicationInstructionTests<T, TSigned, TLong>(RiscVEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
        where TLong : unmanaged, IBinaryInteger<TLong>, IUnsignedNumber<TLong>, IMinMaxValue<TLong>
    {
        if (!config.VersionInfo.Extensions.HasFlag(RiscVExtensions.Multiplication))
            yield break;

        // Without signs
        yield return [new RiscVEmulatorTestCase<T>(config, "mul a0, t2, t1", T.CreateTruncating(30 * 20))];
        yield return [new RiscVEmulatorTestCase<T>(config, "mulh a0, t2, t1", T.CreateTruncating(TLong.CreateTruncating(30 * 20) >> 32))];  // TODO: allow other shifts
        yield return [new RiscVEmulatorTestCase<T>(config, "div a0, t2, t1", T.CreateTruncating(30 / 20))];
        yield return [new RiscVEmulatorTestCase<T>(config, "divu a0, t2, t1", T.CreateTruncating(30 / 20))];
        yield return [new RiscVEmulatorTestCase<T>(config, "rem a0, t2, t1", T.CreateTruncating(30 % 20))];
        yield return [new RiscVEmulatorTestCase<T>(config, "remu a0, t2, t1", T.CreateTruncating(30 % 20))];

        // With signs
        unchecked
        {
            yield return [new RiscVEmulatorTestCase<T>(config, "mul a0, t2, t5", T.CreateTruncating(30 * -20))];
            yield return [new RiscVEmulatorTestCase<T>(config, "mulh a0, t2, t5", T.CreateTruncating((TLong.CreateTruncating(30) * TLong.CreateTruncating(-20)) >> 32))];  // TODO: allow other shifts
            yield return [new RiscVEmulatorTestCase<T>(config, "div a0, t2, t5", T.CreateTruncating(30 / -20))];
            yield return [new RiscVEmulatorTestCase<T>(config, "divu a0, t2, t5", T.CreateTruncating(30) / T.CreateTruncating(-20))];
            yield return [new RiscVEmulatorTestCase<T>(config, "rem a0, t2, t5", T.CreateTruncating(30 % -20))];
            yield return [new RiscVEmulatorTestCase<T>(config, "remu a0, t2, t5", T.CreateTruncating(30) % T.CreateTruncating(-20))];
        }
    }

    private static IEnumerable<object[]> GetLogicalInstructionTests<T>(RiscVEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        yield return [new RiscVEmulatorTestCase<T>(config, "and a0, s8, s9", T.CreateTruncating(K0 & K1))];
        yield return [new RiscVEmulatorTestCase<T>(config, "andi a0, s8, 0x516", T.CreateTruncating(K0 & K1))];
        yield return [new RiscVEmulatorTestCase<T>(config, "or a0, s8, s9", T.CreateTruncating(K0 | K1))];
        yield return [new RiscVEmulatorTestCase<T>(config, "ori a0, s8, 0x516", T.CreateTruncating(K0 | K1))];
        yield return [new RiscVEmulatorTestCase<T>(config, "xor a0, s8, s9", T.CreateTruncating(K0 ^ K1))];
        yield return [new RiscVEmulatorTestCase<T>(config, "xori a0, s8, 0x516", T.CreateTruncating(K0 ^ K1))];
        yield return [new RiscVEmulatorTestCase<T>(config, "sll a0, s6, s3", T.CreateTruncating(101 << 4))];
        yield return [new RiscVEmulatorTestCase<T>(config, "slli a0, s6, 4", T.CreateTruncating(101 << 4))];
        yield return [new RiscVEmulatorTestCase<T>(config, "srl a0, s6, s3", T.CreateTruncating(101 >> 4))];
        yield return [new RiscVEmulatorTestCase<T>(config, "srli a0, s6, 4", T.CreateTruncating(101 >> 4))];
    }

    private static IEnumerable<object[]> GetJumpBranchInstructionTests<T>(RiscVEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Jump
        yield return [new RiscVEmulatorTestCase<T>(config, "j 1000") { ExpectedPC = T.CreateTruncating(1000) }];
        yield return [new RiscVEmulatorTestCase<T>(config, "jal 1000", RiscVGpRegister.ReturnAddress, T.CreateTruncating(4)) { ExpectedPC = T.CreateTruncating(1000) }];

        // Branch Equality
        yield return [new RiscVEmulatorTestCase<T>(config, "beq t1, t2, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new RiscVEmulatorTestCase<T>(config, "beq t0, t0, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new RiscVEmulatorTestCase<T>(config, "bne t0, t0, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new RiscVEmulatorTestCase<T>(config, "bne t2, t1, 80") { ExpectedPC = T.CreateTruncating(84) }];
    }

    private static IEnumerable<object[]> GetMemoryInstructionTests<T>(RiscVEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Load
        yield return [new RiscVEmulatorTestCase<T>(config, "lb a0, 0x100(zero)", T.CreateTruncating(0x12))];
        yield return [new RiscVEmulatorTestCase<T>(config, "lh a0, 0x100(zero)", T.CreateTruncating(0x3412))];
        yield return [new RiscVEmulatorTestCase<T>(config, "lw a0, 0x100(zero)", T.CreateTruncating(0x7856_3412))];

        // TODO: Load unsigned/signed with sign

        // Store
        yield return [new RiscVEmulatorTestCase<T>(config, "sb s7, 0x100(zero)", (T.CreateTruncating(0x100), [0xef, 0x34, 0x56, 0x78]))];
        yield return [new RiscVEmulatorTestCase<T>(config, "sh s7, 0x100(zero)", (T.CreateTruncating(0x100), [0xef, 0xcd, 0x56, 0x78]))];
        yield return [new RiscVEmulatorTestCase<T>(config, "sw s7, 0x100(zero)", (T.CreateTruncating(0x100), [0xef, 0xcd, 0xab, 0x89]))];
    }

    private static IEnumerable<object[]> GetSystemInstructionTests<T>(RiscVEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Syscall and break
        yield return [new RiscVEmulatorTestCase<T>(config, "ecall", RiscVTrap.EnvironmentCallFromUMode)];
        yield return [new RiscVEmulatorTestCase<T>(config, "ebreak", RiscVTrap.Breakpoint)];
    }
}
