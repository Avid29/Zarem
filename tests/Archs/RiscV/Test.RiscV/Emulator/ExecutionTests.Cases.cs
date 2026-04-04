// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Numerics;
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
    }

    private static IEnumerable<object[]> GetArithmeticInstructionTests<T, TS>(RiscVVersionInfo versionInfo)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>, IMinMaxValue<TS>
    {
        // Without signs
        yield return [new ExecutionTestCase<T>("add a0, t1, t0", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T>("addi a0, t1, 10", T.CreateTruncating(30))];
        yield return [new ExecutionTestCase<T>("sub a0, t2, t1", T.CreateTruncating(30 - 20))];
        yield return [new ExecutionTestCase<T>("sra a0, s6, s3", T.CreateTruncating(101 >> 4))];
        yield return [new ExecutionTestCase<T>("srai a0, s6, 4", T.CreateTruncating(101 >> 4))];

        // With signs
        unchecked
        {
            yield return [new ExecutionTestCase<T>("add a0, t2, t4", T.CreateTruncating(30 + (-10)))];
            yield return [new ExecutionTestCase<T>("addi a0, t2, -10", T.CreateTruncating(30 + (-10)))];
            yield return [new ExecutionTestCase<T>("sub a0, t1, t4", T.CreateTruncating(20 - (-10)))];
        }

        // Overflowing
        unchecked
        {
            // Without signs
            yield return [new ExecutionTestCase<T>("add a0, a0, s0", T.CreateTruncating(int.MaxValue + 1))];        // max + 1
            yield return [new ExecutionTestCase<T>("addi a0, a0, 1", T.CreateTruncating(int.MaxValue + 1))];        // max + 1
            yield return [new ExecutionTestCase<T>("sub a0, a1, s0", T.CreateTruncating(int.MinValue - 1))];        // min - 1

            // With signs
            yield return [new ExecutionTestCase<T>("add a0, a1, s4", T.CreateTruncating(int.MinValue + (-1)))];     // min + (-1)
            yield return [new ExecutionTestCase<T>("addi a0, a1, -1", T.CreateTruncating(int.MinValue + (-1)))];    // min + (-1)
            yield return [new ExecutionTestCase<T>("sub a0, a0, s4", T.CreateTruncating(int.MaxValue - (-1)))];     // max - (-1)
        }

        if (versionInfo.Base is >= RiscVBaseVersion.RV64)
        {

        }

        if (versionInfo.Base is >= RiscVBaseVersion.RV128)
        {

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
}
