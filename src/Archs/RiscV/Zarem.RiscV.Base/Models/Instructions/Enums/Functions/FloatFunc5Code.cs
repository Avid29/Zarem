// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum for the float-point function code field of RISC-V floating-point instructions.
/// </summary>
public enum FloatFunc5Code : byte
{
#pragma warning disable CS1591
    Add = 0,
    Subtract = 4,
    Multiply = 8,
    Divide = 12,
    SignInject = 16,
    MinMax = 20,
    SquareRoot = 44,
    Compare = 80,
    ConvertInt = 96,
    ConvertFloat = 112,
    MoveXToF = 120,
    MoveFToX = 112,
    Classify = 112
#pragma warning restore CS1591
}
