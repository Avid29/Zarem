// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.Models.Versioning;

/// <summary>
/// A struct defining a RISC-V version, including the base ISA and supported extensions.
/// </summary>
public readonly partial struct RiscVVersionInfo : IParsable<RiscVVersionInfo>
{
    [GeneratedRegex(@"^RV(32|64|128)(.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
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
    public RiscVExtensionInfo Extensions { get; }

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

        var match = GetRiscVVersionRegex().Match(s.Trim());
        if (!match.Success)
            return false;

        // Parse Base
        RiscVBaseVersion baseVersion = match.Groups[1].Value switch
        {
            "32" => RiscVBaseVersion.RV32,
            "64" => RiscVBaseVersion.RV64,
            "128" => RiscVBaseVersion.RV128,
            _ => RiscVBaseVersion.RV32
        };

        // Delegate extension parsing directly to RiscVExtensionInfo
        string extensionString = match.Groups[2].Value;
        RiscVExtensionInfo extInfo = RiscVExtensionInfo.TryParse(extensionString, provider, out var parsedExt)
            ? parsedExt
            : new RiscVExtensionInfo(RiscVExtensions.Integers);

        result = new RiscVVersionInfo(baseVersion, extInfo);
        return true;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Base}{Extensions}";
}
