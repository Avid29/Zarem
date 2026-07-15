// Avishai Dernis 2025

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Zarem.Assembler.Extensions;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;

namespace Zarem.Assembler.Tokenization;

public partial class Tokenizer
{
    private bool ReTokenizeLine(List<Token> raw, out List<AssemblyLine> lines)
    {
        // Prepare for token retokenization
        _state = TokenizerState.LineBegin;
        List<Token> classified = [];
        lines = [];

        if (_mode is TokenizerMode.BehaviorExpression or TokenizerMode.Expression)
            _state = TokenizerState.ArgumentPhase;

        // Reclassify each token
        var span = CollectionsMarshal.AsSpan(raw);
        while (!span.IsEmpty)
        {
            var newToken = ReTokenizeSpan(span, out var advance);

            // TODO: Handle invalid state
            if (newToken is null)
                return false;

            // Track if the new token is meaningful
            bool meaningful = newToken.Type is not
                (TokenType.Comment or TokenType.Whitespace or
                TokenType.RegisterPrefix or TokenType.ImmediatePrefix or TokenType.RelocationPrefix);

            if (meaningful || _mode is TokenizerMode.IDE or TokenizerMode.BehaviorExpression)
                classified.Add(newToken);

            // Begin a new assembly line if we see a semi-colon
            if (newToken.Type is TokenType.SemiColon)
            {
                // Append the current line
                lines.Add(new AssemblyLine([.. classified]));

                // Begin new assembly line
                classified = [];
                _state = TokenizerState.LineBegin;
            }
            else if (meaningful && newToken.Type is not (TokenType.LabelDeclaration or TokenType.RegisterPrefix or TokenType.ImmediatePrefix or TokenType.RelocationPrefix))
            {
                // If a meaningful token was appended, we are no longer at the start. 
                // we also remain at the start after label declarations
                _state = TokenizerState.ArgumentPhase;
            }

            // Advance the appropriate number of tokens
            span = span[advance..];
        }

        lines.Add(new AssemblyLine([.. classified]));

        return true;
    }

    private Token? ReTokenizeSpan(ReadOnlySpan<Token> tokens, out int advance)
    {
        var current = tokens[0];

        // Handle basic reclassification
        if (CheckPreclassified(current, out var result, out advance) ||
            TryHandlePrefix(tokens, out result, out advance) ||
            TryClassifyOperators(tokens, out result, out advance) ||
            TryClassifyRelocation(current, out result, out advance) ||
            TryClassifyRegister(current, out result, out advance) ||
            TryClassifyImmediate(tokens, out result, out advance))
            return result;

        var peek = Peek(tokens);
        (var type, bool merge) = _state switch
        {
            TokenizerState.LineBegin when Peek(tokens, skipWhitespace: true)?.Source is "=" => (TokenType.MacroDeclaration, false),
            TokenizerState.LineBegin when current.Source is "." => (TokenType.Directive, true),
            TokenizerState.LineBegin when peek?.Source is ":" => (TokenType.LabelDeclaration, true),
            _ => (TokenType.Unknown, false),
        };

        if (type is not TokenType.Unknown)
        {
            if (merge && TryMerge(tokens, type, out result, out advance))
            {
                return result;
            }
            else
            {
                return ReClassify(type, current);
            }
        }

        // Check for either references or instruction names
        if (TryInstructionOrReference(tokens, out result, out advance))
            return result;

        // Token could not be classified
        // Return as-is
        return tokens[0];
    }

    private bool TryInstructionOrReference(ReadOnlySpan<Token> tokens, out Token? result, out int advance)
    {
        // Grab the current token
        var current = tokens[0];
        result = null;
        advance = 1;

        if (!current.IsIdentifier())
            return false;

        if (_state is TokenizerState.LineBegin)
        {
            // Handle instructions
            result = ReClassify(TokenType.Instruction, current);

            // Check for formatted instructions
            var span = tokens;
            do
            {
                var dot = Peek(span, 1);
                var next = Peek(span, 2);
                if (dot?.Source is not "." || !next.IsIdentifier())
                    break;

                result = Merge(TokenType.Instruction, result, dot, next);
                advance += 2;
                span = span[2..];
            } while (true);

        }
        else
        {
            // Handle references
            result = ReClassify(TokenType.Reference, current);
        }

        return true;
    }

    private bool TryHandlePrefix(ReadOnlySpan<Token> tokens, [NotNullWhen(true)] out Token? result, out int advance)
    {
        advance = 1;
        result = null;
        var current = tokens[0];
        if (current.Source.Length is not 1)
            return false;

        var type = TokenType.Unknown;

        // Handle register prefix
        if (_profile.RegisterPrefix != '\0' && current.Source[0] == _profile.RegisterPrefix)
        {
            type = TokenType.RegisterPrefix;
            _state = TokenizerState.RegisterPrefixed;
        }
        // Handle immediate prefix
        else if (_profile.ImmediatePrefix != '\0' && current.Source[0] == _profile.ImmediatePrefix)
        {
            type = TokenType.ImmediatePrefix;
            _state = TokenizerState.ImmediatePrefixed;
        }
        // Handle relocation prefix
        // The relocation prefix can conflict with other token types, so we need to check the next token to see if it is a valid relocation symbol
        else if (_profile.RelocationPrefix != '\0' && current.Source[0] == _profile.RelocationPrefix &&
            _profile.RelocationRegex.IsMatch(Peek(tokens)?.Source ?? ""))
        {
            type = TokenType.RelocationPrefix;
            _state = TokenizerState.RelocationPrefixed;
        }

        if (type is TokenType.Unknown)
            return false;

        result = ReClassify(type, current);
        _prefix = result;
        return true;
    }

    private bool TryClassifyRelocation(Token current, [NotNullWhen(true)] out Token? result, out int advance)
    {
        advance = 1;
        result = null;
        if (_state is not TokenizerState.RelocationPrefixed)
            return false;

        result = ReClassify(TokenType.Relocation, current);
        return true;
    }

    private bool TryClassifyRegister(Token current, [NotNullWhen(true)] out Token? result, out int advance)
    {
        advance = 1;

        bool isPrefixedRegister = _state is TokenizerState.RegisterPrefixed;
        bool isNonPrefixedRegister =
            _profile.RegisterPrefix is '\0' &&
            _state is not TokenizerState.LineBegin &&
            _profile.RegisterRegex.IsMatch(current.Source);

        if (isPrefixedRegister || isNonPrefixedRegister)
        {
            result = ReClassify(TokenType.Register, current);
            return true;
        }

        result = null;
        return false;
    }

    private bool TryClassifyImmediate(ReadOnlySpan<Token> tokens, [NotNullWhen(true)] out Token? result, out int advance)
    {
        result = null;
        advance = 0;

        if (_state is TokenizerState.LineBegin)
            return false;

        return TryConsumeNumericBody(tokens, out result, out advance);
    }

    private bool TryClassifyOperators(ReadOnlySpan<Token> tokens, [NotNullWhen(true)] out Token? result, out int advance)
    {
        var current = tokens[0].Source;
        var next = Peek(tokens)?.Source;

        (TokenType type, advance) = current switch
        {
            "(" => (TokenType.OpenParenthesis, 1),
            ")" => (TokenType.CloseParenthesis, 1),
            "[" => (TokenType.OpenBracket, 1),
            "]" => (TokenType.CloseBracket, 1),
            "," => (TokenType.Comma, 1),
            ";" => (TokenType.SemiColon, 1),

            "+" or "-" or "*" or "/" or "%" or
            "|" or "&" or "^" or "~" => (TokenType.Operator, 1),

            ">" when next is ">" or "=" => (TokenType.Operator, 2),
            ">" => (TokenType.Operator, 1),

            "<" when next is "<" or "=" => (TokenType.Operator, 2),
            "<" => (TokenType.Operator, 1),

            "!" when next is "=" => (TokenType.Operator, 2),
            "!" => (TokenType.Operator, 1),

            "=" when next is "=" => (TokenType.Operator, 2),
            "=" => (TokenType.Assign, 1),

            _ => (TokenType.Unknown, 0),
        };

        (bool success, result) = advance switch
        {
            0 => (false, null),
            1 => (true, ReClassify(type, tokens[0])),
            _ => (true, Merge(type, tokens[0], tokens[1..advance])),
        };

        return success;
    }

    /// <remarks>
    /// Also reclassified chars as an immediate.
    /// </remarks>
    private bool CheckPreclassified(Token token, [NotNullWhen(true)] out Token? result, out int advance)
    {
        advance = 1;

        if (token.Type is TokenType.Char)
        {
            result = ReClassify(TokenType.Immediate, token);
            return true;
        }

        result = null;
        if (token.Type is not (TokenType.String or TokenType.Comment or TokenType.Whitespace))
            return false;

        result = token;
        return true;
    }

    private bool TryMerge(ReadOnlySpan<Token> tokens, TokenType type, out Token? merged, out int advance)
    {
        advance = 1;
        merged = null;

        // Condition not met or tokens not found
        if (tokens.Length < 2)
            return false;

        // Conditions met and tokens found
        merged = Merge(type, tokens[0], tokens[1]);
        advance = 2;
        return true;
    }

    private Token Merge(TokenType type, Token @base, params ReadOnlySpan<Token?> tokens)
    {
        // Generate new text
        var source = new StringBuilder(@base.Source);
        foreach (var token in tokens)
        {
            source.Append(token?.Source);
        }

        var result = new Token($"{source}")
        {
            Type = type,
            Location = @base.Location,
            PrefixToken = _prefix,
        };

        _prefix = null;
        return result;
    }

    private static Token? Peek(ReadOnlySpan<Token> tokens, int n = 1, bool skipWhitespace = false)
    {
        Token? token;
        do
        {
            // We've hit the end. None found
            if (tokens.Length <= n)
                return null;

            // Grab the nth token and
            // advance the slice one for the next pass
            token = tokens[n];
            tokens = tokens[1..];

        } while (skipWhitespace && token.Type is TokenType.Whitespace);

        return token;
    }

    private Token ReClassify(TokenType type, Token original)
    {
        var result = new Token(original.Source)
        {
            Location = original.Location,
            Type = type,
            PrefixToken = _prefix,
        };

        _prefix = null;
        return result;
    }

    /// <summary>
    /// Attempts to consume a numeric pattern (Decimal or Integer) starting at a specific token offset.
    /// </summary>
    private bool TryConsumeNumericBody(ReadOnlySpan<Token> tokens, [NotNullWhen(true)] out Token? merged, out int advance)
    {
        merged = null;
        advance = 0;

        var baseToken = tokens[0];
        if (baseToken == null)
            return false;

        var secondary = Peek(tokens);
        var tertiary = Peek(tokens, 2);
        int baseCount = 0;

        if (baseToken.IsDigits() && secondary?.Source == "." && tertiary?.IsScientific(out bool eSuffix) == true)
        {
            // Case: 123.456 (Digits + Dot + Digits)
            merged = Merge(TokenType.Immediate, baseToken, secondary, tertiary);
            baseCount = 3;
        }
        else if (baseToken.Source == "." && secondary?.IsScientific(out eSuffix) == true)
        {
            // Case: .456 (Dot + Digits)
            merged = Merge(TokenType.Immediate, baseToken, secondary);
            baseCount = 2;
        }
        else if (baseToken.IsScientific(out eSuffix) || baseToken.IsInteger())
        {
            // Case: 123 (Integer)
            merged = Merge(TokenType.Immediate, baseToken);
            baseCount = 1;
        }

        if (merged is null)
            return false;

        // Handle scientific notation
        if (eSuffix)
        {
            // The last token involved in the merge so far ends with 'e' or 'E'
            var next1 = Peek(tokens, baseCount);
            var next2 = Peek(tokens, baseCount + 1);

            if ((next1?.Source is "+" or "-") && next2?.IsDigits() == true)
            {
                merged = Merge(TokenType.Immediate, merged, next1, next2);
                advance = baseCount + 2;
                return true;
            }

            return false;
        }

        advance = baseCount;
        return true;
    }
}
