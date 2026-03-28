// Avishai Dernis 2026

using System;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models.Enum;
using Zarem.Models.Instructions.Enums.Registers;

namespace Test.Emulator.MIPS
{
    public sealed record ExecutionTestCase
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
                        (GPRegister.Argument0, int.MaxValue),
                        (GPRegister.Argument1, (uint)int.MinValue),
                        (GPRegister.Argument2, uint.MaxValue),
                        (GPRegister.Argument3, uint.MinValue),

                        // Saved 1 - 4 are assigned to 1 through 4 respectively,
                        // while saved 5 and 6 are assigned to -1 and -2 (to test sign handling in arithmetic instructions)
                        (GPRegister.Saved1, 1),
                        (GPRegister.Saved2, 2),
                        (GPRegister.Saved3, 3),
                        (GPRegister.Saved4, 4),
                        (GPRegister.Saved5, (uint)-1),
                        (GPRegister.Saved6, (uint)-2),

                        // Temp 1 - 4 are assigned to 10, 20, 30, 40 respectively,
                        // while temp 5 and 6 are assigned to -10 and -20 (to test sign handling in arithmetic instructions)
                        (GPRegister.Temporary1, 10),
                        (GPRegister.Temporary2, 20),
                        (GPRegister.Temporary3, 30),
                        (GPRegister.Temporary4, 40),
                        (GPRegister.Temporary5, (uint)-10),
                        (GPRegister.Temporary6, (uint)-20),
                        (GPRegister.Temporary7, (uint)-30),

                        // Assign some arbitrary values to the rest of the registers as well, just in case
                        (GPRegister.Temporary8, 101),
                        (GPRegister.AssemblerTemporary, 0x89ab_cdef),
                        (GPRegister.Kernel0, ExecutionTests.K0),
                        (GPRegister.Kernel1, ExecutionTests.K1),

                        // Print integer
                        (GPRegister.ReturnValue0, 1),
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

                InitialHighLow = (0x1234, 0x5678);

                MemoryInitialization =
                    [(0x1000, [0x12, 0x34, 0x56, 0x78])];
            }
        }

        public ExecutionTestCase(string input, MipsTrap trap) : this(input)
        {
            ExpectedTrap = trap;
        }

        public ExecutionTestCase(string input, uint writeBack) : this(input)
        {
            ExpectedWriteBack = (GPRegister.ReturnValue0, writeBack);
        }

        public ExecutionTestCase(string input, GPRegister reg, uint? writeBack = null) : this(input)
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

        public ExecutionTestCase(string input, (uint, byte[]) memory) : this(input)
        {
            ExpectedMemory = memory;
        }

        public ExecutionTestCase(string input, ulong highLow) : this(input)
        {
            ExpectedHighLow = ((uint)(highLow >> 32), (uint)highLow);
        }

        public ExecutionTestCase(string input, (uint, uint) highLow) : this(input)
        {
            ExpectedHighLow = highLow;
        }

        public ExecutionTestCase(string input, SideEffect sideEffects) : this(input)
        {
            ExpectedSideEffect = sideEffects;
        }

        public string Input { get; }

        public MipsTrap ExpectedTrap { get; init; } = MipsTrap.None;

        public (GPRegister Regiter, uint? Value)? ExpectedWriteBack { get; init; } = null;

        public (FloatRegister Register, int Value)? ExpectedWordFloatWriteBack { get; init; } = null;

        public (FloatRegister Register, long Value)? ExpectedLongFloatWriteBack { get; init; } = null;

        public uint? ExpectedPC { get; init; } = null;

        public SideEffect? ExpectedSideEffect { get; init; }

        public (uint Address, byte[] Data)? ExpectedMemory { get; init; }

        public (uint High, uint Low)? ExpectedHighLow { get; init; }

        public (GPRegister Register, uint Value)[] RegisterInitialization { get; init; } = [];

        public (FloatRegister Register, uint Value)[] FPRInitialization { get; init; } = [];

        public (uint Address, byte[] Data)[] MemoryInitialization { get; init; } = [];

        public (uint High, uint Low)? InitialHighLow { get; init; }

        public PrivilegeMode PrivilegeMode
        {
            get => Status.PrivilegeMode;
            init => Status = Status with { PrivilegeMode = value };
        }

        public StatusRegister Status { get; init; }
    }

}
