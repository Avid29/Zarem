// Avishai Dernis 2024

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Zarem.Assembler.Config;
using Zarem.Assembler.Extensions.System;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models.Directives;
using Zarem.Assembler.Models.Directives.Abstract;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Models.Enums;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Parsers;

// TODO: Allow repeat with <express> [: <count>] format

/// <summary>
/// A struct for parsing directives.
/// </summary>
public unsafe readonly struct DirectiveParser
{
    private readonly IReadOnlyDictionary<string, Symbol>? _symbols;
    private readonly AssemblerConfig? _config;
    private readonly AssemblerLogger? _logger;
    private readonly Endianness _endianness;
    private readonly bool _realize;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectiveParser"/> struct.
    /// </summary>
    public DirectiveParser(Endianness endianness) : this(endianness, null, null, null, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectiveParser"/> struct.
    /// </summary>
    public DirectiveParser(Endianness endianness, IReadOnlyDictionary<string, Symbol>? symbols, AssemblerConfig? config, ILogger? logger, bool realize)
    {
        _symbols = symbols;
        _config = config;
        _endianness = endianness;
        _realize = realize;

        if (_realize && logger is not null)
        {
            _logger = new AssemblerLogger(logger);
        }
    }

    /// <summary>
    /// Parses a directive from a name and a list of arguments.
    /// </summary>
    /// <param name="line">The assembly line.</param>
    /// <param name="directive">The <see cref="Directive"/>.</param>
    /// <returns>Whether or not an directive was parsed.</returns>
    public bool TryParseDirective(AssemblyLine line, out Directive? directive)
    {
        directive = null;

        Guard.IsNotNull(line.Directive);
        var token = line.Directive;

        return token.Source switch
        {
            // Section directives
            ".text" => TryParseSection(token, line.Args, out directive),
            ".data" => TryParseSection(token, line.Args, out directive),

            // Global References
            ".globl" or ".global" => TryParseGlobal(token, line.Args, out directive),

            // Define
            ".def" or ".define" => TryParseDefine(token, line.Args, out directive),

            // Align or Space
            ".align" => TryParseAlignOrSpace(token, line.Args, out directive, true),
            ".space" => TryParseAlignOrSpace(token, line.Args, out directive, false),

            // Data
            ".word" => TryParseInteger<int>(token, line.Args, out directive),
            ".half" => TryParseInteger<short>(token, line.Args, out directive),
            ".byte" => TryParseInteger<byte>(token, line.Args, out directive),
            ".float" => TryParseFloat<float>(token, line.Args, out directive),
            ".double" => TryParseFloat<double>(token, line.Args, out directive),

            // String
            ".ascii" => TryParseString(line.Args, Encoding.ASCII, false, out directive),
            ".asciiz" => TryParseString(line.Args, Encoding.ASCII, true, out directive),
            ".utf8" => TryParseString(line.Args, Encoding.UTF8, false, out directive),
            ".utf8z" => TryParseString(line.Args, Encoding.UTF8, true, out directive),
            ".unicode" or ".utf16" => TryParseString(line.Args, _endianness is Endianness.Big ? Encoding.BigEndianUnicode : Encoding.Unicode, false, out directive),
            ".unicodez" or ".utf16z" => TryParseString(line.Args, _endianness is Endianness.Big ? Encoding.BigEndianUnicode : Encoding.Unicode, true, out directive),

            // Invalid directive
            _ => _logger?.Log(Severity.Error, LogId.InvalidDirectiveName, token, "DirectiveDoesNotExist", token.Source) ?? false,
        };
    }

    private bool TryParseSection(Token name, AssemblyLineArgs args, out Directive? directive)
    {
        directive = null;

        string sectionName = name.Source;
        if (args.Count is not 0)
        {
            _logger?.Log(Severity.Error, LogId.InvalidDirectiveArgCount, name, "DirectiveTakesNoArguments", sectionName);
            return false;
        }

        directive = new SectionDirective(sectionName);
        return true;
    }

    private bool TryParseGlobal(Token token, AssemblyLineArgs args, out Directive? directive)
    {
        // TODO: Can you declare multiple globals on one line?
        directive = null;

        // Global requires an argument
        if (args.Count is 0)
        {
            _logger?.Log(Severity.Error, LogId.InvalidDirectiveArgCount, token, "DirectiveRequiresAnArgument", token.Source);
            return false;
        }

        // Global takes only one argument
        if (args.Count is > 1)
        {
            // TODO: Improve token range message
            _logger?.Log(Severity.Error, LogId.InvalidDirectiveArgCount, args[1].Tokens, "DirectiveTakesOneArgument", token.Source);
            return false;
        }

        if (args[0].Tokens.Length is not 1)
        {
            // TODO: Improve message
            _logger?.Log(Severity.Error, LogId.InvalidDirectiveArg, args[0].Tokens, "DirectiveNonSymbolArgumentSmall", token.Source);
            return false;
        }

        // Get argument
        var arg = args[0].Tokens[0];

        // Global only takes references as an argument
        if (arg.Type is not TokenType.Reference)
        {
            _logger?.Log(Severity.Error, LogId.InvalidDirectiveArg, arg, "DirectiveNonSymbolArgument", token.Source, arg.Source);
        }

        directive = new GlobalDirective(arg.Source);
        return true;
    }

    private bool TryParseAlignOrSpace(Token token, AssemblyLineArgs args, out Directive? directive, bool align)
    {
        directive = null;
        string directiveName = token.Source;

        // Space and Align require an argument
        if (args.Count is 0)
        {
            _logger?.Log(Severity.Error, LogId.InvalidDirectiveArgCount, token, "DirectiveRequiresAnArgument", directiveName);
            return false;
        }
        
        // Space and Align take only one argument
        if (args.Count is > 1)
        {
            _logger?.Log(Severity.Error, LogId.InvalidDirectiveArgCount, args[1].Tokens, "DirectiveTakesOneArgument", directiveName);
            return false;
        }

        // Parse argument
        if (!ExpressionParser.TryParse<long>(args[0].Tokens, out var result, _symbols, _logger?.Parent))
            return false;

        // Argument must not be relocatable
        if (result.IsSymbolic)
        {
            _logger?.Log(Severity.Error, LogId.InvalidDirectiveArg, args[0].Tokens, "DirectiveNoRelocatableArguments", directiveName);
            return false;
        }

        // Result should not be null
        Guard.IsNotNull(result.Addend);

        var value = result.Addend;

        if (align)
        {
            // Kinda unique behavior here warrants a comment.
            // Any int? comparison operator involving a null returns false, so
            // if there's no config or threshold value this never executes.
            var alignWarningThreshold = _config?.AlignWarningThreshold;
            var alignMessageThreshold = _config?.AlignMessageThreshold;
            if (value >= alignWarningThreshold)
            {
                _logger?.Log(Severity.Warning, LogId.LargeAlignment, args[0].Tokens, "DirectiveLargeAlignWarning", value, alignWarningThreshold);
            }
            else if (value >= alignMessageThreshold)
            {
                _logger?.Log(Severity.Message, LogId.LargeAlignment, args[0].Tokens, "DirectiveLargeAlignMessage", value, alignMessageThreshold);
            }

            directive = new AlignDirective((uint)value);
        }
        else
        {
            var spaceMessageThreshold = _config?.SpaceMessageThreshold;
            if (value >= spaceMessageThreshold)
            {
                _logger?.Log(Severity.Message, LogId.LargeSpacing, args[0].Tokens, "DirectiveLargeAlignMessage", value, spaceMessageThreshold);
            }

            directive = new DataDirective(new byte[value]);
        }
        return true;
    }

    private bool TryParseInteger<T>(Token name, AssemblyLineArgs args, out Directive? directive)
        where T : unmanaged, IBinaryInteger<T>
    {
        directive = null;

        // Allocate space
        int pos = 0;
        int argSize = sizeof(T);
        var bytes = new byte[args.Count * argSize];

        if (_realize)
        {
            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];

                if (!ExpressionParser.TryParse<long>(arg.Tokens, out var result, _symbols, _logger?.Parent))
                    return false;

                Guard.IsNotNull(result.Addend);
                var resultValue = result.Addend;

                // TODO: Double check the logic here. Does this always detect the error?
                T value = T.CreateTruncating(resultValue);
                if (value != T.CreateSaturating(resultValue))
                {
                    _logger?.Log(Severity.Warning, LogId.IntegerTruncated, arg.Tokens, "DirectiveAllocationTruncated", arg.Tokens.Print(), result.Addend, value);
                }

                if (_endianness is Endianness.Big)
                {
                    value.WriteBigEndian(bytes, pos);
                }
                else
                {
                    value.WriteLittleEndian(bytes, pos);
                }
                pos += argSize;
            }
        }

        directive = new DataDirective(bytes);
        return true;
    }

    private bool TryParseFloat<T>(Token name, AssemblyLineArgs args, out Directive? directive)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        directive = null;

        int argSize = sizeof(T);
        int pos = 0;
        var bytes = new byte[args.Count * argSize];

        for (int i = 0; i < args.Count; i++)
        {
            var arg = args[i];

            if (!ExpressionParser.TryParse<double>(arg.Tokens, out var result, _symbols, _logger?.Parent))
                return false;

            if (result.IsSymbolic)
            {
                _logger?.Log(Severity.Error, LogId.InvalidDirectiveDataArg, args[0].Tokens, "DirectiveAllocationNoRelocatableArguments", name);
                return false;
            }

            Guard.IsNotNull(result.Addend);

            T value = T.CreateTruncating(result.Addend);

            // Precision check
            if (double.CreateTruncating(value) != (double)result.Addend)
            {
                _logger?.Log(Severity.Warning, LogId.IntegerTruncated, arg.Tokens, "DirectiveAllocationTruncated", arg.Tokens.Print(), result.Addend, value);
            }

            // Write the raw memory representation of 'value' into the byte array
            Span<byte> destination = bytes.AsSpan(pos, argSize);
            MemoryMarshal.Write(destination, in value);

            // If the host endianness does not match the desired endianness, flip reverse the bytes
            if (BitConverter.IsLittleEndian == _endianness is Endianness.Big)
                destination.Reverse();

            pos += argSize;
        }

        directive = new DataDirective(bytes);
        return true;
    }

    private bool TryParseString(AssemblyLineArgs args, Encoding encoding, bool terminate, out Directive? directive)
    {
        directive = null;

        var bytes = new List<byte>();

        for (int i =  0; i < args.Count; i++)
        {
            ReadOnlySpan<Token> arg = args[i].Tokens;

            // TODO: Evaluate expressions
            // Parse string statement to string literal
            if (!StringParser.TryParseString(arg[0], out var value, _logger?.Parent))
                return false;

            // Encode the string
            var encoded = encoding.GetBytes(value);

            // Copy to byte list
            bytes.Capacity += encoded.Length;
            bytes.AddRange(encoded);

            // Null terminate string conditionally
            if (terminate)
            {
                bytes.AddRange(encoding.GetBytes("\0"));
            }
        }

        directive = new DataDirective([..bytes]);
        return true;
    }

    private bool TryParseDefine(Token name, AssemblyLineArgs args, out Directive? directive)
    {
        directive = null;

        if (args.Count is 0)
        {
            _logger?.Log(Severity.Error, LogId.InvalidDirectiveArgCount, name, "DirectiveRequiresAnArgument", name.Source);
            return false;
        }

        if (args.Count is > 1)
        {
            var proceedingComma = args[0].ProceedingComma;
            Guard.IsNotNull(proceedingComma);
            _logger?.Log(Severity.Error, LogId.UnexpectedToken, proceedingComma, "UnexpectedToken", name.Source);
            return false;
        }

        var arg = args[0];
        if (arg.Tokens.Length is 0)
        {
            var comma = args[0].ProceedingComma;
            Guard.IsNotNull(comma);
            _logger?.Log(Severity.Error, LogId.UnexpectedToken, comma, "EmptyArgument");
            return false;
        }

        var nameToken = arg.Tokens[0];
        var valueTokens = arg.Tokens[1 ..];
        if (!ExpressionParser.TryParse<long>(valueTokens, out var result, _symbols, _logger?.Parent))
            return false;

        if (result.IsSymbolic)
        {
            _logger?.Log(Severity.Error, LogId.InvalidRelocatable, result.SymbolNode.ExpressionToken, "DirectiveNoRelocatableArguments", name);
            return false;
        }

        directive = new DefineDirective(nameToken, result.Addend);
        return true;
    }
}
