// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Zarem.Assembler.Models.Tables;

/// <summary>
/// A base class for a register lookup table.
/// </summary>
public abstract class RegisterTable<TRegister, TSet>
    where TRegister : unmanaged, Enum
    where TSet : unmanaged, Enum
{
    static RegisterTable()
    {

    }

    /// <summary>
    /// Attempts to get a register by name.
    /// </summary>
    /// <param name="name">The name of the register.</param>
    /// <param name="register">The register's index.</param>
    /// <param name="registerSet">Which register set the discovered register belongs to.</param>
    /// <param name="indexed">Whether or not the register was named by index.</param>
    /// <returns>Whether or not an register exists by that name.</returns>
    public bool TryGetRegister(string name, out TRegister register, out TSet registerSet, out bool indexed)
    {
        register = default;
        registerSet = default;
        indexed = false;

        // Check for empty register
        if (name.Length == 0)
            return false;

        foreach (var (set, table) in NamedRegisterTables)
        {
            if (table.TryGetValue(name, out var value))
            {
                register = value;
                registerSet = set;
                return true;
            }
        }

        foreach (var (set, regex) in NumericalSetRegexTable)
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
    public string GetRegisterString(TRegister register, TSet set)
    {
        // Try to find a the ABI name
        if (NamedRegisterTables.TryGetValue(set, out var table))
        {
            // O(n) lookup for the name, but n is small (32)
            var name = table?.FirstOrDefault(x => EqualityComparer<TRegister>.Default.Equals(x.Value, register)).Key;
            if (name != null)
            {
                return $"{name}";
            }
        }

        // Fallback to Numerical name (x10, f10)
        if (NumericalSetFormatTable.TryGetValue(set, out var format))
        {
            return $"{string.Format(format, Convert.ToInt32(register))}";
        }

        // Absolute fallback
        return $"{Convert.ToInt32(register)}";
    }

    /// <summary>
    /// Gets a dictionary of register tables to reference for named register lookups.
    /// </summary>
    protected abstract Dictionary<TSet, Dictionary<string, TRegister>> NamedRegisterTables { get; }

    /// <summary>
    /// Gets a dictionary of regex expressions to match sets to numeric registers.
    /// </summary>
    protected abstract Dictionary<TSet, Regex> NumericalSetRegexTable { get; }

    /// <summary>
    /// Gets a dictionary mapping a set to a format string, e.g. RegisterSet.GP -> "x{0}"
    /// </summary>
    protected abstract Dictionary<TSet, string> NumericalSetFormatTable { get; }
}
