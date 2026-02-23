// Avishai Dernis 2024

using CommunityToolkit.Diagnostics;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Assembler.Parsers;

/// <summary>
/// Parses string statements into string literals.
/// </summary>
public ref struct StringParser
{
    private readonly AssemblerLogger? _logger;
    private readonly Token _token;
    private bool _escapeState;

    private StringParser(Token token, ILogger? logger)
    {
        _token = token;
        _escapeState = false;

        if (logger is not null)
        {
            _logger = new AssemblerLogger(logger);
        }
    }

    /// <summary>
    /// Attempts to parse a literal string from a string statement.
    /// </summary>
    /// <param name="token">The string statement.</param>
    /// <param name="literal">The resulting string literal</param>
    /// <param name="logger">The logger to use when tracking errors.</param>
    /// <returns><see cref="true"/> if the string was successfully parsed. <see cref="false"/> otherwise.</returns>
    public static bool TryParseString(Token token, out string literal, ILogger? logger)
    {
        var parser = new StringParser(token, logger);
        return parser.TryParse(token, '"', out literal);
    }

    /// <summary>
    /// Attempts to parse a single character from a char statement.
    /// </summary>
    /// <param name="token">The char statement.</param>
    /// <param name="c">The char literal.</param>
    /// <param name="logger">The logger to use when tracking errors.</param>
    /// <returns>Whether or not a character was successfully parsed.</returns>
    public static bool TryParseChar(Token token, out char c, ILogger? logger)
    {
        c = default;
        var parser = new StringParser(token, logger);

        if (!parser.TryParse(token, '\'', out string literal))
            return false;

        if (literal.Length != 1)
        {
            parser._logger?.Log(Severity.Error, LogId.InvalidCharLiteral, token, "MustBeSingleCharacter");
            return false;
        }

        c = literal[0];
        return true;
    }

    private bool TryParse(Token token, char wrap, out string literal)
    {
        literal = string.Empty;
        _escapeState = false;

        // Trim whitespace
        var input = token.Source.Trim();

        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Ensure string begins and ends with quotes
        if (input[0] != wrap || input[^1] != wrap || input.Length is 1)
        {
            string expected = wrap switch
            {
                '"' => "String",
                '\'' => "Char",
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>($"{wrap} expected to be either a ''' or '\"' character."),
            };

            // TODO: Improve message
            _logger?.Log(Severity.Error, LogId.IncompleteString, _token, $"Incomplete{expected}");
            return false;
        }

        foreach (char c in input[1..^1])
        {
            if (_escapeState)
            {
                if (!TryParseCharFromEscape(c, ref literal))
                    return false;
            }
            else
            {
                if (!TryParseByChar(c, ref literal))
                    return false;
            }
        }

        if (_escapeState)
        {
            _logger?.Log(Severity.Error, LogId.IncompleteEscapeSequence, _token, "IncompleteEscapeSequence");
            return false;
        }

        return true;
    }

    private bool TryParseByChar(char c, ref string literal)
    {
        switch (c)
        {
            case '"':
                _logger?.Log(Severity.Error, LogId.UnescapedQuoteInString, _token, "UnescapedQuoteInString");
                return false;
            case '\\':
                _escapeState = true;
                break;
            default:
                literal += c;
                break;
        }
        return true;
    }

    private bool TryParseCharFromEscape(char c, ref string literal)
    {
        char e = c switch
        {
            'a' => '\a',
            'b' => '\b',
            'f' => '\f',
            'n' => '\n',
            'r' => '\r',
            't' => '\t',
            'v' => '\v',
            '\\' => '\\',
            '\'' => '\'',
            '"' => '\"',
            _ => (char)0,
        };

        if (e == (char)0)
        {
            _logger?.Log(Severity.Error, LogId.UnrecognizedEscapeSequence, _token, "UnrecognizedEscapeSequence", @$"\{c}");
            return false;
        }

        literal += e;

        _escapeState = false;
        return true;
    }
}
