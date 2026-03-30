// Avishai Dernis 2026

using System.Text.RegularExpressions;
using Zarem.Assembler.Tokenization.Interfaces;

namespace Zarem.Assembler;

/// <summary>
/// An <see cref="ITokenizerProfile"/> for the RISC-V architecture.
/// </summary>
public partial class RiscVTokenizerProfile : ITokenizerProfile
{
    /// <inheritdoc/>
    public char RegisterPrefix => '\0';

    /// <inheritdoc/>
    public char ImmediatePrefix => '\0';

    /// <inheritdoc/>
    public char CommentPrefix => '#';

    /// <inheritdoc/>
    public Regex RegisterRegex { get; } = GetRegisterRegex();


    [GeneratedRegex(@"^(x\d{1,2}|zero|ra|sp|gp|tp|t[0-6]|s[0-1]{1}|s[2-9]|s1[0-1]|a[0-7]|fp)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetRegisterRegex();
}
