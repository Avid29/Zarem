// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Reflection;
using Zarem.Attributes.Arguments;

namespace Zarem.Assembler.Models.Tables;

/// <summary>
/// A class for managing argument lookup tables.
/// </summary>
public static class ArgumentTable<TArg>
    where TArg : unmanaged, Enum
{
    private static readonly Dictionary<TArg, ArgumentAttribute> _attributeTable;

    static ArgumentTable()
    {
        _attributeTable = [];

        foreach (var @field in typeof(TArg).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = @field.GetCustomAttribute<ArgumentAttribute>();
            if (attr != null && @field.GetValue(null) is TArg value)
            {
                _attributeTable[value] = attr;
            }
        }
    }

    /// <summary>
    /// Gets the attribute for <typeparamref name="TArg"/> <paramref name="argument"/>.
    /// </summary>
    public static ArgumentAttribute? GetAttribute(TArg argument)
    {
        _attributeTable.TryGetValue(argument, out var value);
        return value;
    }
}
