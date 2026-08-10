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
using Zarem.Assembler.Tokenization;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Assembler.Tokenization.Profiles;
using Zarem.Attributes.Arguments;
using Zarem.Models;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Parsers;

/// <summary>
/// A base class for instruction parsers.
/// </summary>
public abstract class InstructionParserBase<TInstruction, TMeta, TArg, TRegister, TSet, TRef>
    where TInstruction : struct
    where TMeta : InstructionMetaBase<TArg>
    where TArg : unmanaged, Enum
    where TRegister : unmanaged, Enum
    where TSet : unmanaged, Enum
    where TRef : unmanaged, Enum
{
    private readonly Dictionary<TArg, AssemblyArg> _argTable;
    private readonly List<RelocationEntry> _references;
    private readonly AssemblerLogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionParserBase{TInstruction, TMeta, TArg, TRegister, TSet, TRef}"/> class.
    /// </summary>
    public InstructionParserBase(Address address, IReadOnlyDictionary<string, Symbol>? symbols, ILogger? logger)
    {
        _argTable = [];
        _references = [];
        ParsedArgTable = [];

        CurrentAddress = address;
        Symbols = symbols;

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
    /// Gets the <see cref="ITokenizerProfile"/> used to tokenize pseudo-instruction templates for expansion.
    /// </summary>
    protected abstract ITokenizerProfile TemplateProfile { get; }

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
    /// Gets the symbol table used for resolving symbols during expression parsing.
    /// </summary>
    protected IReadOnlyDictionary<string, Symbol>? Symbols { get; }

    /// <summary>
    /// Gets a table of parsed arguments.
    /// </summary>
    protected Dictionary<TArg, object?> ParsedArgTable { get; }

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

        // Parse arguments
        Guard.IsNotNull(Meta);
        TArg[] pattern = Meta.ArgumentPattern;
        for (int i = 0; i < line.Args.Count; i++)
        {
            var arg = line.Args[i];
            var type = pattern[i];

            // Empty argument
            if (arg.Tokens.Length is 0)
            {
                var reportToken = arg.ProceedingComma ?? arg.PrecedingComma;
                Guard.IsNotNull(reportToken);
                _logger?.Log(Severity.Error, LogId.InvalidInstructionArg, reportToken, "EmptyArgument");
                continue;
            }

            // Parse the argument
            // Only track the argument if successfully parsed
            if (TryParseArg(arg.Tokens, type))
                _argTable[type] = arg;
        }

        // Handle pseudo-instruction expansion if needed
        if (Meta is IPseudoInstructionMeta pMeta)
        {
            return ParseMetaExpansion(pMeta, out references);
        }

        // If the logger has failed, return null to indicate that parsing failed
        if (_logger?.CurrentFailed is true)
        {
            references = null;
            return null;
        }

        references = _references;
        return [BuildInstruction()];
    }

    /// <summary>
    /// Attempts to populate the <see cref="Meta"/> property by parsing the instruction token of the given line.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Meta))]
    protected abstract bool TryDetermineInstruction(AssemblyLine line, [NotNullWhen(true)] out string? name);

    /// <summary>
    /// Creates a new instruction parser for parsing pseudo-instruction expansion templates.
    /// </summary>
    protected abstract InstructionParserBase<TInstruction, TMeta, TArg, TRegister, TSet, TRef> CreateSubParser(Address address);

    /// <summary>
    /// Builds an instruction from the parsed data.
    /// </summary>
    protected abstract TInstruction BuildInstruction();

    /// <summary>
    /// Gets a parsed argument as a certain type.
    /// </summary>
    protected T GetParsedArgument<T>(TArg arg, params TArg[] alts)
        where T : unmanaged
    {
        if (!ParsedArgTable.TryGetValue(arg, out object? result))
        {
            foreach (var alt in alts)
            {
                if (ParsedArgTable.TryGetValue(alt, out result))
                    break;
            }
        }

        return (T?)result ?? default;
    }

    private bool TryParseArg(ReadOnlySpan<Token> arg, TArg type)
    {
        var attr = ArgumentTable<TArg>.GetAttribute(type);

        return attr switch
        {
            RegisterArgumentAttribute<TSet> reg => TryParseRegister(arg, type, reg),
            ImmediateArgumentAttribute<TRef> imm => TryParseExpression(arg, type, imm),
            SplitArgumentAttribute<TArg> split => TryParseAddressOffset(arg, split.RegisterArgument, split.ImmediateArgument),
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<bool>($"Argument of type '{type}' is not within parsable type range."),
        };
    }

    private bool TryParseRegister(ReadOnlySpan<Token> arg, TArg target, RegisterArgumentAttribute<TSet> attr)
    {
        var bounds = 32; // TODO: Handle ISAs with non-32 reg counts

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
        if (!RegisterTable<TRegister, TSet>.TryGetRegister(token.Source, attr.RegisterSet, out var register, out bool indexed))
        {
            if (RegisterTable<TRegister, TSet>.TryGetRegister(token.Source, out _, out _, out _))
            {
                // The register exists, but is not valid for the argument
                _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, $"RegisterWrongSet", token);
                return false;
            }
            else
            {
                // Register does not exist in table
                _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, token, "RegisterNotFound", token);
                return false;
            }
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

        ParsedArgTable[target] = register;

        return true;
    }

    private bool TryParseExpression(ReadOnlySpan<Token> arg, TArg target, ImmediateArgumentAttribute<TRef> attr)
    {
        // Parse the expression
        if (!ExpressionParser.TryParse<long>(arg, out var expResult, Symbols, _logger?.Parent))
            return false;

        Immediate = expResult.IsAbsolute ? (int)CleanInteger(expResult.Addend, arg, attr.BitCount, attr.ShiftAmount, attr.Signed) : 0;

        // Resolve the relocation type
        var type = attr.DefaultRelocation;
        if (expResult.RelocationType is not null)
        {
            if (Meta is IPseudoInstructionMeta)
            {
                _logger?.Log(Severity.Error, LogId.InvalidRelocatable, arg, "PseudoInstructionExplicitRelocation");
                return false;
            }

            if (!ReferenceTypeTable<TRef>.TryGetReferenceType(expResult.RelocationType, out type))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException($"Relocation type '{expResult.RelocationType}' is not a valid relocation type.");
            }
        }

        // Fetch the relocation type
        // If this fails, it is because the relocation type is none, and no action is needed
        if(ReferenceTypeTable<TRef>.TryGetReferenceType(type, out var refAttr))
        {
            // Vaildate that the relocation type is valid for the argument
            if (attr.BitCount != refAttr.BitCount)
            {
                _logger?.Log(Severity.Error, LogId.InvalidRelocatable, arg, "InvalidRelocationType", expResult.RelocationType, target);
                return false;
            }

            // Create reference entry or adjust immediate
            if (expResult.IsSymbolic)
            {
                _references.Add(new RelocationEntry(expResult.Symbol.Name, CurrentAddress, Unsafe.As<TRef, uint>(ref type), default));
            }
            else
            {
                Immediate >>= refAttr.ShiftAmount;
            }
        }

        return true;
    }

    private bool TryParseAddressOffset(ReadOnlySpan<Token> arg, TArg reg, TArg imm)
    {
        // NOTE: Be careful about forwards to other parse functions with regards to 
        // error logging. Address offset argument errors might be inappropriately logged.

        // Split the string into an offset and a register, return false if failed
        if (!SplitOffsetBase(arg, out var offsetStr, out var regStr))
            return false;

        // Try parse offset component, return false if failed
        if (!TryParseArg(offsetStr, imm))
            return false;

        // Parse register component, return false if failed
        if (!TryParseArg(regStr, reg))
            return false;

        return true;
    }

    private bool SplitOffsetBase(ReadOnlySpan<Token> arg, out ReadOnlySpan<Token> offset, out ReadOnlySpan<Token> register)
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

    private TInstruction[] ParseMetaExpansion(IPseudoInstructionMeta pMeta, out IReadOnlyList<RelocationEntry>? references)
    {
        // Parse each expansion component
        var expansions = new TInstruction[pMeta.Expansion.Length];
        int i = 0;
        foreach (var template in pMeta.Expansion)
        {
            // Parse expansion child component
            var tokenizedLine = ExpandTemplate(template, TemplateProfile);
            var childParser = CreateSubParser(CurrentAddress + (i * 4)); // TODO: Handle different size instructions. THIS IS A HUGE ASSUMPTION
            var parsed = childParser.Parse(tokenizedLine, out var childReferences);

            // Append the component
            Guard.IsNotNull(parsed);
            expansions[i] = parsed[0];

            // Track references
            if (childReferences is not null)
            {
                _references.AddRange(childReferences);
            }

            // Increment
            i++;
        }

        references = _references;
        return expansions;
    }

    /// <summary>
    /// Generates an <see cref="AssemblyLine"/> from a pseudo-instruction substitution template.
    /// </summary>
    private AssemblyLine ExpandTemplate(string template, ITokenizerProfile profile)
    {
        string result = template;

        // Apply substitutions to the template
        foreach (var argType in Enum.GetValues<TArg>())
        {
            // Skip args not present in the arg table
            if (!_argTable.ContainsKey(argType))
                continue;

            // Get the template name for the argument type, which is used as a placeholder in the template string
            var argTemplate = typeof(TArg)
                .GetField($"{argType}")
                ?.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()
                ?.Name;

            // Swap the argument template component for the argument body
            var argTemplatePattern = $"${{{argTemplate}}}";
            var argSubstitution = $"{_argTable[argType]}";
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
    private long CleanInteger(long value, ReadOnlySpan<Token> arg, int bitCount, int shiftAmount, bool signed)
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

        return value;
    }
}
