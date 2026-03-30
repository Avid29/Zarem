// Avishai Dernis 2026

using System.Text.RegularExpressions;

namespace Zarem.Assembler.Tokenization.Interfaces;

/// <summary>
/// An interface for a tokenizer profile, which provides information about the syntax of the assembly language being tokenized.
/// </summary>
public interface ITokenizerProfile
{
    /// <summary>
    /// Gets the character prefix to a register in the for the tokenizer.
    /// </summary>
    /// <remarks>
    /// For example, % in x86 or $ in MIPS. If the ISA does not use a prefix, this should be set to the null character '\0'.
    /// Also, alpha-numerical characters such as the 'x' in x0 from RISC-V should not be included in the prefix and instead be handled by the regex.
    /// </remarks>
    char RegisterPrefix { get; }

    /// <summary>
    /// Gets the regex for identifying a register in the ISA.
    /// </summary>
    /// <remarks>
    /// If the ISA does not use a prefix this must exclusively identify valid registers so valid labels names do not get misclassified.
    /// Otherwise, any token that begins with the prefix can be classified by the regex (which should include the prefix).
    /// </remarks>
    Regex RegisterRegex { get; }
}
