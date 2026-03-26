// Avishai Dernis 2024

namespace Zarem.Assembler.Parsers.Expressions.Enums;

/// <summary>
/// An enum for an expression's type.
/// </summary>
public enum ExpressionType
{
#pragma warning disable CS1591

    Integer,
    Float,
    String,

    /// <summary>
    /// The value of the node is invalid.
    /// </summary>
    Invalid,

#pragma warning restore CS1591
}
