// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Zarem.Assembler.Models.Tables;

/// <summary>
/// A class for managing reference type lookup tables.
/// </summary>
public static class ReferenceTypeTable<TRef>
    where TRef : unmanaged, Enum
{
    private static readonly Dictionary<string, TRef> _refTable;

    static ReferenceTypeTable()
    {
        _refTable = [];

        foreach (var field in typeof(TRef).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = @field.GetCustomAttribute<JsonStringEnumMemberNameAttribute>();
            if (attr != null && @field.GetValue(null) is TRef value)
            {
                _refTable[attr.Name] = value;
            }
        }
    }

    /// <summary>
    /// Attempts to get a reference type by name.
    /// </summary>
    public static bool TryGetReferenceType(string name, out TRef refType)
        => _refTable.TryGetValue(name, out refType);
}
