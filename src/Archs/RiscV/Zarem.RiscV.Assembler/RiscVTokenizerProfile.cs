// Avishai Dernis 2026

using System.Text.RegularExpressions;
using Zarem.Assembler.Tokenization.Profiles;

namespace Zarem.Assembler;

/// <summary>
/// An <see cref="ITokenizerProfile"/> for the RISC-V architecture.
/// </summary>
public partial class RiscVTokenizerProfile : ITokenizerProfile
{
    /// <summary>
    /// Gets the default tokenizer profile for the RISC-V architecture.
    /// </summary>
    public static RiscVTokenizerProfile Default { get; } = new();

    /// <inheritdoc/>
    public char CommentPrefix => '#';

    /// <inheritdoc/>
    public char ImmediatePrefix => '\0';

    /// <inheritdoc/>
    public char RegisterPrefix => '\0';

    /// <inheritdoc/>
    public char RelocationPrefix => '%';

    /// <inheritdoc/>
    public Regex RegisterRegex { get; } = GetRegisterRegex();

    /// <inheritdoc/>
    public Regex RelocationRegex { get; } = GetRelocationRegex();

    [GeneratedRegex(@"^(x\d{1,2}|f\d{1,2}|zero|ra|sp|gp|tp|t[0-6]|s[0-1]{1}|s[2-9]|s1[0-1]|a[0-7]|fp|ft[0-9]|ft1[0-1]|fa[0-7]|fs[0-9]|fs1[0-1])$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetRegisterRegex();

    [GeneratedRegex(@"^(hi|lo|pcrel_hi|pcrel_lo|got_pcrel_hi|tprel_hi|tprel_lo|tprel_add|tls_ie_pcrel_hi|tls_gd_pcrel_hi)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetRelocationRegex();
}
