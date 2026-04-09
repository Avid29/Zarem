// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Numerics;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Versioning;
using Zarem.Models.Versioning.Enums;

namespace Test.RiscV.Emulator;

public partial class ExecutionTests
{
    public static IEnumerable<object[]> InstructionTestList_RV32_I
        => GetVersionTests<uint, int>(new RiscVVersionInfo(RiscVBaseVersion.RV32, RiscVExtensions.Integers));
    public static IEnumerable<object[]> InstructionTestList_RV64_I
        => GetVersionTests<ulong, long>(new RiscVVersionInfo(RiscVBaseVersion.RV64, RiscVExtensions.Integers));
    public static IEnumerable<object[]> InstructionTestList_RV128_I
        => GetVersionTests<UInt128, Int128>(new RiscVVersionInfo(RiscVBaseVersion.RV128, RiscVExtensions.Integers));

    private static IEnumerable<object[]> GetVersionTests<T, TS>(RiscVVersionInfo versionInfo)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>, IMinMaxValue<TS>
    {
        foreach (var test in GetArithmeticInstructionTests<T, TS>(versionInfo))
            yield return test;

        foreach (var test in GetLogicalInstructionTests<T>(versionInfo))
            yield return test;

        foreach (var test in GetJumpBranchInstructionTests<T>(versionInfo))
            yield return test;
    }

    private static IEnumerable<object[]> GetArithmeticInstructionTests<T, TS>(RiscVVersionInfo versionInfo)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>, IMinMaxValue<TS>
    {
        // Without signs
        yield return [new ExecutionTestCase<T>("add a0, t1, t0", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T>("addi a0, t1, 10", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T>("sub a0, t2, t1", T.CreateTruncating(30 - 20))];
        yield return [new ExecutionTestCase<T>("sra a0, s6, s3", T.CreateTruncating(TS.CreateTruncating(101)) >> 4)];
        yield return [new ExecutionTestCase<T>("srai a0, s6, 4", T.CreateTruncating(TS.CreateTruncating(101)) >> 4)];

        // With signs
        unchecked
        {
            yield return [new ExecutionTestCase<T>("add a0, t2, t4", T.CreateTruncating(30) + T.CreateTruncating(-10))];
            yield return [new ExecutionTestCase<T>("addi a0, t2, -10", T.CreateTruncating(30) + T.CreateTruncating(-10))];
            yield return [new ExecutionTestCase<T>("sub a0, t1, t4", T.CreateTruncating(20) - T.CreateTruncating(-10))];
        }

        // Overflowing
        unchecked
        {
            // Without signs
            yield return [new ExecutionTestCase<T>("add a0, a0, s0", T.CreateTruncating(int.MaxValue) + T.One)];        // max + 1
            yield return [new ExecutionTestCase<T>("addi a0, a0, 1", T.CreateTruncating(int.MaxValue) + T.One)];        // max + 1
            yield return [new ExecutionTestCase<T>("sub a0, a1, s0", T.CreateTruncating(int.MinValue) - T.One)];        // min - 1

            // With signs
            yield return [new ExecutionTestCase<T>("add a0, a1, s4", T.CreateTruncating(int.MinValue) + (-T.One))];     // min + (-1)
            yield return [new ExecutionTestCase<T>("addi a0, a1, -1", T.CreateTruncating(int.MinValue) + (-T.One))];    // min + (-1)
            yield return [new ExecutionTestCase<T>("sub a0, a0, s4", T.CreateTruncating(int.MaxValue) - (-T.One))];     // max - (-1)
        }

        if (versionInfo.Base is >= RiscVBaseVersion.RV64)
        {
            // TODO: Explicit 32-bit instructions
        }

        if (versionInfo.Base is >= RiscVBaseVersion.RV128)
        {
            // TODO: Explicit 64-bit instructions
        }
    }

    private static IEnumerable<object[]> GetLogicalInstructionTests<T>(RiscVVersionInfo versionInfo)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        yield return [new ExecutionTestCase<T>("and a0, s8, s9", T.CreateTruncating(K0 & K1))];
        yield return [new ExecutionTestCase<T>("andi a0, s8, 0x516", T.CreateTruncating(K0 & K1))];
        yield return [new ExecutionTestCase<T>("or a0, s8, s9", T.CreateTruncating(K0 | K1))];
        yield return [new ExecutionTestCase<T>("ori a0, s8, 0x516", T.CreateTruncating(K0 | K1))];
        yield return [new ExecutionTestCase<T>("xor a0, s8, s9", T.CreateTruncating(K0 ^ K1))];
        yield return [new ExecutionTestCase<T>("xori a0, s8, 0x516", T.CreateTruncating(K0 ^ K1))];
        yield return [new ExecutionTestCase<T>("sll a0, s6, s3", T.CreateTruncating(101 << 4))];
        yield return [new ExecutionTestCase<T>("srl a0, s6, s3", T.CreateTruncating(101 >> 4))];
        yield return [new ExecutionTestCase<T>("slli a0, s6, 4", T.CreateTruncating(101 << 4))];
        yield return [new ExecutionTestCase<T>("srli a0, s6, 4", T.CreateTruncating(101 >> 4))];
    }

    private static IEnumerable<object[]> GetJumpBranchInstructionTests<T>(RiscVVersionInfo versionInfo)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // Jump
        yield return [new ExecutionTestCase<T>("j 1000") { ExpectedPC = T.CreateTruncating(1000) }];
        yield return [new ExecutionTestCase<T>("jal 1000", GPRegister.ReturnAddress, T.CreateTruncating(4)) { ExpectedPC = T.CreateTruncating(1000) }];

        // Branch Equality
        yield return [new ExecutionTestCase<T>("beq t1, t2, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T>("beq t0, t0, 80") { ExpectedPC = T.CreateTruncating(84) }];
        yield return [new ExecutionTestCase<T>("bne t0, t0, 80") { ExpectedPC = T.CreateTruncating(4) }];
        yield return [new ExecutionTestCase<T>("bne t2, t1, 80") { ExpectedPC = T.CreateTruncating(84) }];
    }
}
