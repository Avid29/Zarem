// Avishai Dernis 2024

namespace Zarem.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum for general process registers.
/// </summary>
public enum GPRegister : byte
{
#pragma warning disable CS1591

    Zero = 0,
    AssemblerTemporary = 1,
    ReturnValue0 = 2,
    ReturnValue1 = 3,
    Argument0 = 4,
    Argument1 = 5,
    Argument2 = 6,
    Argument3 = 7,
    Temporary0 = 8,
    Temporary1 = 9,
    Temporary2 = 10,
    Temporary3 = 11,
    Temporary4 = 12,
    Temporary5 = 13,
    Temporary6 = 14,
    Temporary7 = 15,
    Saved0 = 16,
    Saved1 = 17,
    Saved2 = 18,
    Saved3 = 19,
    Saved4 = 20,
    Saved5 = 21,
    Saved6 = 22,
    Saved7 = 23,
    Temporary8 = 24,
    Temporary9 = 25,
    Kernel0 = 26,
    Kernel1 = 27,
    GlobalPointer = 28,
    StackPointer = 29,
    FramePointer = 30,
    ReturnAddress = 31,

    // Non-indexable registers
    High = 32,
    Low = 33,

#pragma warning restore CS1591
}
