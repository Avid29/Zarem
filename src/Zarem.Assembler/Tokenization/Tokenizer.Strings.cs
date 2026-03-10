// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using System.Collections.Generic;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Assembler.Tokenization;

public partial class Tokenizer
{
    private bool PreTokenizeLine(string line, out List<Token> raw)
    {
        // Prepare for string tokenization
        _state = TokenizerState.TokenBegin;
        raw = [];

        line += '\n';

        // Parse the line character by character
        bool status = true;
        foreach (char c in line)
        {
            // Parse the character and retrieve any created tokens
            if (!ParseNextChar(c, out var tokens))
                status = false;
            
            // Add the tokens to the collection
            raw.AddRange(tokens);
            _location += 1;
        }

        _location = _location.NextLine(0);
        _cacheLocation = _location;
        return status;
    }

    private bool ParseNextChar(char c, out List<Token> tokens)
    {
        tokens = [];
        Token? token = null;

        bool status;

        // Lines always end in "\n"
        // Use this to complete final token
        if (c is '\n')
        {
            status = CompleteCacheToken(out token);
            if (token is not null)
                tokens = [token];

            return status;
        }

        status = _state switch
        {
            // LineBegin
            TokenizerState.TokenBegin when c is '#' => AppendCharacter(c, TokenizerState.Comment),                      // Begin a comment
            TokenizerState.TokenBegin when c is '"' => AppendCharacter(c, TokenizerState.StringLiteral),                // Begin a string
            TokenizerState.TokenBegin when c is '\'' => AppendCharacter(c, TokenizerState.CharLiteral),                 // Begin a char
            TokenizerState.TokenBegin when char.IsWhiteSpace(c) => AppendCharacter(c, TokenizerState.Whitespace),       // Begin whitespace
            TokenizerState.TokenBegin when char.IsLetterOrDigit(c) || c is '_'                                          // Begin an identifier
                => AppendCharacter(c, TokenizerState.TokenBody),                                                        // or numerical
            TokenizerState.TokenBegin => AppendAndComplete(c, TokenizerState.TokenBegin, out token),                    // Begin an unknown token

            // Token Body
            TokenizerState.TokenBody when char.IsLetterOrDigit(c) || c is '_' => AppendCharacter(c),
            TokenizerState.TokenBody => CompleteAndContinue(c, out tokens),

            // String Literal
            TokenizerState.StringLiteral when c is '"' => AppendAndComplete(c, TokenizerState.TokenBegin, out token),
            TokenizerState.StringLiteral => AppendCharacter(c),

            // Char Literal
            TokenizerState.CharLiteral when c is '\'' => AppendAndComplete(c, TokenizerState.TokenBegin, out token),
            TokenizerState.CharLiteral => AppendCharacter(c),

            // Comments
            TokenizerState.Comment => AppendCharacter(c),

            // Whitespace
            TokenizerState.Whitespace when char.IsWhiteSpace(c) => AppendCharacter(c),
            TokenizerState.Whitespace => CompleteAndContinue(c, out tokens),

            _ => ThrowHelper.ThrowArgumentOutOfRangeException<bool>(nameof(_state)),
        };

        if (token is not null)
            tokens = [token];

        return status;
    }

    private bool AppendCharacter(char c, TokenizerState? newState = null)
    {
        // Append the character to the cache
        _cache.Append(c);

        // Update the state if needed
        if (newState is not null)
            _state = newState.Value;

        return true;
    }

    private bool AppendAndComplete(char c, TokenizerState newState, out Token? token)
    {
        // Append the character and complete the token
        _cache.Append(c);
        var status = CompleteCacheToken(out token, true);
        _state = newState;
        return status;
    }

    private bool CompleteAndContinue(char c, out List<Token> tokens)
    {
        // Complete the token
        var status = CompleteCacheToken(out var token);
        tokens = token is not null ? [token] : [];
        if (!status)
        {
            return false;
        }

        // Begin the next using this character
        _state = TokenizerState.TokenBegin;
        status = ParseNextChar(c, out var childTokens);
        tokens.AddRange(childTokens);
        return status;
    }

    private bool CompleteCacheToken(out Token? token, bool incrementedCache = false)
    {
        var source = $"{_cache}";
        token = null;

        // Nothing to complete
        if (string.IsNullOrEmpty(source))
            return true;

        // Determine what type of token to create from the state
        var tokenType = _state switch
        {
            TokenizerState.TokenBegin or
            TokenizerState.TokenBody => TokenType.Unknown,
            TokenizerState.CharLiteral => TokenType.Char,
            TokenizerState.StringLiteral => TokenType.String,
            TokenizerState.Comment => TokenType.Comment,
            TokenizerState.Whitespace => TokenType.Whitespace,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<TokenType>(nameof(_state)),
        };

        // Create the token and add to list
        token = new Token(source)
        {
            Type = tokenType,
            Location = _cacheLocation,
        };

        // Reset cache
        _cache.Clear();
        _cacheLocation = incrementedCache ? _location + 1 : _location;
        return true;
    }
}
