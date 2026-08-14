// Avishai Dernis 2024

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zarem.Models.Versioning;
using Zarem.RiscV.Assembler.Models.Meta;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.RiscV.Assembler.Models.Tables;

/// <summary>
/// A class for managing instruction lookup by name.
/// </summary>
public class RiscVInstructionTable : RiscVInstructionTableBase<string>
{
    private readonly Dictionary<string, RiscVVersionInfo> _metadataLookup = [];

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
        out RiscVExtensionInfo? requiredExtension)
    {
        requiredBase = null;
        requiredExtension = null;

        if (base.TryGetInstruction(name, out metadatas))
            return true;

        if (_metadataLookup.TryGetValue(name, out var version))
        {
            if (Config.VersionInfo.Base < version.Base)
                requiredBase = version.Base;

            if (!Config.VersionInfo.HasExtensions(version.Extensions))
                requiredExtension = version.Extensions;
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
        out RiscVExtensionInfo? requiredExtension)
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
        _metadataLookup[metadata.Name] = new(metadata.MinBase, metadata.Extension);
    }
}
