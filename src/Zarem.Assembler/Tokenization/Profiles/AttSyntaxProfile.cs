// Avishai Dernis 2026

using System.Text.RegularExpressions;

namespace Zarem.Assembler.Tokenization.Profiles;

/// <summary>
/// A base class for a tokenizer profile that defines syntax rules guided by AT-T assembly syntax.
/// </summary>
public abstract class AttSyntaxProfile : ITokenizerProfile
{
    /// <inheritdoc/>
    public char CommentPrefix => ';';

    /// <inheritdoc/>
    public char ImmediatePrefix => '$';

    /// <inheritdoc/>
    public char RegisterPrefix => '%';

    /// <inheritdoc/>
    public abstract Regex RegisterRegex { get; }
}
