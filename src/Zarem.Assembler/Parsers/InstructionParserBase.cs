// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Zarem.Assembler.Config;
using Zarem.Assembler.Extensions.System;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models.Meta;
using Zarem.Assembler.Models.Tables;
using Zarem.Assembler.Parsers.Enums;
using Zarem.Assembler.Parsers.Expressions;
using Zarem.Assembler.Tokenization;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Assembler.Tokenization.Profiles;
using Zarem.Models;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Parsers;

/// <summary>
/// A base class for instruction parsers.
/// </summary>
public abstract class InstructionParserBase<TInstruction, TMeta, TArg, TRegister, TSet>
    where TInstruction : struct
    where TMeta : InstructionMetaBase<TArg>
    where TArg : unmanaged, Enum
    where TRegister : unmanaged, Enum
    where TSet : unmanaged, Enum
{
    private readonly RegisterTable<TRegister, TSet> _registerTable;
    private readonly AssemblerLogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterParser{TRegister, TSet}"/> struct.
    /// </summary>
    public InstructionParserBase(Address address, IReadOnlyDictionary<string, Symbol>? symbols, RegisterTable<TRegister, TSet> registerTable, ILogger? logger)
    {
        CurrentAddress = address;
        Symbols = symbols;
        _registerTable = registerTable;

        References = [];

        if (logger is not null)
        {
            _logger = new AssemblerLogger(logger);
        }
    }

    /// <summary>
    /// Gets the assembler configuration.
    /// </summary>
    protected abstract AssemblerConfig Config { get; }

    /// <summary>
    /// Gets the list of relocation entries 
    /// </summary>
    protected List<RelocationEntry> References { get; }

    /// <summary>
    /// Gets the current address.
    /// </summary>
    protected Address CurrentAddress { get; }

    /// <summary>
    /// Gets the immediate value component of the instruction, if applicable.
    /// </summary>
    protected int Immediate { get; set; }

    /// <summary>
    /// Gets the metadata for the instruction being parsed, which may be used to guide parsing and template expansion.
    /// </summary>
    protected TMeta? Meta { get; set; }

    /// <summary>
    /// Gets the <see cref="ITokenizerProfile"/> used to tokenize pseudo-instruction templates for expansion.
    /// </summary>
    protected abstract ITokenizerProfile TemplateProfile { get; }

    /// <summary>
    /// Gets the symbol table used for resolving symbols during expression parsing.
    /// </summary>
    protected IReadOnlyDictionary<string, Symbol>? Symbols { get; }

    /// <summary>
    /// Attempts to parse an instruction from a name and a list of arguments.
    /// </summary>
    /// <param name="line">The assembly line to parse.</param>
    /// <param name="references">The list of relocation entries made by references in the instruction.</param>
    /// <returns>The parsed instruction.</returns>
    public TInstruction[]? Parse(AssemblyLine line, out IReadOnlyList<RelocationEntry>? references)
    {
        references = null;

        // Identify the instruction
        if (!TryDetermineInstruction(line, out _))
            return null;

        references = References;

        // Parse arguments
        Guard.IsNotNull(Meta);
        TArg[] pattern = Meta.ArgumentPattern;
        for (int i = 0; i < line.Args.Count; i++)
        {
            var arg = line.Args[i];

            // Empty argument
            if (arg.Tokens.Length is 0)
            {
                var reportToken = arg.ProceedingComma ?? arg.PrecedingComma;
                Guard.IsNotNull(reportToken);
                _logger?.Log(Severity.Error, LogId.InvalidInstructionArg, reportToken, "EmptyArgument");
                continue;
            }

            TryParseArg(arg.Tokens, pattern[i]);
        }

        // Handle pseudo-instruction expansion if needed
        if (Meta is IPseudoInstructionMeta pMeta)
        {
            var expansions = new TInstruction[pMeta.Expansion.Length];
            int i = 0;
            foreach (var template in pMeta.Expansion)
            {
                var tokenizedLine = ExpandTemplate(template, TemplateProfile);
                var childParser = CreateSubParser();
                var parsed = childParser.Parse(tokenizedLine, out references);
                Guard.IsNotNull(parsed);
                expansions[i] = parsed[0];
                i++;
            }

            return expansions;
        }

        return [BuildInstruction()];
    }

    /// <summary>
    /// Attempts to populate the <see cref="Meta"/> property by parsing the instruction token of the given line.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Meta))]
    protected abstract bool TryDetermineInstruction(AssemblyLine line, [NotNullWhen(true)] out string? name);

    /// <summary>
    /// Parses an arg token span according to the expected argument type.
    /// </summary>
    protected abstract bool TryParseArg(ReadOnlySpan<Token> arg, TArg type);

    /// <summary>
    /// Creates a new instruction parser for parsing pseudo-instruction expansion templates.
    /// </summary>
    protected abstract InstructionParserBase<TInstruction, TMeta, TArg, TRegister, TSet> CreateSubParser();

    /// <summary>
    /// Gets the substitution string for a given argument type, which is used to replace placeholders in pseudo-instruction templates.
    /// </summary>
    protected abstract string GetTemplateArgSubstitution(TArg argType);

    /// <summary>
    /// Builds an instruction from the parsed data.
    /// </summary>
    protected abstract TInstruction BuildInstruction();

    /// <summary>
    /// Attempts to parse a register.
    /// </summary>
    /// <param name="arg">The token to parse as a register.</param>
    /// <param name="register">The resulting register.</param>
    /// <param name="set">The register set the register needs to belong ot.</param>
    /// <param name="bounds">The bounds of a numerical register in the set.</param>
    /// <returns>Whether or not a register was successfuly parsed.</returns>
    protected bool TryParseRegister(ReadOnlySpan<Token> arg, out TRegister register, TSet set, int bounds)
    {
        register = default;

        if (arg.Length is not 1)
        {
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, "ArgumentNotARegister", arg.Print());
            return false;
        }

        // Check that argument is register argument
        var token = arg[0];
        if (token.Type is not TokenType.Register)
        {
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, token, "ArgumentNotARegister", token);
            return false;
        }

        // Get named register from table
        if (!_registerTable.TryGetRegister(token.Source, out register, out TSet parsedSet, out bool indexed))
        {
            // Register does not exist in table
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, token, "RegisterNotFound", token);
            return false;
        }

        var index = Unsafe.As<TRegister, int>(ref register);
        if (index >= bounds)
        {
            var (message, msgArg) = indexed switch
            {
                true => ("RegisterNumberNotFound", (object)index),
                false => ("RegisterNotIndexable", token)
            };

            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, message, msgArg);
            return false;
        }

        // Match register set
        if (!EqualityComparer<TSet>.Default.Equals(parsedSet, default) && !EqualityComparer<TSet>.Default.Equals(parsedSet, set))
        {
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, $"RegisterWrongSet", token);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Attempts to parse an expression.
    /// </summary>
    protected bool TryParseExpression(ReadOnlySpan<Token> tokens, int bitCount, int shift, bool signed, out ExpressionResult<long> expResult)
    {
        if (!ExpressionParser.TryParse(tokens, out expResult, Symbols, _logger?.Parent))
            return false;

        long val = expResult.IsAbsolute ? expResult.Addend : 0;
        CleanInteger(ref val, tokens, bitCount, shift, signed);
        Immediate = (int)val;
        return true;
    }

    /// <summary>
    /// Splits an address offset argument into a token span for the offset and the address register token.
    /// </summary>
    /// <remarks>
    /// Upon return offset and register do not need to be valid offset and register strings.
    /// The register is just the component in parenthesis. The offset is just the component before the parenthesis.
    /// Nothing may follow the parenthesis.
    /// </remarks>
    protected bool SplitOffsetBase(ReadOnlySpan<Token> arg, out ReadOnlySpan<Token> offset, out ReadOnlySpan<Token> register)
    {
        offset = arg;
        register = [];

        // Find matched parenthesis start and end
        var parIndex = arg.FindLast(TokenType.OpenParenthesis, out _);
        var closeIndex = arg.FindLast(TokenType.CloseParenthesis, out _);
        if (parIndex is -1 || closeIndex is -1)
        {
            // TODO: Improve messaging
            _logger?.Log(Severity.Error, LogId.InvalidAddressOffsetArgument, arg, "InvalidAddressOffsetArgument", arg.Print());
            return false;
        }

        // Offset is everything before the parenthesis
        offset = arg[..parIndex];

        // Register is everything between the parenthesis
        register = arg[(parIndex + 1)..closeIndex];

        // Ensure there's no content following the parenthesis.
        if (!arg[(closeIndex + 1)..].IsEmpty)
        {
            // TODO: Improve messaging
            _logger?.Log(Severity.Error, LogId.InvalidAddressOffsetArgument, arg, "InvalidAddressOffsetArgument", arg.Print());
            return false;
        }

        return true;
    }

    /// <summary>
    /// Generates an <see cref="AssemblyLine"/> from a pseudo-instruction substitution template.
    /// </summary>
    protected AssemblyLine ExpandTemplate(string template, ITokenizerProfile profile)
    {
        string result = template;

        // Apply substitutions to the template
        foreach (var argType in Enum.GetValues<TArg>())
        {
            // Get the template name for the argument type, which is used as a placeholder in the template string
            var argTemplate = typeof(TArg)
                .GetField($"{argType}")
                ?.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()
                ?.Name;
            var argTemplatePattern = $"${{{argTemplate}}}";

            var argSubstitution = GetTemplateArgSubstitution(argType);

            Guard.IsNotNull(argTemplatePattern);
            result = result.Replace(argTemplatePattern, argSubstitution);
        }

        // Tokenize the resulting template string
        return Tokenizer.TokenizeLine(result, profile)[0];
    }

    /// <summary>
    /// Cleans a value to a specified bit count and shift amount, while also checking for any changes that occured during the cast.
    /// </summary>
    /// <remarks>
    /// This does not apply the <paramref name="shiftAmount"/>, only masks the lower bits.
    /// </remarks>
    /// <param name="value">A reference to the integer to modify.</param>
    /// <param name="arg">The original tokens of the argument.</param>
    /// <param name="bitCount">The number of bits after casting.</param>
    /// <param name="shiftAmount">The number of bits that will drop from the bottom.</param>
    /// <param name="signed">Whether or not the new value should be signed.</param>
    /// <returns>Whether or not the value can be safely cast.</returns>
    private void CleanInteger(ref long value, ReadOnlySpan<Token> arg, int bitCount, int shiftAmount, bool signed)
    {
        var original = value;

        Guard.IsGreaterThan(bitCount, 1);
        Guard.IsLessThanOrEqualTo(bitCount + shiftAmount, 64);

        // Create a masks for the high and low truncating bits,
        // as well as an overall remaining bits map
        var upperMask = bitCount == 64 ? -1L : (1L << (bitCount + shiftAmount)) - 1;
        var lowerMask = ~((1L << shiftAmount) - 1);
        var mask = upperMask & lowerMask;

        // Truncate mask upper and lower bits
        long truncated = value & mask;

        // Sign extend if signed and not full width
        if (signed && bitCount < 64)
        {
            long signBit = 1L << (bitCount - 1);
            if ((truncated & signBit) != 0)
                truncated |= ~upperMask; // Sign extend
        }

        value = truncated;

        // Compute changes
        var changes = CastingChanges.None;

        // Check if the sign was dropped
        if (!signed && original < 0)
            changes |= CastingChanges.SignChanged;

        // Check for upper truncation
        long upperBits = original & ~upperMask;
        if (upperBits != 0 && upperBits != ~upperMask)
            changes |= CastingChanges.TruncatedHigh;

        // Check for lower truncation
        if ((original & ~lowerMask) != 0)
        {
            changes |= CastingChanges.TruncatedLow;
        }

        // Clean integer to fit within argument bit size and match signs
        // Log a message if the value was truncated and/or had its sign changed
        if (changes is not CastingChanges.None)
        {
            _logger?.Log(Severity.Warning, LogId.IntegerTruncated, arg, $"CastWarning{changes}", arg.Print(), original, value, bitCount, shiftAmount);
        }
    }
}
