// Avishai Dernis 2024

using System;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;

namespace Zarem.Assembler.Extensions.System;

/// <summary>
/// A class containing extensions for <see cref="Span{Token}"/>.
/// </summary>
public static class TokenSpanExtensions
{
    /// <summary>
    /// Grabs the label declaration from the start of a token line.
    /// </summary>
    /// <param name="line">The line to trim.</param>
    /// <param name="type">The type of the </param>
    /// <param name="trimmed">The label on the line, if any.</param>
    public static ReadOnlySpan<Token> TrimType(this ReadOnlySpan<Token> line, TokenType type, out Token? trimmed)
    {
        trimmed = null;

        if (line.Length is 0)
            return line;

        // Find components starts
        if (line[0].Type == type)
        {
            trimmed = line[0];
            line = line[1..];
        }

        return line;
    }

    /// <summary>
    /// Grabs the first token of the span <paramref name="tokens"/>, then slices the span.
    /// </summary>
    /// <param name="tokens">A ref to the span to increment</param>
    /// <returns>The first token popped of the span, or null if empty.</returns>
    public static Token? Next(this ref ReadOnlySpan<Token> tokens)
    {
        if (tokens.Length is 0)
            return null;

        var token = tokens[0];
        tokens = tokens[1..];
        return token;
    }

    /// <summary>
    /// Gets the index of the first instance of a token type in a span of tokens.
    /// </summary>
    /// <param name="line">The line to scan.</param>
    /// <param name="type">The type of the token to find.</param>
    /// <param name="token">The toke found.</param>
    /// <returns>The index of the first token of the <paramref name="type"/>, or -1 if none is found.</returns>
    public static int FindNext(this ReadOnlySpan<Token> line, TokenType type, out Token? token)
    {
        for (int i = 0; i < line.Length; i++)
        {
            token = line[i];
            if (token.Type == type)
                return i;
        }

        token = null;
        return -1;
    }

    /// <summary>
    /// Gets the index of the last instance of a token type in a span of tokens.
    /// </summary>
    /// <param name="line">The line to scan.</param>
    /// <param name="type">The type of the token to find.</param>
    /// <param name="token">The token found.</param>
    /// <returns>The index of the last token of the <paramref name="type"/>, or -1 if none is found.</returns>
    public static int FindLast(this ReadOnlySpan<Token> line, TokenType type, out Token? token)
    {
        for (int i = line.Length - 1; i >= 0; i--)
        {
            token = line[i];
            if (token.Type == type)
                return i;
        }
        token = null;
        return -1;
    }

    /// <summary>
    /// Converts a token span into a string.
    /// </summary>
    public static string Print(this ReadOnlySpan<Token> line)
    {
        string str = string.Empty;
        for (int i = 0; i < line.Length; i++)
        {
            str += line[i] + " ";
        }

        return str.TrimEnd();
    }
}
