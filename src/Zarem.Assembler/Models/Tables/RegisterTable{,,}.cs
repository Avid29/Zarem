// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Zarem.Attributes.Register;

namespace Zarem.Assembler.Models.Tables;

/// <summary>
/// A base class for a register lookup table.
/// </summary>
public static class RegisterTable<TRegister, TSet, TCategory>
    where TRegister : unmanaged, Enum
    where TSet : unmanaged, Enum
    where TCategory : unmanaged, Enum
{
    private static readonly Dictionary<TSet, Dictionary<TRegister, TCategory>> _categoryTable;

    static RegisterTable()
    {
        _categoryTable = BuildCategoryTable();
    }

    /// <summary>
    /// Attempts to get a register's category by value.
    /// </summary>
    /// <param name="register">The register value.</param>
    /// <param name="set">The set the register belongs to.</param>
    /// <returns>The category of the register.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static TCategory GetRegisterCategory(TRegister register, TSet set)
    {
        if (_categoryTable.TryGetValue(set, out var table) && table.TryGetValue(register, out var category))
            return category;

        // TODO: Should this just return default?
        throw new ArgumentException($"Register {register} in set {set} does not have a category defined.");
    }

    private static Dictionary<TSet, Dictionary<TRegister, TCategory>> BuildCategoryTable()
    {
        var table = new Dictionary<TSet, Dictionary<TRegister, TCategory>>();

        foreach(var field in typeof(TSet).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<RegisterSetAttribute>();
            if (attr?.SetType is null || field.GetValue(null) is not TSet value)
                continue;

            var subTable = BuildSetCategoryTable(attr.SetType);
            if (subTable.Count is 0)
                continue;

            table[value] = subTable;
        }

        return table;
    }

    private static Dictionary<TRegister, TCategory> BuildSetCategoryTable(Type setType)
    {
        var table = new Dictionary<TRegister, TCategory>();

        foreach (var field in setType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<RegisterAttribute<TCategory>>();
            if (attr is null)
                continue;

            var rawValue = field.GetValue(null);
            if (rawValue is null)
                continue;

            byte x = Convert.ToByte(rawValue);
            table[Unsafe.As<byte, TRegister>(ref x)] = attr.Category;
        }

        return table;
    }
}
