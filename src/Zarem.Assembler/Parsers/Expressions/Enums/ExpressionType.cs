// Avishai Dernis 2024

namespace Zarem.Assembler.Parsers.Expressions.Enums;

/// <summary>
/// An enum for an expression's type.
/// </summary>
public enum ExpressionType
{
#pragma warning disable CS1591

    /// <remarks>
    /// Any binary integer type. Such as <see langword="char"/>, <see langword="int"/>, etc
    /// </remarks>
    Integer,
    String,

    /// <summary>
    /// The value of the node is invalid.
    /// </summary>
    Invalid,

#pragma warning restore CS1591
}
