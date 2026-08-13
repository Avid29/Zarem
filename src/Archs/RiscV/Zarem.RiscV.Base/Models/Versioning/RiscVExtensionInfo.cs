// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.Models.Versioning;

/// <summary>
/// A struct representing a set of RISC-V extensions parsed from specification strings or JSON models.
/// </summary>
public readonly partial struct RiscVExtensionInfo : IParsable<RiscVExtensionInfo>
{
    private static readonly Dictionary<string, RiscVExtensions> _extensionMap;
    private static readonly List<(string Name, RiscVExtensions Flag)> _standardExtensions;
    private static readonly List<(string Name, RiscVExtensions Flag)> _zExtensions;

    [GeneratedRegex(@"[\d+p\d+]*$", RegexOptions.IgnoreCase)]
    private static partial Regex GetExtensionVersionSuffixRegex();

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVVersionInfo"/> struct.
    /// </summary>
    public RiscVExtensionInfo() : this(RiscVExtensions.Integers)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVVersionInfo"/> struct.
    /// </summary>
    public RiscVExtensionInfo(RiscVExtensions extensions)
    {
        Flags = extensions | RiscVExtensions.Integers; // 'I' is always required
    }

    static RiscVExtensionInfo()
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
    /// Gets the group of extensions in use.
    /// </summary>
    public RiscVExtensions Flags { get; }

    /// <inheritdoc/>
    public static RiscVExtensionInfo Parse(string s, IFormatProvider? provider = null)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException($"The string '{s}' is not a valid RISC-V ISA string.");
        }

        return result;
    }

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out RiscVExtensionInfo result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
            return false;

        string remainder = s.Trim();
        var extensions = RiscVExtensions.Integers;

        // Parse Standard Single-Letter Extensions (e.g. "IMAFDC" or "G")
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

        // Parse Multi-Letter Extensions (e.g. "_Zicsr_Zifencei")
        if (i < remainder.Length)
        {
            var parts = remainder[i..].Split(['_', '+'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                string cleanPart = GetExtensionVersionSuffixRegex().Replace(part, "");
                if (_extensionMap.TryGetValue(cleanPart, out var flag))
                {
                    extensions |= flag;
                }
            }
        }

        result = new RiscVExtensionInfo(extensions);
        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();
        var currentExts = Flags;

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

    /// <summary>
    /// Casts a <see cref="RiscVExtensionInfo"/> to a <see cref="RiscVExtensions"/>.
    /// </summary>
    public static implicit operator RiscVExtensions(RiscVExtensionInfo info) => info.Flags;

    /// <summary>
    /// Casts a <see cref="RiscVExtensions"/> to a <see cref="RiscVExtensionInfo"/>.
    /// </summary>
    public static implicit operator RiscVExtensionInfo(RiscVExtensions flags) => new(flags);
}
