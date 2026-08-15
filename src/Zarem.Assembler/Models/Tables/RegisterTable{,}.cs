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
    private static readonly Dictionary<TSet, RegisterSetAttribute> _setTable;
    private static readonly Dictionary<TSet, Regex> _regexTable;

    static RegisterTable()
    {
        _nameTables = [];
        _setTable = [];
        _regexTable = [];

        BuildTables();
    }

    /// <summary>
    /// Attempts to get a register by name and set.
    /// </summary>
    /// <param name="name">The name of the register.</param>
    /// <param name="set">The register set to lookup.</param>
    /// <param name="register">The register's index.</param>
    /// <param name="indexed">Whether or not the register was named by index.</param>
    /// <returns>Whether or not an register exists by that name.</returns>
    public static bool TryGetRegister(string name, TSet set, out TRegister register, out bool indexed)
    {
        register = default;
        indexed = false;

        // Attempt to lookup by name
        if (_nameTables.TryGetValue(set, out var nameTable) &&
            nameTable.TryGetValue(name, out register))
            return true;

        // Attempt to lookup by regex
        if (_regexTable.TryGetValue(set, out var regex))
        {
            var match = regex.Match(name);
            if (!match.Success)
                return false;

            // Use Group[1] to get just the digits, bypassing the prefix (x, f, etc.)
            var numStr = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
            if (byte.TryParse(numStr, out var num))
            {
                register = Unsafe.As<byte, TRegister>(ref num);
                indexed = true;
                return true;
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// Attempts to get a register by name.
    /// </summary>
    /// <param name="name">The name of the register.</param>
    /// <param name="register">The register's index.</param>
    /// <param name="set">Which register set the discovered register belongs to.</param>
    /// <param name="indexed">Whether or not the register was named by index.</param>
    /// <returns>Whether or not an register exists by that name.</returns>
    public static bool TryGetRegister(string name, out TRegister register, out TSet set, out bool indexed)
    {
        register = default;
        set = default;
        indexed = false;

        // Check for empty register
        if (name.Length == 0)
            return false;

        foreach (var s in _nameTables.Keys)
        {
            if (TryGetRegister(name, s, out var value, out indexed))
            {
                register = value;
                set = s;
                return true;
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
        if (_setTable.TryGetValue(set, out var attr) && attr.Format is not null)
        {
            return $"{string.Format(attr.Format, Convert.ToInt32(register))}";
        }

        // Absolute fallback
        return $"{Convert.ToInt32(register)}";
    }

    /// <summary>
    /// Attempts to get the number of registers in a set.
    /// </summary>
    public static int GetRegisterCount(TSet set)
    {
        if (!_setTable.TryGetValue(set, out var attr))
            return -1;

        return attr.RegisterCount;
    }

    private static void BuildTables()
    {
        foreach (var field in typeof(TSet).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<RegisterSetAttribute>();
            if (attr is null || field.GetValue(null) is not TSet value)
                continue;

            // Populate attr table
            _setTable[value] = attr;

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

            if (attr.Alias is not null)
            {
                byte x = Convert.ToByte(rawValue);
                table[attr.Alias] = Unsafe.As<byte, TRegister>(ref x);
            }
        }

        return table;
    }
}
