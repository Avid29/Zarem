// Avishai Dernis 2024

namespace Zarem.Assembler.Parsers.Expressions.Enums;

/// <summary>
/// An enum for operations in an expression tree.
/// </summary>
public enum Operation
{
#pragma warning disable CS1591

    // Arithmetic
    Addition,
    Subtraction,
    Multiplication,
    Division,
    Modulus,

    // Logical
    And,
    Or,
    Xor,
    LeftShift,
    RightShift,

    // Unary
    UnaryPlus,
    Negation,
    Not,

    // Other
    Function,

#pragma warning restore CS1591
}
