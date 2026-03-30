// Avishai Dernis 2025

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Assembler.Models.Abstract;

/// <summary>
/// A class for managing instruction lookup.
/// </summary>
public abstract class RiscVInstructionTableBase<TKey> : InstructionTableBase<TKey, RiscVInstructionMetaBase>
    where TKey : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVInstructionTableBase{TKey}"/> class.
    /// </summary>
    public RiscVInstructionTableBase(RiscVAssemblerConfig config) : base()
    {
        Config = config;

        Initialize(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Gets the config used to generate the instruction table.
    /// </summary>
    public RiscVAssemblerConfig Config { get; }

    /// <summary>
    /// Attempts to get an instruction by a key.
    /// </summary>
    public virtual bool TryGetInstruction(
        TKey key,
        int argCount,
        [NotNullWhen(true)] out List<RiscVInstructionMetaBase>? metadatas,
        out RiscVBaseVersion? requiredBase,
        out RiscVExtensions? requiredExtension)
    {
        requiredBase = null;
        requiredExtension = null;

        if (base.TryGetInstruction(key, out metadatas))
            return true;

        return false;
    }
}
