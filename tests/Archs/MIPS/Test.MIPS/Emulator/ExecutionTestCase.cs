// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions.Enums.Registers;

namespace Test.MIPS.Emulator;

public sealed record ExecutionTestCase<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
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
                    (GPRegister.Argument0, T.CreateTruncating(int.MaxValue)),
                    (GPRegister.Argument1, T.CreateTruncating(int.MinValue)),
                    (GPRegister.Argument2, T.MaxValue),
                    (GPRegister.Argument3, T.MinValue),

                    // Saved 1 - 4 are assigned to 1 through 4 respectively,
                    // while saved 5 and 6 are assigned to -1 and -2 (to test sign handling in arithmetic instructions)
                    (GPRegister.Saved1, T.CreateTruncating(1)),
                    (GPRegister.Saved2, T.CreateTruncating(2)),
                    (GPRegister.Saved3, T.CreateTruncating(3)),
                    (GPRegister.Saved4, T.CreateTruncating(4)),
                    (GPRegister.Saved5, T.CreateTruncating(-1)),
                    (GPRegister.Saved6, T.CreateTruncating(-2)),

                    // Temp 1 - 4 are assigned to 10, 20, 30, 40 respectively,
                    // while temp 5 and 6 are assigned to -10 and -20 (to test sign handling in arithmetic instructions)
                    (GPRegister.Temporary1, T.CreateTruncating(10)),
                    (GPRegister.Temporary2, T.CreateTruncating(20)),
                    (GPRegister.Temporary3, T.CreateTruncating(30)),
                    (GPRegister.Temporary4, T.CreateTruncating(40)),
                    (GPRegister.Temporary5, T.CreateTruncating(-10)),
                    (GPRegister.Temporary6, T.CreateTruncating(-20)),
                    (GPRegister.Temporary7, T.CreateTruncating(-30)),

                    // Assign some arbitrary values to the rest of the registers as well, just in case
                    (GPRegister.Temporary8, T.CreateTruncating(101)),
                    (GPRegister.AssemblerTemporary, T.CreateTruncating(0x89ab_cdef)),
                    (GPRegister.Kernel0, T.CreateTruncating(ExecutionTests.K0)),
                    (GPRegister.Kernel1, T.CreateTruncating(ExecutionTests.K1)),

                    // Print integer
                    (GPRegister.ReturnValue0, T.One),
                ];

            FPRInitialization =
                [
                    // F0 - F3: Simple Integers (for CVT.S.W or CVT.D.L tests)
                    (FloatRegister.F0, T.CreateTruncating(2)),
                    (FloatRegister.F1, T.CreateTruncating(0)),
                    (FloatRegister.F2, T.CreateTruncating(10)),
                    (FloatRegister.F3, T.CreateTruncating((uint)-10)),

                    // F4 - F11: Small "Clean" Floats (Single Precision)
                    // Using values that have exact representations in binary
                    (FloatRegister.F4, T.CreateTruncating(BitConverter.SingleToUInt32Bits(1.0f))),
                    (FloatRegister.F5, T.CreateTruncating(BitConverter.SingleToUInt32Bits(2.0f))),
                    (FloatRegister.F6, T.CreateTruncating(BitConverter.SingleToUInt32Bits(0.5f))),
                    (FloatRegister.F7, T.CreateTruncating(BitConverter.SingleToUInt32Bits(-2.0f))),
                    (FloatRegister.F8, T.CreateTruncating(BitConverter.SingleToUInt32Bits(10.5f))),
                    (FloatRegister.F9, T.CreateTruncating(BitConverter.SingleToUInt32Bits(2.5f))),
                    (FloatRegister.F10, T.CreateTruncating(BitConverter.SingleToUInt32Bits(1.25f))),
                    (FloatRegister.F11, T.CreateTruncating(BitConverter.SingleToUInt32Bits(-0.75f))),

                    // F12 - F19: Double Precision Pairs (f12/13, f14/15, etc.)
                    // f12/f13 = 1.0, f14/f15 = 0.5, f16/f17 = -2.0, f18/f19 = PI (approx)
                    (FloatRegister.F12, T.CreateTruncating(BitConverter.DoubleToUInt64Bits(2.0))),
                    (FloatRegister.F13, T.CreateTruncating((uint)(BitConverter.DoubleToUInt64Bits(2.0) >> 32))),

                    (FloatRegister.F14, T.CreateTruncating(BitConverter.DoubleToUInt64Bits(0.5))),
                    (FloatRegister.F15, T.CreateTruncating((uint)(BitConverter.DoubleToUInt64Bits(0.5) >> 32))),

                    (FloatRegister.F16, T.CreateTruncating(BitConverter.DoubleToUInt64Bits(-2.0))),
                    (FloatRegister.F17, T.CreateTruncating((uint)(BitConverter.DoubleToUInt64Bits(-2.0) >> 32))),

                    (FloatRegister.F18, T.CreateTruncating(BitConverter.DoubleToUInt64Bits(Math.PI))),
                    (FloatRegister.F19, T.CreateTruncating((uint)(BitConverter.DoubleToUInt64Bits(Math.PI) >> 32))),

                    // F20 - F27: IEEE 754 Edge Cases (Single Precision)
                    (FloatRegister.F20, T.CreateTruncating(BitConverter.SingleToUInt32Bits(float.PositiveInfinity))),
                    (FloatRegister.F21, T.CreateTruncating(BitConverter.SingleToUInt32Bits(float.NegativeInfinity))),
                    (FloatRegister.F22, T.CreateTruncating(BitConverter.SingleToUInt32Bits(float.NaN))),
                    (FloatRegister.F23, T.CreateTruncating(BitConverter.SingleToUInt32Bits(0.0f))),
                    (FloatRegister.F24, T.CreateTruncating(BitConverter.SingleToUInt32Bits(-0.0f))),
                    (FloatRegister.F25, T.CreateTruncating(BitConverter.SingleToUInt32Bits(float.Epsilon))),
                    (FloatRegister.F26, T.CreateTruncating(BitConverter.SingleToUInt32Bits(float.MaxValue))),
                    (FloatRegister.F27, T.CreateTruncating(BitConverter.SingleToUInt32Bits(float.MinValue))),

                    // F28 - F31: Large Integers (to test Rounding/Overflow traps)
                    (FloatRegister.F28, T.CreateTruncating((uint)int.MaxValue)),
                    (FloatRegister.F29, T.CreateTruncating(0)), // Upper bits for F28 if treated as Long
                    (FloatRegister.F30, T.CreateTruncating((uint)int.MinValue)),
                    (FloatRegister.F31, T.CreateTruncating(0xFFFFFFFF)) // All bits set
                ];

            InitialHighLow = (T.CreateTruncating(0x1234), T.CreateTruncating(0x5678));

            MemoryInitialization =
                [(T.CreateTruncating(0x1000), [0x12, 0x34, 0x56, 0x78])];
        }
    }

    public ExecutionTestCase(string input, MipsTrap trap) : this(input)
    {
        ExpectedTrap = trap;
    }

    public ExecutionTestCase(string input, T writeBack) : this(input)
    {
        ExpectedWriteBack = (GPRegister.ReturnValue0, writeBack);
    }

    public ExecutionTestCase(string input, GPRegister reg, T? writeBack = null) : this(input)
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

    public ExecutionTestCase(string input, (T, byte[]) memory) : this(input)
    {
        ExpectedMemory = memory;
    }

    public ExecutionTestCase(string input, (T, T) highLow) : this(input)
    {
        ExpectedHighLow = highLow;
    }

    public ExecutionTestCase(string input, SideEffect sideEffects) : this(input)
    {
        ExpectedSideEffect = sideEffects;
    }

    public string Input { get; }

    public MipsTrap ExpectedTrap { get; init; } = MipsTrap.None;

    public (GPRegister Regiter, T? Value)? ExpectedWriteBack { get; init; } = null;

    public (FloatRegister Register, int Value)? ExpectedWordFloatWriteBack { get; init; } = null;

    public (FloatRegister Register, long Value)? ExpectedLongFloatWriteBack { get; init; } = null;

    public T? ExpectedPC { get; init; } = null;

    public SideEffect? ExpectedSideEffect { get; init; }

    public (T Address, byte[] Data)? ExpectedMemory { get; init; }

    public (T High, T Low)? ExpectedHighLow { get; init; }

    public (GPRegister Register, T Value)[] RegisterInitialization { get; init; } = [];

    public (FloatRegister Register, T Value)[] FPRInitialization { get; init; } = [];

    public (T Address, byte[] Data)[] MemoryInitialization { get; init; } = [];

    public (T High, T Low)? InitialHighLow { get; init; }

    public PrivilegeMode PrivilegeMode
    {
        get => Status.PrivilegeMode;
        init => Status = Status with { PrivilegeMode = value };
    }

    public StatusRegister Status { get; init; }
}
