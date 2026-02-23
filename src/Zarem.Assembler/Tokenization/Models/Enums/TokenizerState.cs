// Avishai Dernis 2024

namespace Zarem.Assembler.Tokenization.Models.Enums;

/// <summary>
/// An enum to track the evaluation state of the tokenizer.
/// </summary>
public enum TokenizerState
{
#pragma warning disable CS1591

    // First pass
    TokenBegin,
    TokenBody,
    StringLiteral,
    CharLiteral,
    Comment,
    Whitespace,

    // Second pass
    LineBegin,


#pragma warning restore CS1591
}
