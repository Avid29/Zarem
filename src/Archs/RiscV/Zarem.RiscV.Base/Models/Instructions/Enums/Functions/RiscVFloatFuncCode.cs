// Avishai Dernis 2026

namespace Zarem.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum for the float-point function code field of RISC-V floating-point instructions.
/// </summary>
public enum RiscVFloatFuncCode : byte
{

#pragma warning disable CS1591

    Add = 0,
    Subtract = 1,
    Multiply = 2,
    Divide = 3,
    SquareRoot = 4,
    MinMax = 5,

#pragma warning restore CS1591
}
