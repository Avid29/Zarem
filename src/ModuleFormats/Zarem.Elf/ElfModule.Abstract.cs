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
                ElfArch.RISCV => "RISC-V",
                _ => null
            };

            Guard.IsNotNull(archName);
            Module = new(archName);
        }

        public Module Module { get; }

        public readonly bool AbstractStreamSection(ElfStreamSection streamSection)
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
                if (string.IsNullOrEmpty(name))
                    continue;

                var sectionName = elfSymbol.SectionLink.Section?.Name.Value;
                Section? section = sectionName is not null ? Module.GetOrCreateSection(sectionName) : null;

                var value = new Address(section, (long)elfSymbol.Value);
                var binding = elfSymbol.Bind.FromElf();
                var symbol = Module.GetOrCreateSymbol(name);
                symbol.Address = value;
                symbol.Binding = binding;
                symbol.Type = elfSymbol.Type switch
                {
                    ElfSymbolType.Common => SymbolType.Label,
                    ElfSymbolType.SpecificOS0 => SymbolType.Constant,
                    ElfSymbolType.NoType or _ => SymbolType.Unknown,
                };
            }

            return true;
        }

        public readonly bool AbstractRelocationTable(ElfRelocationTable relocationTable)
        {
            if (_symTab is null)
                return false;

            var elfSection = relocationTable.Info.Section;
            if (elfSection is null)
            {
                elfSection = relocationTable.Parent?.Sections[relocationTable.Info.SpecialIndex];
            }

            var sectionName = elfSection?.Name.Value;
            if (sectionName is null)
                return false;

            var section = Module.GetOrCreateSection(sectionName);

            foreach (var relEntry in relocationTable.Entries)
            {
                var elfSymbol = _symTab.Entries[(int)relEntry.SymbolIndex];
                string symbolName = elfSymbol.Name.Value;

                if (string.IsNullOrEmpty(symbolName) && elfSymbol.SectionLink.Section != null)
                    symbolName = elfSymbol.SectionLink.Section.Name.Value;

                var address = new Address(section, (long)relEntry.Offset);
                section.AddRelocation(new RelocationEntry(symbolName, address, relEntry.Type.Value, relEntry.Addend));
            }

            return true;
        }
    }

    /// <inheritdoc/>
    public Module? Abstract(ElfConfig config)
    {
        var context = new ElfAbstractContext(_elfFile);

        // First pass: Symbol table and stream sections
        foreach (var section in _elfFile.Sections)
        {
            _ = section switch
            {
                ElfStreamSection streamSection => context.AbstractStreamSection(streamSection),
                ElfSymbolTable symbolTable => context.AbstractSymbolTable(symbolTable),
                _ => false,
            };
        }

        // Second pass: Relocation table
        foreach (var section in _elfFile.Sections)
        {
            _ = section switch
            {
                ElfRelocationTable relocationTable => context.AbstractRelocationTable(relocationTable),
                _ => false,
            };
        }

        return context.Module;
    }
}
