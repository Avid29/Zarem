// Avishai Dernis 2025

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Zarem.Assembler.Config;
using Zarem.Models.Instructions.Enums;

namespace Zarem.Assembler.Models.Abstract;

/// <summary>
/// A class for managing instruction lookup.
/// </summary>
public abstract class InstructionTableBase<T>
    where T : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionTable"/> class.
    /// </summary>
    public InstructionTableBase(MipsAssemblerConfig config)
    {
        Config = config;

        LookupTable = [];
        Initialize();
    }

    /// <summary>
    /// Gets the config used to generate the instruction table.
    /// </summary>
    public MipsAssemblerConfig Config { get; }

    /// <summary>
    /// The table of elements in the instruction table.
    /// </summary>
    protected Dictionary<T, List<MipsInstructionMetaBase>> LookupTable { get; }

    /// <summary>
    /// Attempts to get an instruction by a key.
    /// </summary>
    /// <param name="key">The key to lookup the instruction.</param>
    /// <param name="metadatas">The metadatas of matching instructions.</param>
    /// <param name="requiredVersion">The required version to have this instruction, if there is one.</param>
    /// <param name="banned">Indicates if the instruction was found, but is banned according the config.</param>
    /// <returns>Whether or not an instruction exists by that name</returns>
    public virtual bool TryGetInstruction(T key, [NotNullWhen(true)] out List<MipsInstructionMetaBase>? metadatas, out MipsVersion? requiredVersion, out bool banned)
    {
        requiredVersion = null;
        banned = false;

        if (LookupTable.TryGetValue(key, out metadatas))
            return true;

        return false;
    }

    /// <summary>
    /// Gets all instructions in the instruction table.
    /// </summary>
    /// <returns>An array of the instructions in the table.</returns>
    public MipsInstructionMetaBase[] GetInstructions(bool maxArgs = true)
    {
        if (maxArgs)
        {
            return [..LookupTable.Values
                .Select(x => x
                    .OrderByDescending(x => x.ArgumentPattern.Length)
                    .First())];
        }

        return [..LookupTable.Values.SelectMany(x => x)];
    }

    private void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resources = assembly.GetManifestResourceNames();
        resources = [..resources.Where(x => x.EndsWith("Instructions.json"))];

        foreach (var resource in resources)
        {
            var instructions = LoadInstructionSet(assembly, resource);

            foreach (var instruction in instructions)
            {
                LoadInstruction(instruction);
            }
        }
    }

    /// <summary>
    /// Loads an instruction into the <see cref="LookupTable"/>.
    /// </summary>
    /// <param name="metadata">The metadata of the instruction.</param>
    protected abstract void LoadInstruction(MipsInstructionMetaBase metadata);

    /// <summary>
    /// Loads an instruction into the <see cref="LookupTable"/>.
    /// </summary>
    protected void LoadInstruction(T key, MipsInstructionMetaBase metadata)
    {
        if (!LookupTable.TryGetValue(key, out List<MipsInstructionMetaBase>? instructions))
        {
            instructions = [];
            LookupTable.Add(key, instructions);
        }

        instructions.Add(metadata);
    }

    private static MipsInstructionMetaBase[] LoadInstructionSet(Assembly assembly, string resourceName)
    {
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return [];

        var instructions = JsonSerializer.Deserialize<MipsInstructionMetaBase[]>(stream);
        if (instructions is null)
            return [];

        return instructions;
    }
}
