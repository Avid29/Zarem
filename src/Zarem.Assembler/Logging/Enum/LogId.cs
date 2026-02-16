// Adam Dernis 2024

namespace Zarem.Assembler.Logging.Enum;

/// <summary>
/// An id for types of logs.
/// </summary>
public enum LogId : uint
{
#pragma warning disable CS1591

    // General
    IllegalSymbolName,
    UnexpectedToken,
    DuplicateSymbolDefinition,
    DisabledFeatureInUse,
    NotInVersion,

    // Macros
    MacroMissingValue,
    MacroCannotBeRelocatable,

    // Instruction parser 
    InvalidInstructionName,
    InvalidInstructionArg,
    InvalidInstructionArgCount,
    InvalidRegisterArgument,
    InvalidAddressOffsetArgument,
    InvalidFloatFormat,
    BranchBetweenSections,
    ExternalBranching,
    IntegerTruncated,
    RelocatableReferenceInShift,
    ZeroRegWriteback,

    // Expression parser
    UnparsableExpression,
    InvalidExpressionOperation,
    UndeclaredSymbolReferenced,

    // Directive parser
    InvalidDirectiveDataArg,
    InvalidDirectiveArg,
    InvalidDirectiveArgCount,
    LargeAlignment,
    LargeSpacing,

    // Char/String parsing
    NotAString,
    IncompleteString,
    InvalidCharLiteral,
    IncompleteEscapeSequence,
    UnrecognizedEscapeSequence,
    UnescapedQuoteInString,

    // Linker Errors
    FailedToLoadModule,

#pragma warning restore CS1591
}
