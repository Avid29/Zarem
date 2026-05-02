// Avishai Dernis 2024

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zarem.Assembler.Models.Abstract;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Assembler.Models;

/// <summary>
/// A class for managing instruction lookup by name.
/// </summary>
public class RiscVInstructionTable : RiscVInstructionTableBase<string>
{
    private readonly Dictionary<string, (RiscVBaseVersion Base, RiscVExtensions Extension)> _metadataLookup = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVInstructionTable"/> class.
    /// </summary>
    public RiscVInstructionTable(RiscVAssemblerConfig config) : base(config)
    {
    }

    /// <summary>
    /// Attempts to get an instruction by name.
    /// </summary>
    public bool TryGetInstruction(
        string name,
        [NotNullWhen(true)] out List<RiscVInstructionMetaBase>? metadatas,
        out RiscVBaseVersion? requiredBase,
        out RiscVExtensions? requiredExtension)
    {
        requiredBase = null;
        requiredExtension = null;

        if (base.TryGetInstruction(name, out metadatas))
            return true;


        if (_metadataLookup.TryGetValue(name, out var version))
        {
            if (Config.VersionInfo.Base < version.Base)
                requiredBase = version.Base;

            if (!Config.VersionInfo.Extensions.HasFlag(version.Extension))
                requiredExtension = version.Extension;
        }

        return false;
    }

    /// <summary>
    /// Attempts to get an instruction by name.
    /// </summary>
    public bool TryGetInstruction(
        string name,
        int argCount,
        [NotNullWhen(true)] out RiscVInstructionMetaBase? metadata,
        out RiscVBaseVersion? requiredBase,
        out RiscVExtensions? requiredExtension)
    {
        metadata = null;
        if (TryGetInstruction(name, out var metadatas, out requiredBase, out requiredExtension))
        {
            metadata = metadatas.FirstOrDefault(m => m.ArgumentPattern.Length == argCount);
            return metadata is not null;
        }

        return false;
    }

    /// <inheritdoc/>
    protected override void LoadInstruction(RiscVInstructionMetaBase metadata)
    {
        // Check if the current config allows this specific instruction
        if (metadata.IsValidFor(Config.VersionInfo))
            LoadInstruction(metadata.Name, metadata);

        // Track metadata for diagnostics even if currently disabled
        _metadataLookup[metadata.Name] = (metadata.MinBase, metadata.Extension);
    }
}
