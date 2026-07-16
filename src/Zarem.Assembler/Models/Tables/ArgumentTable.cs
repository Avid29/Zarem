// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Reflection;
using Zarem.Attributes;

namespace Zarem.Assembler.Models.Tables;

/// <summary>
/// A class for managing argument lookup tables.
/// </summary>
public class ArgumentTable<TArg>
    where TArg : unmanaged, Enum
{
    private readonly Dictionary<TArg, AssemblerArgumentAttribute> _attributeTable;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentTable{TArg}"/> class.
    /// </summary>
    public ArgumentTable()
    {
        _attributeTable = [];

        foreach (var field in typeof(TArg).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<AssemblerArgumentAttribute>();
            if (attr != null && field.GetValue(null) is TArg value)
            {
                _attributeTable[value] = attr;
            }
        }
    }

    /// <summary>
    /// Gets the attribute for <typeparamref name="TArg"/> <paramref name="argument"/>.
    /// </summary>
    public AssemblerArgumentAttribute? GetAttribute(TArg argument)
    {
        _attributeTable.TryGetValue(argument, out var value);
        return value;
    }
}
