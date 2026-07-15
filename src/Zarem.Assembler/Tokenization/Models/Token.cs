// Avishai Dernis 2024

using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Models.Tables;

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
    /// Gets the token's location.
    /// </summary>
    public SourceLocation Location { get; init; }

    /// <summary>
    /// Gets the token's type.
    /// </summary>
    public TokenType Type { get; init; }

    /// <summary>
    /// Gets the prefix token, if one exists.
    /// </summary>
    public Token? PrefixToken { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (PrefixToken is not null)
        {
            return $"{PrefixToken}{Source}";
        }

        return $"{Source}";
    }
}
