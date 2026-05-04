// Avishai Dernis 2025

using System;
using Zarem.Mips.Models.Instructions.Enums;

namespace Zarem.Mips.Assembler.Helpers.Tables;

/// <summary>
/// A class containing a constant table for argument lookup.
/// </summary>
public static class ArgumentTable
{
    /// <summary>
    /// Gets the <see cref="MipsArgument"/> as a usage pattern string.
    /// </summary>
    public static string GetArgPatternString(MipsArgument argument) => _argumentTable[(int)argument];

    /// <summary>
    /// Attempts to get an argument by name.
    /// </summary>
    /// <param name="name">The name of the argument.</param>
    /// <param name="argument">The argument enum value.</param>
    /// <returns>Whether or not an argument exists by that name.</returns>
    public static bool TryGetArgument(string name, out MipsArgument argument)
    {
        name = name.Trim();
        argument = (MipsArgument)Array.IndexOf(_argumentTable, name);
        return argument is not (MipsArgument)(-1);
    }

    private static string[] _argumentTable =
    {
        "$rs",
        "$rt",
        "$rd",
        "shift",
        "immediate",
        "offset",
        "address",
        "offset(base)",
        "immediate",
        "$fs",
        "$ft",
        "$fd"
    };
}
