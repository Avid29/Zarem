// Avishai Dernis 2026

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Zarem.Z80.Models.Versioning.Enums;

namespace Zarem.Z80.Models.Versioning;

/// <summary>
/// A struct defining a Z80-family architecture version.
/// </summary>
public readonly partial struct Z80VersionInfo : IParsable<Z80VersionInfo>
{
    [GeneratedRegex(@"^(e)?z(80|180|280|380)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetZ80VersionRegex();

    /// <summary>
    /// Initializes a new instance of the <see cref="Z80VersionInfo"/> struct defaulting to the baseline Z80.
    /// </summary>
    public Z80VersionInfo() : this(Z80Generation.Z80, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Z80VersionInfo"/> struct with specific generation.
    /// </summary>
    public Z80VersionInfo(Z80Generation @base) : this(@base, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Z80VersionInfo"/> struct.
    /// </summary>
    public Z80VersionInfo(Z80Generation @base, bool isExtended)
    {
        Generation = @base;
        IsExtended = isExtended;

        // Strict validation: Only the Z80 can be extended (creating the eZ80)
        if (IsExtended && @base != Z80Generation.Z80)
        {
            throw new ArgumentException(
                $"Architecture baseline '{@base}' cannot be extended. Only the Z80 supports the extended (eZ80) modification.");
        }
    }

    /// <summary>
    /// Gets the base Z80 CPU generation family.
    /// </summary>
    public Z80Generation Generation { get; }

    /// <summary>
    /// Gets whether the architecture variant runs in extended mode (eZ80).
    /// </summary>
    public bool IsExtended { get; }

    /// <inheritdoc/>
    public static Z80VersionInfo Parse(string s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException($"The string '{s}' is not a valid Z80 target ISA string.");
        }

        return result;
    }

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Z80VersionInfo result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
            return false;

        var match = GetZ80VersionRegex().Match(s.Trim());
        if (!match.Success)
            return false;

        bool isExtended = match.Groups[1].Success; // True if the "e" prefix is present
        string baseNumber = match.Groups[2].Value; // "80", "180", "280", "380"

        Z80Generation baseVersion = baseNumber switch
        {
            "80" => Z80Generation.Z80,
            "180" => Z80Generation.Z180,
            "280" => Z80Generation.Z280,
            "380" => Z80Generation.Z380,
            _ => Z80Generation.Z80
        };

        // Fail parsing immediately if a non-Z80 generation tries to use the 'e' prefix
        if (isExtended && baseVersion != Z80Generation.Z80)
            return false;

        result = new Z80VersionInfo(baseVersion, isExtended);
        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (IsExtended)
        {
            return "ez80";
        }

        return Generation.ToString().ToUpperInvariant();
    }
}
