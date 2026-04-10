// Avishai Dernis 2026

namespace Zarem.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum representing the general-purpose registers in RISC-V.
/// </summary>
public enum RiscVGpRegister : byte
{
#pragma warning disable CS1591

    Zero = 0,
    ReturnAddress = 1,
    StackPointer = 2,
    GlobalPointer = 3,
    ThreadPointer = 4,
    Temporary0 = 5,
    Temporary1 = 6,
    Temporary2 = 7,
    Saved0 = 8,
    FramePointer = 8,
    Saved1 = 9,
    Argument0 = 10,
    Argument1 = 11,
    Argument2 = 12,
    Argument3 = 13,
    Argument4 = 14,
    Argument5 = 15,
    Argument6 = 16,
    Argument7 = 17,
    Saved2 = 18,
    Saved3 = 19,
    Saved4 = 20,
    Saved5 = 21,
    Saved6 = 22,
    Saved7 = 23,
    Saved8 = 24,
    Saved9 = 25,
    Saved10 = 26,
    Saved11 = 27,
    Temporary3 = 28,
    Temporary4 = 29,
    Temporary5 = 30,
    Temporary6 = 31

#pragma warning restore CS1591
}
