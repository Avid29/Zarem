// Avishai Dernis 2026

using ModelingEvolution.JsonParsableConverter;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Zarem.RiscV.Attributes;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.Models.Versioning;

/// <summary>
/// A struct representing a set of RISC-V extensions parsed from specification strings or JSON models.
/// </summary>
[JsonConverter(typeof(JsonParsableConverter<RiscVExtensionInfo>))]
public readonly partial struct RiscVExtensionInfo : IParsable<RiscVExtensionInfo>
{
    private static readonly Dictionary<string, RiscVExtensions> _extensionMap;
    private static readonly Dictionary<string, RiscVZExtensions> _zExtensionMap;
    private static readonly List<(string Name, RiscVExtensions Flag)> _sortedExtensions;
    private static readonly List<(string Name, RiscVZExtensions Flag)> _sortedZExtensions;
    private static readonly Dictionary<string, RiscVExtensionInfo> _dependencyMap;

    [GeneratedRegex(@"[\d+p\d+]*$", RegexOptions.IgnoreCase)]
    private static partial Regex GetExtensionVersionSuffixRegex();

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVVersionInfo"/> struct.
    /// </summary>
    public RiscVExtensionInfo(RiscVExtensions extensions, RiscVZExtensions zExtensions = RiscVZExtensions.None)
    {
        this.MisaFlags = extensions | RiscVExtensions.Integers; // 'I' is always required
        ZFlags = zExtensions;
    }

    static RiscVExtensionInfo()
    {
        _extensionMap = [];
        _zExtensionMap = [];
        _dependencyMap = [];

        foreach (var field in typeof(RiscVExtensions).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<RiscVExtensionAttribute>();
            if (attr != null && field.GetValue(null) is RiscVExtensions value)
            {
                _extensionMap[attr.Alias] = value;

                if (attr.Dependencies.MisaFlags is not RiscVExtensions.Integers
                    || attr.Dependencies.ZFlags is not RiscVZExtensions.None)
                    _dependencyMap[attr.Alias] = attr.Dependencies;
            }
        }
        foreach (var field in typeof(RiscVZExtensions).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<RiscVExtensionAttribute>();
            if (attr != null && field.GetValue(null) is RiscVZExtensions value)
                _zExtensionMap[attr.Alias] = value;
        }

        // Pre-sort for ToString() canonical order
        _sortedExtensions = [.. _extensionMap
            .Where(kvp => kvp.Key.Length == 1)
            .OrderBy(kvp => "IMAFDQLCBJTPN".IndexOf(kvp.Key[0])) // Standard RISC-V order
            .Select(kvp => (kvp.Key, kvp.Value))];

        _sortedZExtensions = [.. _zExtensionMap
            .Where(kvp => kvp.Key.Length > 1)
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => (kvp.Key, kvp.Value))];
    }

    /// <summary>
    /// Gets the flagged collection of MISA extensions present.
    /// </summary>
    public RiscVExtensions MisaFlags { get; }

    /// <summary>
    /// Gets the flagged collection of Z extensions present.
    /// </summary>
    public RiscVZExtensions ZFlags { get; }

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
        var zExtensions = RiscVZExtensions.None;

        // Parse Standard Single-Letter Extensions (e.g. "IMAFDC" or "G")
        int i = 0;
        while (i < remainder.Length && !char.IsDigit(remainder[i]) && remainder[i] != '_' && remainder[i] != '+')
        {
            string single = remainder[i].ToString();
            if (_extensionMap.TryGetValue(single, out var flag))
            {
                extensions |= flag;
            }

            if (_dependencyMap.TryGetValue(single, out var dependencies))
            {
                extensions |= dependencies.MisaFlags;
                zExtensions |= dependencies.ZFlags;
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
                if (_zExtensionMap.TryGetValue(cleanPart, out var flag))
                {
                    zExtensions |= flag;
                }
            }
        }

        result = new RiscVExtensionInfo(extensions, zExtensions);
        return true;
    }

    /// <summary>
    /// Determines whether this instance contains all flags specified in <paramref name="extensions"/>.
    /// </summary>
    public bool Contains(RiscVExtensionInfo extensions) =>
        MisaFlags.HasFlag(extensions.MisaFlags)
        && ZFlags.HasFlag(extensions.ZFlags);

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();
        var impliedMisa = RiscVExtensions.None;
        var impliedZ = RiscVZExtensions.None;

        // Handle determining I vs G
        var gDependencies = _dependencyMap["G"];
        bool isG = Contains(gDependencies);
        sb.Append(isG ? 'G' : 'I');

        foreach (var (alias, dependencies) in _dependencyMap)
        {
            _extensionMap.TryGetValue(alias, out var flag);
            _zExtensionMap.TryGetValue(alias, out var zFlag);
            var extension = new RiscVExtensionInfo(flag, zFlag);

            if (Contains(extension) && Contains(dependencies))
            {
                // Append the extension only if it's a pure alias
                if (extension.MisaFlags is 0 && extension.ZFlags is 0)
                    sb.Append(alias);

                // Track implied dependencies
                impliedMisa |= dependencies.MisaFlags;
                impliedZ |= dependencies.ZFlags;
            }
        }

        var printMisa = (MisaFlags & ~impliedMisa) | RiscVExtensions.Integers;
        var printZ = ZFlags & ~impliedZ;

        // Append remaining Standard Extensions
        foreach (var (name, flag) in _sortedExtensions)
        {
            // I/G is already handled
            if (name is "I" or "G")
                continue;

            if (flag is not 0 &&
                printMisa.HasFlag(flag))
            {
                sb.Append(name);
            }
        }

        // Append Multi-letter extensions with '_'
        bool firstZ = true;
        foreach (var (name, flag) in _sortedZExtensions)
        {
            if (printZ.HasFlag(flag))
            {
                sb.Append(firstZ ? "_" : "_"); // Use '_' or '+' per preference
                sb.Append(name);
                firstZ = false;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Casts a <see cref="RiscVExtensions"/> to a <see cref="RiscVExtensionInfo"/>.
    /// </summary>
    public static implicit operator RiscVExtensionInfo(RiscVExtensions flags) => new(flags);

    /// <summary>
    /// Casts a <see cref="RiscVZExtensions"/> to a <see cref="RiscVExtensionInfo"/>.
    /// </summary>
    public static implicit operator RiscVExtensionInfo(RiscVZExtensions flags) => new(RiscVExtensions.None, flags);
}
