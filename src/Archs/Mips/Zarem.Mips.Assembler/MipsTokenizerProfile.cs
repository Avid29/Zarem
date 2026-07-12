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
    public char CommentPrefix => '#';

    /// <inheritdoc/>
    public char ImmediatePrefix => '\0';

    /// <inheritdoc/>
    public char RegisterPrefix => '$';

    /// <inheritdoc/>
    public char RelocationPrefix => '%';

    /// <inheritdoc/>
    /// <remarks>
    /// Validates the register name after the '$'.
    /// Note: This is used for validation/highlighting, not initial merging.
    /// </remarks>
    public Regex RegisterRegex { get; } = GetRegisterRegex();

    /// <inheritdoc/>
    public Regex RelocationRegex { get; } = GetRelocationRegex();

    [GeneratedRegex(@"^(zero|at|v[0-1]|a[0-3]|t[0-9]|s[0-7]|k[0-1]|gp|sp|fp|ra|[0-9]|[1-2][0-9]|3[0-1])$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetRegisterRegex();

    [GeneratedRegex(@"^(hi|lo|got|pcrel_hi|pcrel_lo|call16|got_hi|got_lo|got_disp|got_page|got_ofst|neg|gp_rel|tlsgd|tlsldm|dtprel_hi|dtprel_lo|tprel_hi|tprel_lo)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetRelocationRegex();
}
