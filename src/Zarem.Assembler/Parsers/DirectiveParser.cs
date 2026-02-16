// Adam Dernis 2024

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Zarem.Assembler.Config;
using Zarem.Assembler.Extensions;
using Zarem.Assembler.Extensions.System;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models.Directives;
using Zarem.Assembler.Models.Directives.Abstract;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Parsers;

// TODO: Allow repeat with <express> [: <count>] format

/// <summary>
/// A struct for parsing directives.
/// </summary>
public readonly struct DirectiveParser
{
    private readonly IReadOnlyDictionary<string, Symbol>? _symbols;
    private readonly AssemblerConfig _config;
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectiveParser"/> struct.
    /// </summary>
    public DirectiveParser(IReadOnlyDictionary<string, Symbol>? symbols, AssemblerConfig config, ILogger? logger = null)
    {
        _symbols = symbols;
        _config = config;
        _logger = logger;
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
            ".globl" => TryParseGlobal(token, line.Args, out directive),

            // Align or Space
            ".align" => TryParseAlignOrSpace(token, line.Args, out directive, true),
            ".space" => TryParseAlignOrSpace(token, line.Args, out directive, false),

            // Data
            ".word" => TryParseData<int>(token, line.Args, out directive),
            ".half" => TryParseData<short>(token, line.Args, out directive),
            ".byte" => TryParseData<byte>(token, line.Args, out directive),
            ".ascii" => TryParseAscii(line.Args, false, out directive),
            ".asciiz" => TryParseAscii(line.Args, true, out directive),

            // Invalid directive
            _ => false
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
            _logger?.Log(Severity.Error, LogId.InvalidDirectiveArgCount, token, "DirectiveRequiresAnArgument", ".globl");
            return false;
        }

        // Global takes only one argument
        if (args.Count is > 1)
        {
            // TODO: Improve token range message
            _logger?.Log(Severity.Error, LogId.InvalidDirectiveArgCount, args[1].Tokens, "DirectiveTakesOneArgument", ".globl");
            return false;
        }

        if (args[0].Tokens.Length is not 1)
        {
            // TODO: Improve message
            _logger?.Log(Severity.Error, LogId.InvalidDirectiveArg, args[0].Tokens, "DirectiveNonSymbolArgumentSmall", ".globl");
            return false;
        }

        // Get argument
        var arg = args[0].Tokens[0];

        // Global only takes references as an argument
        if (arg.Type is not TokenType.Reference)
        {
            _logger?.Log(Severity.Error, LogId.InvalidDirectiveArg, arg, "DirectiveNonSymbolArgument", ".globl", arg.Source);
        }

        directive = new GlobalDirective(arg.Source);
        return true;
    }

    private bool TryParseAlignOrSpace(Token token, AssemblyLineArgs args, out Directive? directive, bool align)
    {
        directive = null;
        string directiveName = align ? ".align" : ".space";

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
        if (!ExpressionParser.TryParse(args[0].Tokens, out var result, _symbols, _logger))
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
            // if there's no context or threshold value this never executes.
            var alignWarningThreshold = _config.AlignWarningThreshold;
            var alignMessageThreshold = _config.AlignMessageThreshold;
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
            var spaceMessageThreshold = _config.SpaceMessageThreshold;
            if (value >= spaceMessageThreshold)
            {
                _logger?.Log(Severity.Message, LogId.LargeSpacing, args[0].Tokens, "DirectiveLargeAlignMessage", value, spaceMessageThreshold);
            }

            directive = new DataDirective(new byte[value]);
        }
        return true;
    }

    private bool TryParseData<T>(Token name, AssemblyLineArgs args, out Directive? directive)
        where T : unmanaged, IBinaryInteger<T>
    {
        directive = null;

        T value = default;
        int argSize = value.GetByteCount();

        int pos = 0;

        // Allocate space
        var bytes = new byte[args.Count * argSize];

        for (int i = 0; i < args.Count; i++)
        {
            var arg = args[i];

            if (!ExpressionParser.TryParse(arg.Tokens, out var result, _symbols, _logger))
                return false;

            if (result.IsSymbolic)
            {
                // TODO: Can data be a reference to a relocatable address?
                _logger?.Log(Severity.Error, LogId.InvalidDirectiveDataArg, args[0].Tokens, "DirectiveAllocationNoRelocatableArguments", name);
                return false;
            }

            Guard.IsNotNull(result.Addend);
            var resultValue = result.Addend;
            
            // TODO: Double check the logic here. Does this always detect the error?
            value = T.CreateTruncating(resultValue);
            if (value != T.CreateSaturating(resultValue))
            {
                _logger?.Log(Severity.Warning, LogId.IntegerTruncated, arg.Tokens, "DirectiveAllocationTruncated",  arg.Tokens.Print(), result.Addend, value);
            }

            value.WriteBigEndian(bytes, pos);
            pos += argSize;
        }

        directive = new DataDirective(bytes);
        return true;
    }

    private bool TryParseAscii(AssemblyLineArgs args, bool terminate, out Directive? directive)
    {
        directive = null;

        var bytes = new List<byte>();

        for (int i =  0; i < args.Count; i++)
        {
            ReadOnlySpan<Token> arg = args[i].Tokens;

            // TODO: Evaluate expressions
            // Parse string statement to string literal
            if (!StringParser.TryParseString(arg[0], out var value, _logger))
                return false;

            // Copy to byte list
            bytes.Capacity += value.Length;
            bytes.AddRange(value.Select(x => (byte)x));

            // Null terminate string conditionally
            if (terminate)
                bytes.Add(0);
        }

        directive = new DataDirective([..bytes]);
        return true;
    }
}
