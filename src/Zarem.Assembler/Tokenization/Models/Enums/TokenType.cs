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
    Register,
    Immediate,
    Directive,
    Operator,

    LabelDeclaration,
    MacroDeclaration,
    Reference,          // This could be either a label or a macro

    OpenParenthesis,
    CloseParenthesis,
    OpenBracket,
    CloseBracket,
    Comma,
    Assign,

#pragma warning restore CS1591

}
