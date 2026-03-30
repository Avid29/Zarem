// Avishai Dernis 2025

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Zarem.Assembler.Config;
using Zarem.Models.Instructions.Enums;

namespace Zarem.Assembler.Models.Abstract;

/// <summary>
/// A class for managing instruction lookup.
/// </summary>
public abstract class MipsInstructionTableBase<TKey> : InstructionTableBase<TKey, MipsInstructionMetaBase>
    where TKey : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionTable"/> class.
    /// </summary>
    public MipsInstructionTableBase(MipsAssemblerConfig config) : base()
    {
        Config = config;

        Initialize(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Gets the config used to generate the instruction table.
    /// </summary>
    public MipsAssemblerConfig Config { get; }

    /// <summary>
    /// Attempts to get an instruction by a key.
    /// </summary>
    /// <param name="key">The key to lookup the instruction.</param>
    /// <param name="metadatas">The metadatas of matching instructions.</param>
    /// <param name="requiredVersion">The required version to have this instruction, if there is one.</param>
    /// <param name="is64bit">Whether or not the instruction requires 64-bit MIPS.</param>
    /// <param name="banned">Indicates if the instruction was found, but is banned according the config.</param>
    /// <returns>Whether or not an instruction exists by that name</returns>
    public virtual bool TryGetInstruction(TKey key, [NotNullWhen(true)] out List<MipsInstructionMetaBase>? metadatas, out MipsVersion? requiredVersion, out bool is64bit, out bool banned)
    {
        requiredVersion = null;
        is64bit = false;
        banned = false;

        if (base.TryGetInstruction(key, out metadatas))
            return true;

        return false;
    }
}
