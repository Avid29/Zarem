// Avishai Dernis 2024

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zarem.Assembler.Config;
using Zarem.Assembler.Models.Abstract;
using Zarem.Assembler.Models.Enums;
using Zarem.Assembler.Models.Meta;
using Zarem.Models.Instructions.Enums;

namespace Zarem.Assembler.Models;

/// <summary>
/// A class for managing instruction lookup by name.
/// </summary>
public class MipsInstructionTable : MipsInstructionTableBase<string>
{
    private readonly Dictionary<string, (MipsVersion Min, MipsVersion? Max)> _versionRanges = [];
    private readonly HashSet<string> _banned = [];
    private readonly HashSet<string> _is64bitLookup = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionTable"/> class.
    /// </summary>
    public MipsInstructionTable(MipsAssemblerConfig config) : base(config)
    {
    }

    /// <inheritdoc/>
    public override bool TryGetInstruction(string name, [NotNullWhen(true)] out List<MipsInstructionMetaBase>? metadatas, out MipsVersion? requiredVersion, out bool is64bit, out bool banned)
    {
        banned = _banned.Contains(name);
        is64bit = _is64bitLookup.Contains(name);
        requiredVersion = null;

        if (base.TryGetInstruction(name, out metadatas, out _, out _, out _))
            return true;

        if (_versionRanges.TryGetValue(name, out var range))
        {
            if (Config.MipsVersion < range.Min)
            {
                // Instruction exists in a future version
                requiredVersion = range.Min;
            }
            else if (range.Max.HasValue && Config.MipsVersion >= range.Max.Value)
            {
                // Instruction was removed/obsolete in a past version
                // We return the last valid version it was in
                requiredVersion = range.Max.Value;
            }
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
    /// <param name="is64bit">Whether or not the instruction requires 64-bit MIPS.</param>
    /// <param name="banned">Indicates if the instruction was found, but is banned according the config.</param>
    /// <returns>Whether or not an instruction exists by that name</returns>
    public bool TryGetInstruction(string name, int argCount, out MipsInstructionMetaBase? metadata, out MipsVersion? requiredVersion, out bool is64bit, out bool banned)
    {
        metadata = null;

        if (TryGetInstruction(name, out var metadatas, out requiredVersion, out is64bit, out banned))
        {
            metadata = metadatas.FirstOrDefault(x => x.ArgumentPattern.Length == argCount);
            return metadata is not null;
        }

        return false;
    }

    /// <inheritdoc/>
    protected override void LoadInstruction(MipsInstructionMetaBase metadata)
    {
        // Handle Banning (Pseudo-instruction logic)
        if (metadata is PseudoInstructionMeta && Config.PseudoInstructionPermissibility is not null)
        {
            bool isBlacklist = Config.PseudoInstructionPermissibility == PseudoInstructionPermissibility.Blacklist;
            bool isInList = Config.PseudoInstructionSet?.Contains(metadata.Name) ?? false;

            if (isBlacklist == isInList)
            {
                _banned.Add(metadata.Name);
                return; // If banned, we don't even track it
            }
        }

        bool isSupported = metadata.IsValidFor(Config.MipsVersion);
        if (isSupported)
        {
            // Add to the active lookup table in InstructionTableBase
            LoadInstruction(metadata.Name, metadata);
        }

        if (metadata.Is64Bit)
        {
            _is64bitLookup.Add(metadata.Name);
        }

        // Track version ranges for error reporting/diagnostics
        if (!_versionRanges.TryGetValue(metadata.Name, out var range))
        {
            _versionRanges[metadata.Name] = (metadata.AddedIn, metadata.RemovedIn);
        }
        else
        {
            // Expand the known range for this instruction name
            var newMin = metadata.AddedIn < range.Min ? metadata.AddedIn : range.Min;
            var newMax = (metadata.RemovedIn > range.Max || !metadata.RemovedIn.HasValue)
                         ? metadata.RemovedIn
                         : range.Max;
            _versionRanges[metadata.Name] = (newMin, newMax);
        }
    }
}
