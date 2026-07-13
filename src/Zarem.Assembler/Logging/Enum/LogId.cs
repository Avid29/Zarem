// Avishai Dernis 2024

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
    Other,

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
    InvalidRelocatable,
    InvalidRelocationType,
    ZeroRegWriteback,

    // Expression parser
    UnparsableExpression,
    InvalidExpressionOperation,
    UndeclaredSymbolReferenced,
    InvalidCast,

    // Directive parser
    InvalidDirectiveName,
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
    WrongArchitecture,
    UndefinedSymbol,
    OutOfRange,
    MissingEntryPoint,

#pragma warning restore CS1591
}
