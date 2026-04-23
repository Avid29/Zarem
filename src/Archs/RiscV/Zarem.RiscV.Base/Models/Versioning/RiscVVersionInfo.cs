// Avishai Dernis 2026

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Models.Versioning;

/// <summary>
/// A struct definig a RISC-V version, including the base ISA and supported extensions.
/// </summary>
public readonly partial struct RiscVVersionInfo : IParsable<RiscVVersionInfo>
{
    [GeneratedRegex(@"^RV(32|64|128)(.*)$", RegexOptions.IgnoreCase)]
    private static partial Regex GetRiscVVersionRegex();

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVVersionInfo"/> struct.
    /// </summary>
    public RiscVVersionInfo() : this(RiscVBaseVersion.RV32, RiscVExtensions.Integers)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVVersionInfo"/> struct.
    /// </summary>
    public RiscVVersionInfo(RiscVBaseVersion @base, RiscVExtensions extensions)
    {
        Base = @base;
        Extensions = extensions | RiscVExtensions.Integers; // 'I' is always required
    }

    /// <summary>
    /// Gets the base RISC-V ISA version.
    /// </summary>
    public RiscVBaseVersion Base { get; }

    /// <summary>
    /// Gets the group of extensions in use.
    /// </summary>
    public RiscVExtensions Extensions { get; }

    /// <summary>
    /// Gets the major version number of the RISC-V specification.
    /// </summary>
    public byte SpecMajor { get; } = 2;

    /// <summary>
    /// Gets the minor version number of the RISC-V specification.
    /// </summary>
    public byte SpecMinor { get; } = 0;

    /// <inheritdoc/>
    public static RiscVVersionInfo Parse(string s, IFormatProvider? provider = null)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException($"The string '{s}' is not a valid RISC-V ISA string.");
        }

        return result;
    }

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out RiscVVersionInfo result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        // Pattern: RV(32|64|128) followed by extension characters
        // Note: This pattern captures the bit-width and the remaining characters
        var match = GetRiscVVersionRegex().Match(s);

        if (!match.Success)
            return false;

        // Parse Base
        RiscVBaseVersion baseVersion = match.Groups[1].Value switch
        {
            "32" => RiscVBaseVersion.RV32,
            "64" => RiscVBaseVersion.RV64,
            "128" => RiscVBaseVersion.RV128,
            _ => RiscVBaseVersion.RV32 // Default or handle RV128 if enum supports it
        };

        var extensions = RiscVExtensions.Integers; // 'I' is implied/required
        string extChars = match.Groups[2].Value.ToUpperInvariant();

        foreach (char c in extChars)
        {
            var flag = c switch
            {
                'I' => RiscVExtensions.Integers,
                'M' => RiscVExtensions.Multiplication,
                'A' => RiscVExtensions.Atomic,
                'F' => RiscVExtensions.FloatingPoint,
                'D' => RiscVExtensions.DoubleFloatingPoint,
                'C' => RiscVExtensions.Compressed,
                'G' => RiscVExtensions.General,
                _ => (RiscVExtensions)0 // Ignore unknown or handle Z-extensions
            };

            extensions |= flag;
        }

        result = new RiscVVersionInfo(baseVersion, extensions);
        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();

        // Append the Base
        sb.Append(Base.ToString().ToUpperInvariant());
        
        // Apend extensions in canonical order
        // 'I' is handled by the base, but if you have a separate bit, ensure it's first.
        if (!CheckAppend(sb, RiscVExtensions.General, 'G'))
        {
            CheckAppend(sb, RiscVExtensions.Integers, 'I');
            CheckAppend(sb, RiscVExtensions.Multiplication, 'M');
            CheckAppend(sb, RiscVExtensions.Atomic, 'A');
            CheckAppend(sb, RiscVExtensions.FloatingPoint, 'F');
            CheckAppend(sb, RiscVExtensions.DoubleFloatingPoint, 'D');
        }
        else
        {
            CheckAppend(sb, RiscVExtensions.Compressed, 'C');
        }

        return sb.ToString();
    }

    private bool CheckAppend(StringBuilder sb, RiscVExtensions extension, char c)
    {
        if (Extensions.HasFlag(extension))
        {
            sb.Append(c);
            return true;
        }

        return false;
    }
}
