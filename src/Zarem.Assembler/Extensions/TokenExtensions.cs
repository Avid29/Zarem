// Avishai Dernis 2025

using System.Linq;
using System.Text.RegularExpressions;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Assembler.Extensions;

/// <summary>
/// A static class containing extensions on the <see cref="Token"/> type.
/// </summary>
public static partial class TokenExtensions
{
    /// <summary>
    /// Gets whether or not a token is numerical an identifier.
    /// </summary>
    public static bool IsIdentifier(this Token? token)
        => token?.Source.All(x => char.IsLetterOrDigit(x) || x is '_') ?? false;

    /// <summary>
    /// Gets whether or not a token is numerical.
    /// </summary>
    public static bool IsInteger(this Token? token)
        => IntegerRegex().IsMatch(token?.Source ?? string.Empty);

    /// <summary>
    /// Gets whether or not a token is numerical.
    /// </summary>
    public static bool IsDigits(this Token? token)
        => DigitsRegex().IsMatch(token?.Source ?? string.Empty);

    [GeneratedRegex(@"^(?:0x[0-9a-fA-F]+|0b[01]+|0o[0-7]+|\d+)$")]
    private static partial Regex IntegerRegex();

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex DigitsRegex();
}
