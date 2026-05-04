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
/// A struct definig a RISC-V version, including the base ISA and supported extensions.
/// </summary>
public readonly partial struct RiscVVersionInfo : IParsable<RiscVVersionInfo>
{
    private static readonly Dictionary<string, RiscVExtensions> _extensionMap;
    private static readonly List<(string Name, RiscVExtensions Flag)> _standardExtensions;
    private static readonly List<(string Name, RiscVExtensions Flag)> _zExtensions;

    [GeneratedRegex(@"^RV(32|64|128)(.*)$", RegexOptions.IgnoreCase)]
    private static partial Regex GetRiscVVersionRegex();

    [GeneratedRegex(@"[\d+p\d+]*$", RegexOptions.IgnoreCase)]
    private static partial Regex GetExtensionVersionSuffixRegex();

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

    static RiscVVersionInfo()
    {
        _extensionMap = [];

        var enumType = typeof(RiscVExtensions);
        foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<JsonStringEnumMemberNameAttribute>();
            if (attr != null && field.GetValue(null) is RiscVExtensions value)
            {
                // Skip 'General' for the map to avoid overlap during parsing, 
                // handle it as a special case or bit-mask if preferred.
                if (value == RiscVExtensions.General) continue;

                _extensionMap[attr.Name] = value;
            }
        }

        // Pre-sort for ToString() canonical order
        _standardExtensions = [.. _extensionMap
            .Where(kvp => kvp.Key.Length == 1)
            .OrderBy(kvp => "IMAFDQLCBJTPN".IndexOf(kvp.Key[0])) // Standard RISC-V order
            .Select(kvp => (kvp.Key, kvp.Value))];

        _zExtensions = [.. _extensionMap
            .Where(kvp => kvp.Key.Length > 1)
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => (kvp.Key, kvp.Value))];
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
        if (string.IsNullOrWhiteSpace(s)) return false;

        var match = GetRiscVVersionRegex().Match(s);
        if (!match.Success) return false;

        // Parse Base
        RiscVBaseVersion baseVersion = match.Groups[1].Value switch
        {
            "32" => RiscVBaseVersion.RV32,
            "64" => RiscVBaseVersion.RV64,
            "128" => RiscVBaseVersion.RV128,
            _ => RiscVBaseVersion.RV32
        };

        var extensions = RiscVExtensions.Integers;
        string remainder = match.Groups[2].Value;

        // Parse Standard Extensions (Single letters)
        int i = 0;
        while (i < remainder.Length && !char.IsDigit(remainder[i]) && remainder[i] != '_' && remainder[i] != '+')
        {
            string single = remainder[i].ToString();
            if (single == "G")
            {
                extensions |= RiscVExtensions.General;
            }
            else if (_extensionMap.TryGetValue(single, out var flag))
            {
                extensions |= flag;
            }
            i++;
        }

        // Parse Multi-letter Extensions (Z-extensions)
        // RISC-V spec usually uses '_' or '+' as separators for multi-letter names
        var parts = remainder[i..].Split(['_', '+'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            // Remove version numbers if present (e.g., Zfh1p0 -> Zfh)
            string cleanPart = GetExtensionVersionSuffixRegex().Replace(part, "");
            if (_extensionMap.TryGetValue(cleanPart, out var flag))
            {
                extensions |= flag;
            }
        }

        result = new RiscVVersionInfo(baseVersion, extensions);
        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Base.ToString().ToUpperInvariant());

        var currentExts = Extensions;

        // If G is present, it replaces IMAFD
        if (currentExts.HasFlag(RiscVExtensions.General))
        {
            sb.Append('G');

            // Remove flags covered by G to avoid double printing
            currentExts &= ~RiscVExtensions.General;
        }

        // Append remaining Standard Extensions
        foreach (var (name, flag) in _standardExtensions)
        {
            if (name == "I" && sb.ToString().Contains('G'))
                continue;

            if (currentExts.HasFlag(flag))
            {
                sb.Append(name);
            }
        }

        // Append Multi-letter extensions with '+'
        bool firstZ = true;
        foreach (var (name, flag) in _zExtensions)
        {
            if (currentExts.HasFlag(flag))
            {
                sb.Append(firstZ ? "_" : "_"); // Use '_' or '+' per preference
                sb.Append(name);
                firstZ = false;
            }
        }

        return sb.ToString();
    }
}
