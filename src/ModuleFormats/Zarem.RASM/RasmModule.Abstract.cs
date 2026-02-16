// Avishai Dernis 2025

using System.Collections.Generic;
using Zarem.Extensions.System.IO;
using Zarem.Models;
using Zarem.Models.Tables;
using Zarem.Models.Tables.Enums;
using Zarem.RASM.Config;
using RasmReference = Zarem.RASM.Tables.ReferenceEntry;
using RasmRelocation = Zarem.RASM.Tables.RelocationEntry;
using RasmSymbol = Zarem.RASM.Tables.SymbolEntry;
using ReferenceEntry = Zarem.Models.Tables.ReferenceEntry;
using Symbol = Zarem.Models.Tables.Symbol;

namespace ObjFormats.RASM;

public partial class RasmModule
{
    /// <inheritdoc/>
    public Module? Abstract(RasmConfig config)
    {
        // Validate the header for this assembly
        if (!ValidateHeader(config))
            return null;

        // Return to start of module
        ResetStream();

        // Grab section sizes
        var sizes = new uint[]
        {
            Header.TextSize,
            Header.ReadOnlyDataSize,
            Header.DataSize,
            Header.SmallDataSize,
            Header.SmallUninitializedDataSize,
            Header.UninitializedDataSize
        };

        // Copy sections
        var sections = new Dictionary<string, Section>();
        for (int i = 0; i < SectionNames.Length; i++)
        {
            var sectionName = SectionNames[i];
            var size = (int)sizes[i];
            sections.Add(sectionName, new Section(sectionName, SectionFlags.Default));
            var section = sections[sectionName];
            if (size is not 0)
            {
                section.Stream.CopyFrom(_source, size);
            }
        }

        // Load table entries
        var relocations = new RasmRelocation[Header.RelocationTableCount];
        var references = new RasmReference[Header.ReferenceTableCount];
        var symbols = new RasmSymbol[Header.DefinitionsTableCount];
        for (int i = 0; i < relocations.Length; i++)
            relocations[i] = relocations[i].Read(_source);
        for (int i = 0; i < references.Length; i++)
            references[i] = references[i].Read(_source);
        for (int i = 0; i < symbols.Length; i++)
            symbols[i] = symbols[i].Read(_source);

        // Get string table
        var strings = LoadStrings();

        // Initialize the tables
        var referenceList = new List<ReferenceEntry>();
        var symbolTable = new Dictionary<string, Symbol>();

        // Convert relocations
        foreach (var rel in relocations)
            referenceList.Add(rel.Convert(null));

        // Convert references
        foreach (var @ref in references)
        {
            var name = strings[(int)@ref.SymbolIndex];
            var reference = @ref.Convert(name);
            referenceList.Add(reference);
        }

        // Convert symbols
        foreach (var sym in symbols)
        {
            var name = strings[(int)sym.SymbolIndex];
            var symbol = sym.Convert(name);
            symbolTable.Add(symbol.Name, symbol);
        }

        // Create constructor from the sections
        return new Module(sections, referenceList, symbolTable);
    }
}
