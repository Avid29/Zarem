// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Tokenization;

/// <summary>
/// A class for tokenizing an assembly file.
/// </summary>
public partial class Tokenizer
{
    private readonly TokenizerMode _mode;
    private readonly StringBuilder _cache;

    private TokenizerState _state;

    private SourceLocation _location;
    private SourceLocation _cacheLocation;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tokenizer"/> class.
    /// </summary>
    private Tokenizer(string? filename, TokenizerMode mode = TokenizerMode.Assembly)
    {
        TokenLines = [];
        _mode = mode;
        _state = TokenizerState.TokenBegin;
        _cache = new();
        _location = new SourceLocation(filename);
        _cacheLocation = _location;
    }

    private List<AssemblyLine> TokenLines { get; }

    /// <inheritdoc/>
    public static async Task<TokenizedAssembly> TokenizeAsync(Stream stream, string? filename = null)
    {
        using var reader = new StreamReader(stream);
        return await TokenizeAsync(reader, filename);
    }

    /// <summary>
    /// Tokenizes a stream of assembly code.
    /// </summary>
    /// <param name="reader">The stream of code.</param>
    /// <param name="filename">The filename of the stream.</param>
    /// <returns>A list of tokens.</returns>
    public static async Task<TokenizedAssembly> TokenizeAsync(TextReader reader, string? filename = null)
    {
        // Create tokenizer
        Tokenizer tokenizer = new(filename);

        // Parse line by line from stream
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null)
                break;

            tokenizer.TokenizeLine(line);
        }

        return new TokenizedAssembly(tokenizer.TokenLines);
    }

    /// <summary>
    /// Tokenizes a single line of assembly code.
    /// </summary>
    public static AssemblyLine TokenizeLine(string line, string? filename = null, TokenizerMode mode = TokenizerMode.Assembly)
    {
        Tokenizer tokenizer = new(filename, mode: mode);

        if (line.Contains('\n'))
            ThrowHelper.ThrowArgumentException("Single line tokenizer cannot contain a new line.");

        tokenizer.TokenizeLine(line);
        return tokenizer.TokenLines[0];
    }

    private bool TokenizeLine(string line)
    {
        // First pass
        if (!PreTokenizeLine(line, out var raw))
            return false;
        
        // Second pass
        if (!ReTokenizeLine(raw, out var classified))
            return false;

        TokenLines.Add(new AssemblyLine([..classified]));
        return true;
    }
}
