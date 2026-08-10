// Avishai Dernis 2026

using System;
using System.Numerics;
using Test.Archs.Emulator;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Emulator.Machine.Registers.CoProcessor0;
using Zarem.Mips.Models.Instructions.Enums.Registers;
using Zarem.Mips.Models.Versioning.Enums;

namespace Test.Mips.Emulator;

public sealed record MipsEmulatorTestCase<T, TFloat> : MipsEmulatorTestCase
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
{
    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input) : base(config, input)
    {
        PrivilegeMode = PrivilegeMode.User;
        UseLegacyPairedFloatRegisters = config.VersionInfo.Generation is < MipsGeneration.MipsIII;

        unchecked
        {
            RegisterInitialization =
                [
                    // Max/Min values to test edge cases, as well as some arbitrary non-edge-case values for good measure
                    // Stored in the argument registers
                    (MipsGpRegister.Argument0, T.CreateTruncating(int.MaxValue)),
                    (MipsGpRegister.Argument1, T.CreateTruncating(int.MinValue)),
                    (MipsGpRegister.Argument2, T.MaxValue),
                    (MipsGpRegister.Argument3, T.MinValue),

                    // Saved 1 - 4 are assigned to 1 through 4 respectively,
                    // while saved 5 and 6 are assigned to -1 and -2 (to test sign handling in arithmetic instructions)
                    (MipsGpRegister.Saved1, T.CreateTruncating(1)),
                    (MipsGpRegister.Saved2, T.CreateTruncating(2)),
                    (MipsGpRegister.Saved3, T.CreateTruncating(3)),
                    (MipsGpRegister.Saved4, T.CreateTruncating(4)),
                    (MipsGpRegister.Saved5, T.CreateTruncating(-1)),
                    (MipsGpRegister.Saved6, T.CreateTruncating(-2)),

                    // Temp 1 - 4 are assigned to 10, 20, 30, 40 respectively,
                    // while temp 5 and 6 are assigned to -10 and -20 (to test sign handling in arithmetic instructions)
                    (MipsGpRegister.Temporary1, T.CreateTruncating(10)),
                    (MipsGpRegister.Temporary2, T.CreateTruncating(20)),
                    (MipsGpRegister.Temporary3, T.CreateTruncating(30)),
                    (MipsGpRegister.Temporary4, T.CreateTruncating(40)),
                    (MipsGpRegister.Temporary5, T.CreateTruncating(-10)),
                    (MipsGpRegister.Temporary6, T.CreateTruncating(-20)),
                    (MipsGpRegister.Temporary7, T.CreateTruncating(-30)),

                    // Assign some arbitrary values to the rest of the registers as well, just in case
                    (MipsGpRegister.Temporary8, T.CreateTruncating(101)),
                    (MipsGpRegister.AssemblerTemporary, T.CreateTruncating(0x89ab_cdef)),
                    (MipsGpRegister.Kernel0, T.CreateTruncating(MipsEmulatorTests.K0)),
                    (MipsGpRegister.Kernel1, T.CreateTruncating(MipsEmulatorTests.K1)),

                    (MipsGpRegister.GlobalPointer, T.CreateTruncating(0x8000_0000)),

                    // Print integer
                    (MipsGpRegister.ReturnValue0, T.One),
                ];

            FPRInitialization =
                [
                    // F0 - F3: Simple Integers (for CVT.S.W or CVT.D.L tests)
                    (MipsFloatRegister.F0, TFloat.CreateTruncating(2)),
                    (MipsFloatRegister.F1, TFloat.CreateTruncating(0)),
                    (MipsFloatRegister.F2, TFloat.CreateTruncating(10)),
                    (MipsFloatRegister.F3, TFloat.CreateTruncating((uint)-10)),

                    // F4 - F11: Small "Clean" Floats (Single Precision)
                    // Using values that have exact representations in binary
                    (MipsFloatRegister.F4, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(1.0f))),
                    (MipsFloatRegister.F5, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(2.0f))),
                    (MipsFloatRegister.F6, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(0.5f))),
                    (MipsFloatRegister.F7, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(-2.0f))),
                    (MipsFloatRegister.F8, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(10.5f))),
                    (MipsFloatRegister.F9, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(2.5f))),
                    (MipsFloatRegister.F10, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(1.25f))),
                    (MipsFloatRegister.F11, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(-0.75f))),

                    // F12 - F19: Double Precision Pairs (f12/13, f14/15, etc.)
                    // f12/f13 = 1.0, f14/f15 = 0.5, f16/f17 = -2.0, f18/f19 = PI (approx)
                    (MipsFloatRegister.F12, TFloat.CreateTruncating(BitConverter.DoubleToUInt64Bits(2.0))),
                    (MipsFloatRegister.F13, TFloat.CreateTruncating((uint)(BitConverter.DoubleToUInt64Bits(2.0) >> 32))),

                    (MipsFloatRegister.F14, TFloat.CreateTruncating(BitConverter.DoubleToUInt64Bits(0.5))),
                    (MipsFloatRegister.F15, TFloat.CreateTruncating((uint)(BitConverter.DoubleToUInt64Bits(0.5) >> 32))),

                    (MipsFloatRegister.F16, TFloat.CreateTruncating(BitConverter.DoubleToUInt64Bits(-2.0))),
                    (MipsFloatRegister.F17, TFloat.CreateTruncating((uint)(BitConverter.DoubleToUInt64Bits(-2.0) >> 32))),

                    (MipsFloatRegister.F18, TFloat.CreateTruncating(BitConverter.DoubleToUInt64Bits(Math.PI))),
                    (MipsFloatRegister.F19, TFloat.CreateTruncating((uint)(BitConverter.DoubleToUInt64Bits(Math.PI) >> 32))),

                    // F20 - F27: IEEE 754 Edge Cases (Single Precision)
                    (MipsFloatRegister.F20, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(float.PositiveInfinity))),
                    (MipsFloatRegister.F21, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(float.NegativeInfinity))),
                    (MipsFloatRegister.F22, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(float.NaN))),
                    (MipsFloatRegister.F23, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(0.0f))),
                    (MipsFloatRegister.F24, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(-0.0f))),
                    (MipsFloatRegister.F25, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(float.Epsilon))),
                    (MipsFloatRegister.F26, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(float.MaxValue))),
                    (MipsFloatRegister.F27, TFloat.CreateTruncating(BitConverter.SingleToUInt32Bits(float.MinValue))),

                    // F28 - F31: Large Integers (to test Rounding/Overflow traps)
                    (MipsFloatRegister.F28, TFloat.CreateTruncating((uint)int.MaxValue)),
                    (MipsFloatRegister.F29, TFloat.CreateTruncating(0)), // Upper bits for F28 if treated as Long
                    (MipsFloatRegister.F30, TFloat.CreateTruncating((uint)int.MinValue)),
                    (MipsFloatRegister.F31, TFloat.CreateTruncating(0xFFFFFFFF)) // All bits set
                ];

            InitialHighLow = (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678));

            MemoryInitialization =
                [(T.CreateTruncating(0x1000), [0x12, 0x34, 0x56, 0x78])];
        }
    }

    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input, MipsTrap trap) : this(config, input)
    {
        ExpectedTrap = trap;
    }

    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input, T writeBack) : this(config, input)
    {
        ExpectedWriteBack = (MipsGpRegister.ReturnValue0, writeBack);
    }

    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input, MipsGpRegister reg, T? writeBack = null) : this(config, input)
    {
        ExpectedWriteBack = (reg, writeBack);
    }

    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input, MipsFloatRegister reg, float writeBack) : this(config, input, reg, BitConverter.SingleToInt32Bits(writeBack))
    {
    }

    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input, MipsFloatRegister reg, double writeBack) : this(config, input, reg, BitConverter.DoubleToInt64Bits(writeBack))
    {
    }

    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input, MipsFloatRegister reg, int writeBack) : this(config, input)
    {
        ExpectedWordFloatWriteBack = (reg, writeBack);
    }

    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input, MipsFloatRegister reg, long writeBack) : this(config, input)
    {
        ExpectedLongFloatWriteBack = (reg, writeBack);
    }

    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input, (T, byte[]) memory) : this(config, input)
    {
        ExpectedMemory = memory;
    }

    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input, (T, T) highLow) : this(config, input)
    {
        ExpectedHighLow = highLow;
    }

    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input, MipsSideEffect sideEffects) : this(config, input)
    {
        ExpectedSideEffect = sideEffects;
    }

    public MipsTrap ExpectedTrap { get; init; } = MipsTrap.None;

    public (MipsGpRegister Register, T? Value)? ExpectedWriteBack { get; init; } = null;

    public (MipsFloatRegister Register, int Value)? ExpectedWordFloatWriteBack { get; init; } = null;

    public (MipsFloatRegister Register, long Value)? ExpectedLongFloatWriteBack { get; init; } = null;

    public T InitialPC { get; init; } = T.Zero;

    public T? ExpectedPC { get; init; } = null;

    public MipsSideEffect? ExpectedSideEffect { get; init; }

    public (T Address, byte[] Data)? ExpectedMemory { get; init; }

    public (T High, T Low)? ExpectedHighLow { get; init; }

    public (MipsGpRegister Register, T Value)[] RegisterInitialization { get; init; } = [];

    public (MipsFloatRegister Register, TFloat Value)[] FPRInitialization { get; init; } = [];

    public (T Address, byte[] Data)[] MemoryInitialization { get; init; } = [];

    public (T High, T Low)? InitialHighLow { get; init; }

    public PrivilegeMode PrivilegeMode
    {
        get => Status.PrivilegeMode;
        init => Status = Status with { PrivilegeMode = value };
    }

    public bool UseLegacyPairedFloatRegisters
    {
        get => !Status.FloatingPoint64BitMode;
        init => Status = Status with { FloatingPoint64BitMode = !value };
    }
}
