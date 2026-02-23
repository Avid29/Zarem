// Avishai Dernis 2024

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zarem.Assembler.Config;
using Zarem.Assembler.Models.Abstract;
using Zarem.Assembler.Models.Enums;
using Zarem.Models.Instructions.Enums;

namespace Zarem.Assembler.Models;

/// <summary>
/// A class for managing instruction lookup by name.
/// </summary>
public class InstructionTable : InstructionTableBase<string>
{
    private readonly Dictionary<string, HashSet<MipsVersion>> _outOfVersion = [];
    private readonly HashSet<string> _banned = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionTable"/> class.
    /// </summary>
    public InstructionTable(MIPSAssemblerConfig config) : base(config)
    {
    }

    /// <inheritdoc/>
    public override bool TryGetInstruction(string name, [NotNullWhen(true)] out List<InstructionMetadata>? metadatas, out MipsVersion? requiredVersion, out bool banned)
    {
        banned = _banned.Contains(name);
        metadatas = null;
        requiredVersion = null;

        if (base.TryGetInstruction(name, out metadatas, out _, out _))
            return true;

        if (banned)
        {
            return false;
        }

        if (_outOfVersion.TryGetValue(name, out var versions))
        {
            // Higher version instruction. Get the lowest version available.
            if (Config.MipsVersion < versions.FirstOrDefault())
                requiredVersion = versions.Min();
            // Deprecated instruction. Get the highest version available.
            else
                requiredVersion = versions.Max();
        }

        return false;
    }

    /// <summary>
    /// Attempts to get an instruction by name.
    /// </summary>
    /// <param name="name">The name of the instruction.</param>
    /// <param name="argCount">The number of arguments for the instruction.</param>
    /// <param name="metadata">The instruction metadatas.</param>
    /// <param name="requiredVersion">The required version to have this instruction, if there is one.</param>
    /// <param name="banned">Indicates if the instruction was found, but is banned according the config.</param>
    /// <returns>Whether or not an instruction exists by that name</returns>
    public bool TryGetInstruction(string name, int argCount, out InstructionMetadata metadata, out MipsVersion? requiredVersion, out bool banned)
    {
        metadata = default;

        if (TryGetInstruction(name, out var metadatas, out requiredVersion, out banned))
        {
            if (metadatas is null)
                return false;

            if (!metadatas.Any(x => x.ArgumentPattern.Length == argCount))
                return false;

            metadata = metadatas.FirstOrDefault(x => x.ArgumentPattern.Length == argCount);
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    protected override void LoadInstruction(InstructionMetadata metadata)
    {
        if (metadata.IsPseudoInstruction && Config.PseudoInstructionPermissibility is not null)
        {
            var blacklist = Config.PseudoInstructionPermissibility is PseudoInstructionPermissibility.Blacklist;
            var listed = Config.PseudoInstructionSet?.Contains(metadata.Name);

            // If blacklist and listed, ban it
            // If whitelist and not listed, also ban it
            // Otherwise, it's allowed
            if (blacklist == listed)
            {
                _banned.Add(metadata.Name);
            }
        }


        if (metadata.MIPSVersions.Contains(Config.MipsVersion))
        {
            LoadInstruction(metadata.Name, metadata);
        }
        else if (!_outOfVersion.ContainsKey(metadata.Name))
        {
            _outOfVersion.Add(metadata.Name, metadata.MIPSVersions);
        }
    }
}
