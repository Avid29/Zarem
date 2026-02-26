// Avishai Dernis 2024

namespace Zarem.Assembler.Tokenization.Models;

using CommunityToolkit.HighPerformance;
using System;
using System.Collections.Generic;
using Zarem.Assembler.Extensions.System;
using Zarem.Assembler.Tokenization.Models.Enums;

/// <summary>
/// A struct for wrapping the assembly args component 
/// </summary>
public struct AssemblyLineArgs
{
    private AssemblyArg[] _args;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyLineArgs"/> struct.
    /// </summary>
    /// <param name="argsSegment"></param>
    public AssemblyLineArgs(ArraySegment<Token> argsSegment)
    {
        _args = [];
        SplitArgs(argsSegment);
    }

    /// <summary>
    /// Gets an arg from the args array;
    /// </summary>
    public readonly AssemblyArg this[int index] => _args[index];

    /// <summary>
    /// Gets a range of args from the args array.
    /// </summary>
    public readonly ReadOnlySpan<AssemblyArg> this[Range range] => _args is not null ? _args[range] : [];

    /// <summary>
    /// Gets the number of args.
    /// </summary>
    public readonly int Count => _args?.Length ?? 0;

    private void SplitArgs(ArraySegment<Token> tokens)
    {
        var args = new List<AssemblyArg>();
        
        Token? lastComma = null;
        while (tokens.Count > 0)
        {
            var nextCommaIndex = tokens.AsSpan().FindNext(TokenType.Comma, out var comma);
            if (nextCommaIndex is -1)
            {
                // This is a base case for the final argument
                // We add the remaining tokens and break
                args.Add(new AssemblyArg(tokens, lastComma, comma));
                break;
            }

            // Extract the argument up to the next comma
            var arg = new AssemblyArg(tokens[..nextCommaIndex], lastComma, comma);
            args.Add(arg);

            // Slice tokens to the start of the next argument
            tokens = tokens[(nextCommaIndex+1)..];
            lastComma = comma;
        }

        // Convert the list to an array segment
        _args = [..args];
    }
}
