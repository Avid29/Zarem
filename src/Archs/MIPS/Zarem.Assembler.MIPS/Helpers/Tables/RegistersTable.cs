// Avishai Dernis 2025

using System.Collections.Generic;
using System.Linq;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Assembler.Helpers.Tables;

/// <summary>
/// A class containing a constant table for register lookup.
/// </summary>
public static class RegistersTable
{
    /// <summary>
    /// Attempts to get a register by name.
    /// </summary>
    /// <param name="name">The name of the register.</param>
    /// <param name="register">The register enum value.</param>
    /// <param name="set">Which register set table to reference.</param>
    /// <returns>Whether or not an register exists by that name.</returns>
    public static bool TryGetRegister(string name, out GPRegister register, out RegisterSet set)
    {
        register = GPRegister.Zero;
        set = RegisterSet.Numbered;
        
        // Trim the '$' prefix
        if (name.StartsWith('$'))
            name = name[1..];

        // Check for empty register
        if (name.Length == 0)
            return false;

        // Check for float register
        // NOTE: Checking "fp" explicitly is a bit of a hack, but it works (at least for now).
        if (name[0] == 'f' && name != "fp")
        {
            // Assign set as floating point then fall-through to numerical
            // register set parsing with the 'f' removed
            set = RegisterSet.FloatingPoints;
            name = name[1..];
        }

        // Check for numerical register
        if (byte.TryParse(name, out var num))
        {
            // Lowest register enum to highest register enum
            if (num is < (byte)GPRegister.Zero or > (byte)GPRegister.ReturnAddress)
                return false;

            // Cast num to register
            register = (GPRegister)num;
            return true;
        }

        if (_gpRegisterTable.TryGetValue(name, out register))
        {
            set = RegisterSet.GeneralPurpose;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to get a register's string by value.
    /// </summary>
    /// <param name="register">The register value.</param>
    /// <param name="set">The set the register belongs to.</param>
    /// <returns>The name of the register as a string.</returns>
    public static string GetRegisterString(GPRegister register, RegisterSet? set = RegisterSet.GeneralPurpose)
    {
        // Default to numbered.
        set ??= RegisterSet.Numbered;

        string name = set switch
        {
            // This is O(n) when it could easily be O(1). But n is 32, so idc.
            RegisterSet.GeneralPurpose => _gpRegisterTable.First(x => x.Value == register).Key,
            RegisterSet.FloatingPoints => $"f{(int)register}",
            //RegisterSet.CoProc0 => throw new System.NotImplementedException("CoProc0 registers not implemented."),    
            _ or RegisterSet.Numbered => $"{(int)register}",
        };

        // Prepend '$'
        return $"${name}";
    }

    private static readonly Dictionary<string, GPRegister> _gpRegisterTable = new()
    {
        { "zero", GPRegister.Zero },

        { "at", GPRegister.AssemblerTemporary },

        { "v0", GPRegister.ReturnValue0 },
        { "v1", GPRegister.ReturnValue1 },

        { "a0", GPRegister.Argument0 },
        { "a1", GPRegister.Argument1 },
        { "a2", GPRegister.Argument2 },
        { "a3", GPRegister.Argument3 },

        { "t0", GPRegister.Temporary0 },
        { "t1", GPRegister.Temporary1 },
        { "t2", GPRegister.Temporary2 },
        { "t3", GPRegister.Temporary3 },
        { "t4", GPRegister.Temporary4 },
        { "t5", GPRegister.Temporary5 },
        { "t6", GPRegister.Temporary6 },
        { "t7", GPRegister.Temporary7 },

        { "s0", GPRegister.Saved0 },
        { "s1", GPRegister.Saved1 },
        { "s2", GPRegister.Saved2 },
        { "s3", GPRegister.Saved3 },
        { "s4", GPRegister.Saved4 },
        { "s5", GPRegister.Saved5 },
        { "s6", GPRegister.Saved6 },
        { "s7", GPRegister.Saved7 },

        { "t8", GPRegister.Temporary8 },
        { "t9", GPRegister.Temporary9 },

        { "k0", GPRegister.Kernel0 },
        { "k1", GPRegister.Kernel1 },

        { "gp", GPRegister.GlobalPointer },
        { "sp", GPRegister.StackPointer },
        { "fp", GPRegister.FramePointer },

        { "ra", GPRegister.ReturnAddress },
    };
}
