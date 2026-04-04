// Avishai Dernis 2026

using System.Numerics;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions.Enums.Registers;

namespace Test.RiscV.Emulator;

public sealed record ExecutionTestCase<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
{
    public ExecutionTestCase(string input)
    {
        Input = input;

        unchecked
        {
            RegisterInitialization =
                [
                    // Max/Min values to test edge cases, as well as some arbitrary non-edge-case values for good measure
                    // Stored in the argument registers
                    (GPRegister.Argument0, T.CreateTruncating(int.MaxValue)),
                    (GPRegister.Argument1, T.CreateTruncating(int.MinValue)),
                    (GPRegister.Argument2, T.CreateTruncating(uint.MaxValue)),
                    (GPRegister.Argument3, T.CreateTruncating(uint.MinValue)),

                    // Saved 0 - 3 are assigned to 1 through 4 respectively,
                    // while saved 4 and 5 are assigned to -1 and -2 (to test sign handling in arithmetic instructions)
                    (GPRegister.Saved0, T.CreateTruncating(1)),
                    (GPRegister.Saved1, T.CreateTruncating(2)),
                    (GPRegister.Saved2, T.CreateTruncating(3)),
                    (GPRegister.Saved3, T.CreateTruncating(4)),
                    (GPRegister.Saved4, T.CreateTruncating(-1)),
                    (GPRegister.Saved5, T.CreateTruncating(-2)),

                    // Temp 1 - 4 are assigned to 10, 20, 30, 40 respectively,
                    // while temp 5 and 6 are assigned to -10 and -20 (to test sign handling in arithmetic instructions)
                    (GPRegister.Temporary0, T.CreateTruncating(10)),
                    (GPRegister.Temporary1, T.CreateTruncating(20)),
                    (GPRegister.Temporary2, T.CreateTruncating(30)),
                    (GPRegister.Temporary3, T.CreateTruncating(40)),
                    (GPRegister.Temporary4, T.CreateTruncating(-10)),
                    (GPRegister.Temporary5, T.CreateTruncating(-20)),
                    (GPRegister.Temporary6, T.CreateTruncating(-30)),

                    // Assign some arbitrary values to the rest of the registers as well, just in case
                    (GPRegister.Saved6, T.CreateTruncating(101)),
                    (GPRegister.Saved7, T.CreateTruncating(0x89ab_cdef)),
                    (GPRegister.Saved8, T.CreateTruncating(ExecutionTests.K0)),
                    (GPRegister.Saved9, T.CreateTruncating(ExecutionTests.K1)),
                ];
        }
    }

    public ExecutionTestCase(string input, T writeBack) : this(input)
    {
        ExpectedWriteBack = (GPRegister.Argument0, writeBack);
    }

    public ExecutionTestCase(string input, GPRegister reg, T? writeBack = null) : this(input)
    {
        ExpectedWriteBack = (reg, writeBack);
    }

    public string Input { get; }

    public RiscVTrap ExpectedTrap { get; init; } = RiscVTrap.None;

    public (GPRegister Regiter, T? Value)? ExpectedWriteBack { get; init; } = null;

    public (GPRegister Register, T Value)[] RegisterInitialization { get; init; } = [];
}
