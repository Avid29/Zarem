// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum for the float-point function code field of RISC-V floating-point instructions.
/// </summary>
public enum FloatFunc5Code : byte
{
#pragma warning disable CS1591
    Add = 0,
    Subtract = 1,
    Multiply = 2,
    Divide = 3,
    SignInject = 4,
    MinMax = 5,
    SquareRoot = 11,
    Compare = 20,
    ConvertToInt = 24,
    ConvertToFloat = 28,
    MoveXToF = 28,
    Classify = 28,
    MoveFToX = 30,
#pragma warning restore CS1591
}
