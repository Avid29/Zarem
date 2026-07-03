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
public abstract class RegisterTable<TRegister, TSet, TCategory> : RegisterTable<TRegister, TSet>
    where TRegister : unmanaged, Enum
    where TSet : unmanaged, Enum
    where TCategory : unmanaged, Enum
{
    /// <summary>
    /// Attempts to get a register's category by value.
    /// </summary>
    /// <param name="register">The register value.</param>
    /// <param name="set">The set the register belongs to.</param>
    /// <returns>The category of the register.</returns>
    /// <exception cref="ArgumentException"></exception>
    public TCategory GetRegisterCategory(TRegister register, TSet set)
    {
        if (RegisterCategoryTable.TryGetValue(set, out var table) && table.TryGetValue(register, out var category))
            return category;

        // TODO: Should this just return default?
        throw new ArgumentException($"Register {register} in set {set} does not have a category defined.");
    }

    /// <summary>
    /// Gets a dictionary mapping a set and register to a category.
    /// </summary>
    protected abstract Dictionary<TSet, Dictionary<TRegister, TCategory>> RegisterCategoryTable { get; }
}
