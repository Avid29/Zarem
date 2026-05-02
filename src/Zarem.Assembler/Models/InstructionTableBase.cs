// Avishai Dernis 2025

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Zarem.Assembler.Models;

/// <summary>
/// A class for managing instruction lookup.
/// </summary>
public abstract class InstructionTableBase<TKey, TEntry>
    where TKey : notnull
    where TEntry : IInstructionMeta
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionTableBase{TKey, TEntry}"/> class.
    /// </summary>
    public InstructionTableBase()
    {
        LookupTable = [];
    }

    /// <summary>
    /// The table of elements in the instruction table.
    /// </summary>
    protected Dictionary<TKey, List<TEntry>> LookupTable { get; }

    /// <summary>
    /// Attempts to get an instruction by a key.
    /// </summary>
    /// <param name="key">The key to lookup the instruction.</param>
    /// <param name="metadatas">The metadatas of matching instructions.</param>
    /// <returns>Whether or not an instruction exists by that name</returns>
    public virtual bool TryGetInstruction(TKey key, [NotNullWhen(true)] out List<TEntry>? metadatas)
    {
        if (LookupTable.TryGetValue(key, out metadatas))
            return true;

        return false;
    }

    /// <summary>
    /// Gets all instructions in the instruction table.
    /// </summary>
    /// <returns>An array of the instructions in the table.</returns>
    public virtual TEntry[] GetInstructions(bool maxArgs = true)
    {
        if (maxArgs)
        {
            return [..LookupTable.Values
                .Select(x => x
                    .OrderByDescending(x => x.ArgumentCount)
                    .First())];
        }

        return [.. LookupTable.Values.SelectMany(x => x)];
    }

    /// <summary>
    /// Initialzes the instruction table by loading all instruction sets from the assembly's resources.
    /// </summary>
    protected void Initialize(Assembly assembly)
    {
        var resources = assembly.GetManifestResourceNames();
        resources = [.. resources.Where(x => x.EndsWith("inst.json"))];

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
    protected abstract void LoadInstruction(TEntry metadata);

    /// <summary>
    /// Loads an instruction into the <see cref="LookupTable"/>.
    /// </summary>
    protected void LoadInstruction(TKey key, TEntry metadata)
    {
        if (!LookupTable.TryGetValue(key, out List<TEntry>? instructions))
        {
            instructions = [];
            LookupTable.Add(key, instructions);
        }

        instructions.Add(metadata);
    }

    private static TEntry[] LoadInstructionSet(Assembly assembly, string resourceName)
    {
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return [];

        var instructions = JsonSerializer.Deserialize<TEntry[]>(stream);
        if (instructions is null)
            return [];

        return instructions;
    }
}
