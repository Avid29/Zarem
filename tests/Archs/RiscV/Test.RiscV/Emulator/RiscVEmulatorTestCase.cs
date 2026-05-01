// Avishai Dernis 2026

using System.Numerics;
using Test.Archs.Emulator;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine.Enums;
using Zarem.Models.Instructions.Enums.Registers;

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

    public RiscVTrap ExpectedTrap { get; init; } = RiscVTrap.None;

    public T? ExpectedPC { get; init; } = null;

    public RiscVSideEffect? ExpectedSideEffect { get; init; }

    public (RiscVGpRegister Register, T? Value)? ExpectedWriteBack { get; init; } = null;

    public (RiscVGpRegister Register, T Value)[] RegisterInitialization { get; init; } = [];
}
