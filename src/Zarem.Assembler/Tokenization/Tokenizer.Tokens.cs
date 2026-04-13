// Avishai Dernis 2025

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Assembler.Extensions;
using Zarem.Assembler.Tokenization.Models;
using System.Runtime.CompilerServices;

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
            bool meaningful = newToken.Type is not (TokenType.Comment or TokenType.Whitespace or TokenType.RegisterPrefix or TokenType.ImmediatePrefix);

            if (meaningful || _mode is TokenizerMode.IDE or TokenizerMode.BehaviorExpression)
                classified.Add(newToken);

            // Begin a new assembly line if we see a semi-colon
            if (newToken.Type is TokenType.SemiColon)
            {
                // Append the current line
                lines.Add(new AssemblyLine([..classified]));

                // Begin new assembly line
                classified = [];
                _state = TokenizerState.LineBegin;
            }
            else if (meaningful && newToken.Type is not (TokenType.LabelDeclaration or TokenType.RegisterPrefix or TokenType.ImmediatePrefix))
            {
                // If a meaningful token was appended, we are no longer at the start. 
                // we also remain at the start after label declarations
                _state = TokenizerState.ArgumentPhase;
            }

            // Advance the appropriate number of tokens
            span = span[advance..];
        }

        lines.Add(new AssemblyLine([..classified]));

        return true;
    }

    private Token? ReTokenizeSpan(ReadOnlySpan<Token> tokens, out int advance)
    {
        advance = 1;

        // Stage 1: Trivial classifications
        if (TrySimpleReclass(tokens[0], out var simple))
            return simple;

        // Stage 2: Multi-token merges (registers, directives, and labels)
        if (TryMergeTokens(tokens, out var merged, ref advance))
            return merged;

        // Stage 3: Check for either references or instruction names
        if (TryInstructionOrReference(tokens, out var result, ref advance))
            return result;

        // Token could not be classified
        // Return as-is
        return tokens[0];
    }

    private bool TrySimpleReclass(Token current, out Token? classified)
    {
        classified = null;

        // Handle by determining a proper type
        TokenType type = current.Type switch
        {
            // Handle no-change types
            TokenType.String => TokenType.String,
            TokenType.Whitespace => TokenType.Whitespace,
            TokenType.Comment => TokenType.Comment,

            // Handle chars
            TokenType.Char => TokenType.Immediate,

            // Handle operators
            _ => current.Source switch
            {
                "(" => TokenType.OpenParenthesis,
                ")" => TokenType.CloseParenthesis,
                "[" => TokenType.OpenBracket,
                "]" => TokenType.CloseBracket,
                "," => TokenType.Comma,
                ";" => TokenType.SemiColon,

                "+" or "-" or "*" or "/" or "%" or
                "|" or "&" or "^" or "~" => TokenType.Operator,

                // Behavior mode only?
                "!" or "<" or ">" => TokenType.Operator,
                "=" => TokenType.Assign,
                _ => TokenType.Unknown,
            },
        };

        // Handle prefixes
        if (current.Source.Length is 1)
        {
            if (_profile.RegisterPrefix != '\0' && current.Source[0] == _profile.RegisterPrefix)
            {
                type = TokenType.RegisterPrefix;
                _state = TokenizerState.RegisterPrefixed;
            }

            else if (_profile.ImmediatePrefix != '\0' && current.Source[0] == _profile.ImmediatePrefix)
            {
                type = TokenType.ImmediatePrefix;
                _state = TokenizerState.ImmediatePrefixed;
            }
        }

        // Type not found. Return false
        if (type is TokenType.Unknown)
            return false;

        // Type was found. Reclassify the token and return true
        classified = ReClassify(type, current);
        return true;
    }

    private bool TryMergeTokens(ReadOnlySpan<Token> tokens, out Token? merged, ref int advance)
    {
        var current = tokens[0];
        var peek = Peek(tokens);

        // Handle register 
        if (_profile.RegisterPrefix is '\0' && _state is not TokenizerState.LineBegin)
        {
            if (_profile.RegisterRegex.IsMatch(current.Source))
            {
                // Handle non-prefixed registers
                merged = ReClassify(TokenType.Register, current);
                advance = 1;
                return true;
            }
        }
        else if (_state is TokenizerState.RegisterPrefixed)
        {
            merged = ReClassify(TokenType.Register, current);
            advance = 1;
            return true;
        }

        // Handle merging immediates tokens
        if (_state is not TokenizerState.LineBegin)
        {
            // Check for Standard Immediates
            if (TryConsumeNumericBody(tokens, out merged, out var numericAdvance))
            {
                advance = numericAdvance;
                return true;
            }
        }

        // Determine appropriate type
        (var type, bool merge) = _state switch
        {
            TokenizerState.LineBegin when Peek(tokens, skipWhitespace: true)?.Source is "=" => (TokenType.MacroDeclaration, false),
            TokenizerState.LineBegin when current.Source is "." => (TokenType.Directive, true),
            TokenizerState.LineBegin when peek?.Source is ":" => (TokenType.LabelDeclaration, true),
            _ => (TokenType.Unknown, false),
        };

        // Type not found
        merged = null;
        if (type is TokenType.Unknown)
            return false;

        // Type found, and it doesn't need to merge
        if (!merge)
        {
            merged = ReClassify(type, current);
            return true;
        }

        // Attempt to merge current and peek into the appropriate type
        return TryMerge(tokens, type, out merged, ref advance); 
    }

    private bool TryInstructionOrReference(ReadOnlySpan<Token> tokens, out Token? result, ref int advance)
    {
        // Grab the current token
        var current = tokens[0];
        result = null;

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

    private static bool TryMerge(ReadOnlySpan<Token> tokens, TokenType type, out Token? merged, ref int advance)
    {
        merged = null;

        // Condition not met or tokens not found
        if (tokens.Length < 2)
            return false;

        // Conditions met and tokens found
        merged = Merge(type, tokens[0], tokens[1]);
        advance = 2;
        return true;
    }

    private static Token Merge(TokenType type, Token @base, params Token?[] tokens)
    {
        // Generate new text
        var source = new StringBuilder(@base.Source);
        foreach (var token in tokens)
        {
            source.Append(token?.Source);
        }

        return new Token($"{source}")
        {
            Type = type,
            Location = @base.Location,
        };
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

    private static Token ReClassify(TokenType type, Token original)
    {
        return new Token(original.Source)
        {
            Location = original.Location,
            Type = type
        };
    }

    /// <summary>
    /// Attempts to consume a numeric pattern (Decimal or Integer) starting at a specific token offset.
    /// </summary>
    private static bool TryConsumeNumericBody(ReadOnlySpan<Token> tokens, out Token? merged, out int advance)
    {
        merged = null;
        advance = 0;

        var baseToken = tokens[0];
        if (baseToken == null)
            return false;

        var secondary = Peek(tokens);
        var tertiary = Peek(tokens, 2);

        // Case: 123.456 (Digits + Dot + Digits)
        if (baseToken.IsDigits() && secondary?.Source == "." && tertiary?.IsDigits() == true)
        {
            // Merge the prefix (if any) plus the 3 numeric tokens
            merged = Merge(TokenType.Immediate, baseToken, secondary, tertiary);
            advance = 3;
            return true;
        }

        // Case: .456 (Dot + Digits)
        if (baseToken.Source == "." && secondary?.IsDigits() == true)
        {
            merged = Merge(TokenType.Immediate, baseToken, secondary);
            advance = 2;
            return true;
        }

        // Case: 123 (Integer)
        if (baseToken.IsInteger())
        {
            merged = Merge(TokenType.Immediate, baseToken);
            advance = 1;
            return true;
        }

        return false;
    }
}
