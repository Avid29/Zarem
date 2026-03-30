// Avishai Dernis 2024

namespace Zarem.Assembler.Tokenization.Models.Enums;

/// <summary>
/// An enum designating a parse token's type.
/// </summary>
public enum TokenType
{

#pragma warning disable CS1591
    // First pass
    Unknown,
    String,
    Char,
    Comment,
    Whitespace,

    // Second pass
    Instruction,
    RegisterPrefix,
    Register,
    ImmediatePrefix,
    Immediate,
    Directive,
    Operator,

    LabelDeclaration,
    MacroDeclaration,
    Reference,

    OpenParenthesis,
    CloseParenthesis,
    OpenBracket,
    CloseBracket,
    Comma,
    Assign,
    SemiColon,

#pragma warning restore CS1591

}
