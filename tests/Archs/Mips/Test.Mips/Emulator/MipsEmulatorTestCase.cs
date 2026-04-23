// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Registers;
using Zarem.Models.Instructions.Enums.Registers;

namespace Test.MIPS.Emulator;

public sealed record MipsEmulatorTestCase<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
{
    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input)
    {
        Config = config;
        Input = input;

        PrivilegeMode = PrivilegeMode.User;

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
                    (MipsGpRegister.Kernel0, T.CreateTruncating(ExecutionTests.K0)),
                    (MipsGpRegister.Kernel1, T.CreateTruncating(ExecutionTests.K1)),

                    // Print integer
                    (MipsGpRegister.ReturnValue0, T.One),
                ];

            FPRInitialization =
                [
                    // F0 - F3: Simple Integers (for CVT.S.W or CVT.D.L tests)
                    (MipsFloatRegister.F0, T.CreateTruncating(2)),
                    (MipsFloatRegister.F1, T.CreateTruncating(0)),
                    (MipsFloatRegister.F2, T.CreateTruncating(10)),
                    (MipsFloatRegister.F3, T.CreateTruncating((uint)-10)),

                    // F4 - F11: Small "Clean" Floats (Single Precision)
                    // Using values that have exact representations in binary
                    (MipsFloatRegister.F4, T.CreateTruncating(BitConverter.SingleToUInt32Bits(1.0f))),
                    (MipsFloatRegister.F5, T.CreateTruncating(BitConverter.SingleToUInt32Bits(2.0f))),
                    (MipsFloatRegister.F6, T.CreateTruncating(BitConverter.SingleToUInt32Bits(0.5f))),
                    (MipsFloatRegister.F7, T.CreateTruncating(BitConverter.SingleToUInt32Bits(-2.0f))),
                    (MipsFloatRegister.F8, T.CreateTruncating(BitConverter.SingleToUInt32Bits(10.5f))),
                    (MipsFloatRegister.F9, T.CreateTruncating(BitConverter.SingleToUInt32Bits(2.5f))),
                    (MipsFloatRegister.F10, T.CreateTruncating(BitConverter.SingleToUInt32Bits(1.25f))),
                    (MipsFloatRegister.F11, T.CreateTruncating(BitConverter.SingleToUInt32Bits(-0.75f))),

                    // F12 - F19: Double Precision Pairs (f12/13, f14/15, etc.)
                    // f12/f13 = 1.0, f14/f15 = 0.5, f16/f17 = -2.0, f18/f19 = PI (approx)
                    (MipsFloatRegister.F12, T.CreateTruncating(BitConverter.DoubleToUInt64Bits(2.0))),
                    (MipsFloatRegister.F13, T.CreateTruncating((uint)(BitConverter.DoubleToUInt64Bits(2.0) >> 32))),

                    (MipsFloatRegister.F14, T.CreateTruncating(BitConverter.DoubleToUInt64Bits(0.5))),
                    (MipsFloatRegister.F15, T.CreateTruncating((uint)(BitConverter.DoubleToUInt64Bits(0.5) >> 32))),

                    (MipsFloatRegister.F16, T.CreateTruncating(BitConverter.DoubleToUInt64Bits(-2.0))),
                    (MipsFloatRegister.F17, T.CreateTruncating((uint)(BitConverter.DoubleToUInt64Bits(-2.0) >> 32))),

                    (MipsFloatRegister.F18, T.CreateTruncating(BitConverter.DoubleToUInt64Bits(Math.PI))),
                    (MipsFloatRegister.F19, T.CreateTruncating((uint)(BitConverter.DoubleToUInt64Bits(Math.PI) >> 32))),

                    // F20 - F27: IEEE 754 Edge Cases (Single Precision)
                    (MipsFloatRegister.F20, T.CreateTruncating(BitConverter.SingleToUInt32Bits(float.PositiveInfinity))),
                    (MipsFloatRegister.F21, T.CreateTruncating(BitConverter.SingleToUInt32Bits(float.NegativeInfinity))),
                    (MipsFloatRegister.F22, T.CreateTruncating(BitConverter.SingleToUInt32Bits(float.NaN))),
                    (MipsFloatRegister.F23, T.CreateTruncating(BitConverter.SingleToUInt32Bits(0.0f))),
                    (MipsFloatRegister.F24, T.CreateTruncating(BitConverter.SingleToUInt32Bits(-0.0f))),
                    (MipsFloatRegister.F25, T.CreateTruncating(BitConverter.SingleToUInt32Bits(float.Epsilon))),
                    (MipsFloatRegister.F26, T.CreateTruncating(BitConverter.SingleToUInt32Bits(float.MaxValue))),
                    (MipsFloatRegister.F27, T.CreateTruncating(BitConverter.SingleToUInt32Bits(float.MinValue))),

                    // F28 - F31: Large Integers (to test Rounding/Overflow traps)
                    (MipsFloatRegister.F28, T.CreateTruncating((uint)int.MaxValue)),
                    (MipsFloatRegister.F29, T.CreateTruncating(0)), // Upper bits for F28 if treated as Long
                    (MipsFloatRegister.F30, T.CreateTruncating((uint)int.MinValue)),
                    (MipsFloatRegister.F31, T.CreateTruncating(0xFFFFFFFF)) // All bits set
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

    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input, SideEffect sideEffects) : this(config, input)
    {
        ExpectedSideEffect = sideEffects;
    }

    public MipsEmulatorConfig Config { get; }

    public string Input { get; }

    public MipsTrap ExpectedTrap { get; init; } = MipsTrap.None;

    public (MipsGpRegister Register, T? Value)? ExpectedWriteBack { get; init; } = null;

    public (MipsFloatRegister Register, int Value)? ExpectedWordFloatWriteBack { get; init; } = null;

    public (MipsFloatRegister Register, long Value)? ExpectedLongFloatWriteBack { get; init; } = null;

    public T? ExpectedPC { get; init; } = null;

    public SideEffect? ExpectedSideEffect { get; init; }

    public (T Address, byte[] Data)? ExpectedMemory { get; init; }

    public (T High, T Low)? ExpectedHighLow { get; init; }

    public (MipsGpRegister Register, T Value)[] RegisterInitialization { get; init; } = [];

    public (MipsFloatRegister Register, T Value)[] FPRInitialization { get; init; } = [];

    public (T Address, byte[] Data)[] MemoryInitialization { get; init; } = [];

    public (T High, T Low)? InitialHighLow { get; init; }

    public PrivilegeMode PrivilegeMode
    {
        get => Status.PrivilegeMode;
        init => Status = Status with { PrivilegeMode = value };
    }

    public StatusRegister Status { get; init; }
}
