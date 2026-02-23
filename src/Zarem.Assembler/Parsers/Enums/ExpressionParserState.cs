// Avishai Dernis 2024

namespace Zarem.Assembler.Parsers.Enums;

/// <summary>
/// An enum for tracking the state of the expression parser.
/// </summary>
public enum ExpressionParserState
{
#pragma warning disable CS1591

    Start,
    Immediate,
    Operator,
    Reference,

#pragma warning restore CS1591
}
