// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using LibObjectFile.Elf;
using ObjFormats.LibOF.Extensions;
using Zarem.Elf.Config;
using Zarem.Models;
using Zarem.Models.Tables;
using Zarem.Models.Tables.Enums;

namespace Zarem.Elf;

public partial class ElfModule
{
    private ref struct ElfAbstractContext
    {
        private ElfSymbolTable? _symTab;

        public ElfAbstractContext(ElfFile elfFile)
        {
            var archName = elfFile.Arch.Value switch
            {
                ElfArch.MIPS => "MIPS",
                _ => null
            };

            Guard.IsNotNull(archName);
            Module = new(archName);
        }

        public Module Module { get; }

        public bool AbstractStreamSection(ElfStreamSection streamSection)
        {
            var sectionName = streamSection.Name.Value;
            Module.GetOrCreateSection(sectionName, stream: streamSection.Stream);
            return true;
        }

        public bool AbstractSymbolTable(ElfSymbolTable symbolTable)
        {
            _symTab = symbolTable;

            foreach (var elfSymbol in symbolTable.Entries)
            {
                var name = elfSymbol.Name.Value;
                if (name is null)
                    continue;

                var sectionName = elfSymbol.SectionLink.Section?.Name.Value;
                Section? section = sectionName is not null ? Module.GetOrCreateSection(sectionName) : null;

                var value = new Address(section, (long)elfSymbol.Value);
                var binding = elfSymbol.Bind.FromElf();
                var symbol = Module.GetOrCreateSymbol(name);
                symbol.Address = value;
                symbol.Binding = binding;
            }

            return true;
        }

        public bool AbstractRelocationTable(ElfRelocationTable relocationTable)
        {
            if (_symTab is null)
                return false;

            var sectionName = relocationTable.Info.Section?.Name.Value;
            if (sectionName is null)
                return false;

            var section = Module.GetOrCreateSection(sectionName);

            foreach (var relEntry in relocationTable.Entries)
            {
                var symbol = _symTab.Entries[(int)relEntry.SymbolIndex];
                var address = new Address(section, (long)relEntry.Offset);
                section.AddRelocation(new RelocationEntry(symbol.Name.Value ?? string.Empty, address, relEntry.Type.Value, relEntry.Addend));
            }

            return true;
        }
    }

    /// <inheritdoc/>
    public Module? Abstract(ElfConfig config)
    {
        var context = new ElfAbstractContext(_elfFile);

        foreach (var section in _elfFile.Sections)
        {
            _ = section switch
            {
                // Stream sections
                ElfStreamSection streamSection => context.AbstractStreamSection(streamSection),
                ElfSymbolTable symbolTable => context.AbstractSymbolTable(symbolTable),
                ElfRelocationTable relocationTable => context.AbstractRelocationTable(relocationTable),
                _ => false,
            };
        }

        return context.Module;
    }
}
