// Avishai Dernis 2025

using System.Collections.Generic;
using System.Linq;

namespace Zarem.Assembler.Tokenization.Models;

/// <summary>
/// A class for wrapping tokenized assembly files.
/// </summary>
public class TokenizedAssembly
{
    private readonly List<AssemblyLine> _tokenLines;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenizedAssembly"/> class.
    /// </summary>
    public TokenizedAssembly(List<AssemblyLine> tokens) => _tokenLines = tokens;

    /// <summary>
    /// Gets a line's token list as a span.
    /// </summary>
    /// <param name="line">The number of the line to retrieve.</param>
    /// <returns>The line's token list as a span.</returns>
    public AssemblyLine this[int line]
    {
        get => _tokenLines[line];
    }

    /// <summary>
    /// Gets the total number of lines.
    /// </summary>
    public int LineCount => _tokenLines.Count;

    /// <summary>
    /// Gets the total number of tokens.
    /// </summary>
    public int TokenCount => _tokenLines.Sum(x => x.Count);
}
