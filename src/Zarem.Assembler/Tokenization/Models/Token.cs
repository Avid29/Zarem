// Avishai Dernis 2024

using Zarem.Assembler.Tokenization.Models.Enums;

namespace Zarem.Assembler.Tokenization.Models;

/// <summary>
/// A token for parsing the assembly.
/// </summary>
public class Token
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> class.
    /// </summary>
    /// <param name="source"></param>
    public Token(string source)
    {
        Source = source;
    }

    /// <summary>
    /// Gets the value of the token as a string.
    /// </summary>
    public string Source { get; init; }

    /// <summary>
    /// Gets the token's file path.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets the token's location.
    /// </summary>
    public SourceLocation Location { get; init; }

    /// <summary>
    /// Gets the token's type.
    /// </summary>
    public TokenType Type { get; init; }

    /// <inheritdoc/>
    public override string ToString() => Source;
}
