// Avishai Dernis 2026

using System;
using System.Numerics;
using Test.Archs.Emulator;
using Zarem.Emulator.Config;
using Zarem.Mips.Models.Instructions.Enums.Registers;
using Zarem.RiscV.Emulator.Config;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Test.RiscV.Emulator;

public sealed record RiscVEmulatorTestCase<T> : EmulatorTestCase<RiscVEmulatorConfig>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
{
    public RiscVEmulatorTestCase(RiscVEmulatorConfig config, string input) : base(config, input)
    {
        unchecked
        {
            RegisterInitialization =
                [
                    // Max/Min values to test edge cases, as well as some arbitrary non-edge-case values for good measure
                    // Stored in the argument registers
                    (RiscVGpRegister.Argument0, T.CreateTruncating(int.MaxValue)),
                    (RiscVGpRegister.Argument1, T.CreateTruncating(int.MinValue)),
                    (RiscVGpRegister.Argument2, T.CreateTruncating(uint.MaxValue)),
                    (RiscVGpRegister.Argument3, T.CreateTruncating(uint.MinValue)),

                    // Saved 0 - 3 are assigned to 1 through 4 respectively,
                    // while saved 4 and 5 are assigned to -1 and -2 (to test sign handling in arithmetic instructions)
                    (RiscVGpRegister.Saved0, T.CreateTruncating(1)),
                    (RiscVGpRegister.Saved1, T.CreateTruncating(2)),
                    (RiscVGpRegister.Saved2, T.CreateTruncating(3)),
                    (RiscVGpRegister.Saved3, T.CreateTruncating(4)),
                    (RiscVGpRegister.Saved4, T.CreateTruncating(-1)),
                    (RiscVGpRegister.Saved5, T.CreateTruncating(-2)),

                    // Temp 1 - 4 are assigned to 10, 20, 30, 40 respectively,
                    // while temp 5 and 6 are assigned to -10 and -20 (to test sign handling in arithmetic instructions)
                    (RiscVGpRegister.Temporary0, T.CreateTruncating(10)),
                    (RiscVGpRegister.Temporary1, T.CreateTruncating(20)),
                    (RiscVGpRegister.Temporary2, T.CreateTruncating(30)),
                    (RiscVGpRegister.Temporary3, T.CreateTruncating(40)),
                    (RiscVGpRegister.Temporary4, T.CreateTruncating(-10)),
                    (RiscVGpRegister.Temporary5, T.CreateTruncating(-20)),
                    (RiscVGpRegister.Temporary6, T.CreateTruncating(-30)),

                    // Assign some arbitrary values to the rest of the registers as well, just in case
                    (RiscVGpRegister.Saved6, T.CreateTruncating(101)),
                    (RiscVGpRegister.Saved7, T.CreateTruncating(0x89ab_cdef)),
                    (RiscVGpRegister.Saved8, T.CreateTruncating(RiscVExecutionTests.K0)),
                    (RiscVGpRegister.Saved9, T.CreateTruncating(RiscVExecutionTests.K1)),
                ];

            FPRInitialization =
                [
                    // F0 - F3: Simple Integers (for CVT.S.W or CVT.D.L tests)
                    (RiscVFloatRegister.Temporary0, float.CreateTruncating(2)),
                    (RiscVFloatRegister.Temporary1, float.CreateTruncating(0)),
                    (RiscVFloatRegister.Temporary2, float.CreateTruncating(10)),
                    (RiscVFloatRegister.Temporary3, float.CreateTruncating((uint)-10)),

                    // F4 - F11: Small "Clean" Floats (Single Precision)
                    // Using values that have exact representations in binary
                    (RiscVFloatRegister.Temporary4, float.CreateTruncating(BitConverter.SingleToUInt32Bits(1.0f))),
                    (RiscVFloatRegister.Temporary5, float.CreateTruncating(BitConverter.SingleToUInt32Bits(2.0f))),
                    (RiscVFloatRegister.Temporary6, float.CreateTruncating(BitConverter.SingleToUInt32Bits(0.5f))),
                    (RiscVFloatRegister.Temporary7, float.CreateTruncating(BitConverter.SingleToUInt32Bits(-2.0f))),
                    (RiscVFloatRegister.Saved0, float.CreateTruncating(BitConverter.SingleToUInt32Bits(10.5f))),
                    (RiscVFloatRegister.Saved1, float.CreateTruncating(BitConverter.SingleToUInt32Bits(2.5f))),
                    (RiscVFloatRegister.Argument0, float.CreateTruncating(BitConverter.SingleToUInt32Bits(1.25f))),
                    (RiscVFloatRegister.Argument1, float.CreateTruncating(BitConverter.SingleToUInt32Bits(-0.75f))),

                    // F12 - F19: Double Precision Pairs (f12/13, f14/15, etc.)
                    // f12/f13 = 1.0, f14/f15 = 0.5, f16/f17 = -2.0, f18/f19 = PI (approx)
                    (RiscVFloatRegister.Argument2, float.CreateTruncating(BitConverter.DoubleToUInt64Bits(2.0))),
                    
                    (RiscVFloatRegister.Argument4, float.CreateTruncating(BitConverter.DoubleToUInt64Bits(0.5))),

                    (RiscVFloatRegister.Argument6, float.CreateTruncating(BitConverter.DoubleToUInt64Bits(-2.0))),

                    (RiscVFloatRegister.Saved2, float.CreateTruncating(BitConverter.DoubleToUInt64Bits(Math.PI))),

                    // F20 - F27: IEEE 754 Edge Cases (Single Precision)
                    (RiscVFloatRegister.Saved4, float.CreateTruncating(BitConverter.SingleToUInt32Bits(float.PositiveInfinity))),
                    (RiscVFloatRegister.Saved5, float.CreateTruncating(BitConverter.SingleToUInt32Bits(float.NegativeInfinity))),
                    (RiscVFloatRegister.Saved6, float.CreateTruncating(BitConverter.SingleToUInt32Bits(float.NaN))),
                    (RiscVFloatRegister.Saved7, float.CreateTruncating(BitConverter.SingleToUInt32Bits(0.0f))),
                    (RiscVFloatRegister.Saved8, float.CreateTruncating(BitConverter.SingleToUInt32Bits(-0.0f))),
                    (RiscVFloatRegister.Saved9, float.CreateTruncating(BitConverter.SingleToUInt32Bits(float.Epsilon))),
                    (RiscVFloatRegister.Saved10, float.CreateTruncating(BitConverter.SingleToUInt32Bits(float.MaxValue))),
                    (RiscVFloatRegister.Saved11, float.CreateTruncating(BitConverter.SingleToUInt32Bits(float.MinValue))),

                    // F28 - F31: Large Integers (to test Rounding/Overflow traps)
                    (RiscVFloatRegister.Temporary8, float.CreateTruncating((uint)int.MaxValue)),
                    (RiscVFloatRegister.Temporary9, float.CreateTruncating(0)), // Upper bits for F28 if treated as Long
                    (RiscVFloatRegister.Temporary10, float.CreateTruncating((uint)int.MinValue)),
                    (RiscVFloatRegister.Temporary11, float.CreateTruncating(0xFFFFFFFF)) // All bits set
                ];

            MemoryInitialization =
                [(T.CreateTruncating(0x100), [0x12, 0x34, 0x56, 0x78])];
        }
    }

    public RiscVEmulatorTestCase(RiscVEmulatorConfig config, string input, RiscVTrap trap) : this(config, input)
    {
        ExpectedTrap = trap;
    }

    public RiscVEmulatorTestCase(RiscVEmulatorConfig config, string input, T writeBack) : this(config, input)
    {
        ExpectedWriteBack = (RiscVGpRegister.Argument0, writeBack);
    }

    public RiscVEmulatorTestCase(RiscVEmulatorConfig config, string input, RiscVGpRegister reg, T? writeBack = null) : this(config, input)
    {
        ExpectedWriteBack = (reg, writeBack);
    }

    public RiscVEmulatorTestCase(RiscVEmulatorConfig config, string input, RiscVFloatRegister reg, float writeBack) : this(config, input)
    {
        ExpectedSingleWriteBack = (reg, writeBack);
    }

    public RiscVEmulatorTestCase(RiscVEmulatorConfig config, string input, RiscVFloatRegister reg, double writeBack) : this(config, input)
    {
        ExpectedDoubleWriteBack = (reg, writeBack);
    }

    public RiscVEmulatorTestCase(RiscVEmulatorConfig config, string input, (T, byte[]) memory) : this(config, input)
    {
        ExpectedMemory = memory;
    }

    public RiscVTrap ExpectedTrap { get; init; } = RiscVTrap.None;

    public T? ExpectedPC { get; init; } = null;

    public RiscVSideEffect? ExpectedSideEffect { get; init; }

    public (RiscVGpRegister Register, T? Value)? ExpectedWriteBack { get; init; } = null;

    public (RiscVFloatRegister Register, float Value)? ExpectedSingleWriteBack { get; init; } = null;

    public (RiscVFloatRegister Register, double Value)? ExpectedDoubleWriteBack { get; init; } = null;

    public (T Address, byte[] Data)? ExpectedMemory { get; init; }

    public (RiscVGpRegister Register, T Value)[] RegisterInitialization { get; init; } = [];

    public (RiscVFloatRegister Register, float Value)[] FPRInitialization { get; init; } = [];

    public (T Address, byte[] Data)[] MemoryInitialization { get; init; } = [];
}
