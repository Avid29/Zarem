// Avishai Dernis 2026

using System.Text.RegularExpressions;
using Zarem.Assembler.Tokenization.Profiles;

namespace Zarem.Assembler;

/// <summary>
/// An <see cref="ITokenizerProfile"/> for the MIPS architecture.
/// </summary>
public partial class MipsTokenizerProfile : ITokenizerProfile
{
    /// <summary>
    /// Gets the default tokenizer profile for the MIPS architecture.
    /// </summary>
    public static MipsTokenizerProfile Default { get; } = new();

    /// <inheritdoc/>
    public char RegisterPrefix => '$';

    /// <inheritdoc/>
    public char ImmediatePrefix => '\0';

    /// <inheritdoc/>
    public char CommentPrefix => '#';

    /// <inheritdoc/>
    /// <remarks>
    /// Validates the register name after the '$'.
    /// Note: This is used for validation/highlighting, not initial merging.
    /// </remarks>
    public Regex RegisterRegex { get; } = GetRegisterRegex();

    [GeneratedRegex(@"^\$(zero|at|v[0-1]|a[0-3]|t[0-9]|s[0-7]|k[0-1]|gp|sp|fp|ra|[0-9]|[1-2][0-9]|3[0-1])$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetRegisterRegex();

}
