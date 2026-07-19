// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Zarem.Attributes;

namespace Zarem.Assembler.Models.Tables;

/// <summary>
/// A class for managing reference type lookup tables.
/// </summary>
public static class ReferenceTypeTable<TRef>
    where TRef : unmanaged, Enum
{
    private static readonly Dictionary<string, TRef> _refTable;
    private static readonly Dictionary<TRef, ReferenceTypeAttribute> _refAttrTable;

    static ReferenceTypeTable()
    {
        _refTable = [];
        _refAttrTable = [];

        foreach (var field in typeof(TRef).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = @field.GetCustomAttribute<ReferenceTypeAttribute>();
            if (attr != null && @field.GetValue(null) is TRef value)
            {
                _refAttrTable[value] = attr;
                if (attr.Alias is not null)
                {
                    _refTable[attr.Alias] = value;
                }
            }
        }
    }

    /// <summary>
    /// Attempts to get a reference type by alias.
    /// </summary>
    public static bool TryGetReferenceType(string alias, out TRef refType)
        => _refTable.TryGetValue(alias, out refType);

    /// <summary>
    /// Attempts to get a reference type attribute by value.
    /// </summary>
    public static bool TryGetReferenceType(TRef value, [NotNullWhen(true)] out ReferenceTypeAttribute? attr)
        => _refAttrTable.TryGetValue(value, out attr);
}
