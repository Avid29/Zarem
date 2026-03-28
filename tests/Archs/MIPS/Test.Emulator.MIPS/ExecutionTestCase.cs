// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models.Enum;
using Zarem.Models.Instructions.Enums.Registers;

namespace Test.Emulator.MIPS
{
    public sealed record ExecutionTestCase<T, TSigned>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>, IMinMaxValue<TSigned>
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
                        (GPRegister.Argument0, T.CreateTruncating(TSigned.MaxValue)),
                        (GPRegister.Argument1, T.CreateTruncating(TSigned.MinValue)),
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

        public (FloatRegister Register, uint Value)[] FPRInitialization { get; init; } = [];

        public (T Address, byte[] Data)[] MemoryInitialization { get; init; } = [];

        public (T High, T Low)? InitialHighLow { get; init; }

        public PrivilegeMode PrivilegeMode
        {
            get => Status.PrivilegeMode;
            init => Status = Status with { PrivilegeMode = value };
        }

        public StatusRegister Status { get; init; }
    }

}
