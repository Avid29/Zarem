// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using Zarem.Mips.Models.Versioning.Enums;

namespace Zarem.Mips.Models.Versioning;

/// <summary>
/// A struct definig a MIPS version.
/// </summary>
public readonly partial struct MipsVersionInfo : IParsable<MipsVersionInfo>
{
    [GeneratedRegex(@"^mips(?:(32|64))?(i{1,3}|iv|v|[1-5]|r[1-356])(?:_(32bit|64bit))?$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetMipsVersionRegex();

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsVersionInfo"/> struct.
    /// </summary>
    public MipsVersionInfo() : this(MipsGeneration.R2, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsVersionInfo"/> struct.
    /// </summary>
    public MipsVersionInfo(MipsGeneration @base)
    {
        Generation = @base;
        Is64Bit = Is64BitDefault(@base);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsVersionInfo"/> struct.
    /// </summary>
    public MipsVersionInfo(MipsGeneration @base, bool is64Bit)
    {
        Generation = @base;
        Is64Bit = is64Bit;

        if (Is64Bit && @base <= MipsGeneration.MipsII)
        {
            ThrowHelper.ThrowArgumentException(
                $"Architecture baseline '{@base}' cannot be configured as 64-bit. MIPS I and MIPS II are strictly 32-bit architectures.");
        }
    }

    /// <summary>
    /// Gets the base MIPS Generation.
    /// </summary>
    public MipsGeneration Generation { get; }

    /// <summary>
    /// Gets whether or not the 64-bit version is in use.
    /// </summary>
    public bool Is64Bit { get; }

    /// <inheritdoc/>
    public static MipsVersionInfo Parse(string s, IFormatProvider? provider = null)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException($"The string '{s}' is not a valid MIPS target ISA string.");
        }

        return result;
    }

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out MipsVersionInfo result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
            return false;

        var match = GetMipsVersionRegex().Match(s.Trim());
        if (!match.Success)
            return false;

        string explicitBitnessPrefix = match.Groups[1].Value;                       // "32" or "64"
        string architectureIdentifier = match.Groups[2].Value.ToUpperInvariant();   // e.g., "II", "2", "R2"
        string legacyBitnessSuffix = match.Groups[3].Value;                         // "_32bit" or "_64bit" (though the underscore is not captured

        bool hasBitnessPrefix = match.Groups[1].Success;
        bool hasLegacySuffix = match.Groups[3].Success;
        bool hasModernRPrefix = architectureIdentifier.StartsWith('R');

        // Force strict bitness check: if it uses the 'r' prefix (e.g., mipsr2), it MUST have 32 or 64 
        if (hasModernRPrefix && !hasBitnessPrefix)
            return false;

        // 2. Parse the Base Version string layout
        // Strings with 'R' map to modern releases. Raw digits and Roman numerals map to classic.
        MipsGeneration baseVersion = architectureIdentifier switch
        {
            "I" or "1" => MipsGeneration.MipsI,
            "II" or "2" => MipsGeneration.MipsII,
            "III" or "3" => MipsGeneration.MipsIII,
            "IV" or "4" => MipsGeneration.MipsIV,
            "V" or "5" => MipsGeneration.MipsV,
            "R1" => MipsGeneration.R1,
            "R2" => MipsGeneration.R2,
            "R3" => MipsGeneration.R3,
            "R5" => MipsGeneration.R5,
            "R6" => MipsGeneration.R6,
            _ => MipsGeneration.MipsI
        };

        // Determine Bitness
        bool is64Bit;
        if (hasBitnessPrefix)
        {
            is64Bit = explicitBitnessPrefix == "64";
        }
        else if (match.Groups[3].Success)
        {
            is64Bit = legacyBitnessSuffix == "64bit";
        }
        else
        {
            is64Bit = Is64BitDefault(baseVersion);
        }

        // MipsI and MipsII cannot be 64bit
        if (is64Bit && baseVersion <= MipsGeneration.MipsII)
            return false;

        result = new MipsVersionInfo(baseVersion, is64Bit);
        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("mips");

        if (Generation >= MipsGeneration.R1)
        {
            // Outputs modern target strings like: "mips32r2" or "mips64r6"
            sb.Append(Is64Bit ? "64" : "32");
            sb.Append(Generation.ToString().ToLowerInvariant());
        }
        else
        {
            // Outputs classic target strings like: "mips1", "mips3", or "mips3_32bit"
            sb.Append((int)Generation);
            if (Is64BitDefault(Generation) && !Is64Bit)
            {
                sb.Append("_32bit");
            }
        }

        return sb.ToString();
    }

    private static bool Is64BitDefault(MipsGeneration version) => version >= MipsGeneration.MipsIII;
}
