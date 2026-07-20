// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Zarem.Assembler.Tokenization.Profiles;
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

    /// <summary>
    /// Gets the usage display for an argument.
    /// </summary>
    public static string GetDisplay(TArg argument, ITokenizerProfile profile)
    {
        var attr = GetAttribute(argument);
        Guard.IsNotNull(attr);

        // Special case: Split
        if (attr is SplitArgumentAttribute<TArg> split)
        {
            var imm = GetDisplay(split.ImmediateArgument, profile);
            var reg = GetDisplay(split.RegisterArgument, profile);
            return $"{imm}({reg})";
        }

        char prefix = '\0';
        var openAttrType = attr.GetType().GetGenericTypeDefinition();
        if (openAttrType == typeof(RegisterArgumentAttribute<>)) prefix = profile.RegisterPrefix;
        if (openAttrType == typeof(ImmediateArgumentAttribute<>)) prefix = profile.ImmediatePrefix;

        return prefix is '\0' ? attr.Alias : $"{prefix}{attr.Alias}";
    }
}
