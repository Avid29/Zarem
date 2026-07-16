// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Zarem.Attributes.Register;

namespace Zarem.Assembler.Models.Tables;

/// <summary>
/// A base class for a register lookup table.
/// </summary>
public static class RegisterTable<TRegister, TSet>
    where TRegister : unmanaged, Enum
    where TSet : unmanaged, Enum
{
    private static readonly Dictionary<TSet, Dictionary<string, TRegister>> _nameTables;
    private static readonly Dictionary<TSet, string> _formatTable;
    private static readonly Dictionary<TSet, Regex> _regexTable;

    static RegisterTable()
    {
        _nameTables = [];
        _formatTable = [];
        _regexTable = [];

        BuildTables();
    }

    /// <summary>
    /// Attempts to get a register by name.
    /// </summary>
    /// <param name="name">The name of the register.</param>
    /// <param name="register">The register's index.</param>
    /// <param name="registerSet">Which register set the discovered register belongs to.</param>
    /// <param name="indexed">Whether or not the register was named by index.</param>
    /// <returns>Whether or not an register exists by that name.</returns>
    public static bool TryGetRegister(string name, out TRegister register, out TSet registerSet, out bool indexed)
    {
        register = default;
        registerSet = default;
        indexed = false;

        // Check for empty register
        if (name.Length == 0)
            return false;

        foreach (var (set, table) in _nameTables)
        {
            if (table.TryGetValue(name, out var value))
            {
                register = value;
                registerSet = set;
                return true;
            }
        }

        foreach (var (set, regex) in _regexTable)
        {
            var match = regex.Match(name);
            if (match.Success)
            {
                // Use Group[1] to get just the digits, bypassing the prefix (x, f, etc.)
                var numStr = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
                if (byte.TryParse(numStr, out var num))
                {
                    register = Unsafe.As<byte, TRegister>(ref num);
                    registerSet = set;
                    indexed = true;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to get a register's string by value.
    /// </summary>
    /// <param name="register">The register value.</param>
    /// <param name="set">The set the register belongs to.</param>
    /// <returns>The name of the register as a string.</returns>
    public static string GetRegisterString(TRegister register, TSet set)
    {
        // Try to find a the ABI name
        if (_nameTables.TryGetValue(set, out var table))
        {
            // O(n) lookup for the name, but n is small (32)
            var name = table?.FirstOrDefault(x => EqualityComparer<TRegister>.Default.Equals(x.Value, register)).Key;
            if (name != null)
            {
                return $"{name}";
            }
        }

        // Fallback to Numerical name (x10, f10)
        if (_formatTable.TryGetValue(set, out var format))
        {
            return $"{string.Format(format, Convert.ToInt32(register))}";
        }

        // Absolute fallback
        return $"{Convert.ToInt32(register)}";
    }

    private static void BuildTables()
    {
        foreach (var field in typeof(TSet).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<RegisterSetAttribute>();
            if (attr is null || field.GetValue(null) is not TSet value)
                continue;

            // Populate format table
            if (!string.IsNullOrEmpty(attr.Format))
            {
                _formatTable[value] = attr.Format;
            }

            // Populate regex table
            if (!string.IsNullOrEmpty(attr.Regex))
            {
                _regexTable[value] = new Regex(attr.Regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            // Populate named table
            if (attr.SetType is not null)
            {
                var nameSubTable = BuildNameTable(attr.SetType);
                if (nameSubTable.Count is 0)
                    continue;

                _nameTables[value] = nameSubTable;
            }
        }
    }

    private static Dictionary<string, TRegister> BuildNameTable(Type setType)
    {
        var table = new Dictionary<string, TRegister>();

        foreach (var field in setType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<RegisterAttribute>();
            if (attr is null)
                continue;

            var rawValue = field.GetValue(null);
            if (rawValue is null)
                continue;

            byte x = Convert.ToByte(rawValue);
            table[attr.Alias] = Unsafe.As<byte, TRegister>(ref x);
        }

        return table;
    }
}
