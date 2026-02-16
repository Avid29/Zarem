// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using LibObjectFile.Elf;
using ObjFormats.LibOF.Extensions;
using System.Collections.Generic;
using System.Linq;
using Zarem.Elf.Config;
using Zarem.Extensions.System.IO;
using Zarem.Models;

namespace Zarem.Elf;

/// <summary>
/// An object module in ELF format.
/// </summary>
public partial class ElfModule
{
    private ref struct ElfBuildContext
    {
        private ElfSymbolTable? _symtab;

        public ElfBuildContext(Module module)
        {
            Module = module;
            ElfFile = new ElfFile(ElfArch.MIPS)
            {
                new ElfSectionHeaderStringTable(),
                new ElfSectionHeaderTable()
            };
        }

        public Module Module { get; }

        public ElfFile ElfFile { get; }

        public void CreateSections()
        {
            ulong pos = 0;

            foreach (var section in Module.Sections.Values)
            {
                var type = section.Name switch
                {
                    ".text" => ElfSectionSpecialType.Text,
                    ".data" => ElfSectionSpecialType.Data,
                    ".rodata" => ElfSectionSpecialType.ReadOnlyData,
                    ".sdata" => ElfSectionSpecialType.Data,
                    ".bss" => ElfSectionSpecialType.Bss,
                    ".sbss" => ElfSectionSpecialType.Bss,
                    _ => ElfSectionSpecialType.None,
                };

                var elfSec = new ElfStreamSection(type, section.Stream);
                ElfFile.Add(elfSec);

                section.Stream.Position = 0;
                elfSec.Stream.CopyFrom(section.Stream, (int)section.Stream.Length);

                elfSec.VirtualAddress = section.VirtualAddress;
                pos += (ulong)section.Stream.Length;
                pos += 4096 - (pos % 4096);
            }
        }

        public void CreateSymTable()
        {
            _symtab = new ElfSymbolTable()
            {
                Link = new(new ElfStringTable()),
            };

            foreach (var symbol in Module.Symbols.Values)
            {
                var sectionName = symbol.Address.Section?.Name;

                ElfSectionLink link = default;
                if (sectionName is not null)
                {
                    var section = ElfFile.Sections.FirstOrDefault(x => x.Name == sectionName);
                    link = new ElfSectionLink(section);
                }

                var elfSymbol = new ElfSymbol()
                {
                    Name = symbol.Name,
                    Value = (ulong)symbol.Address.Offset,
                    Bind = symbol.Binding.ToElf(),
                    SectionLink = link,
                };

                _symtab.Entries.Add(elfSymbol);
            }

            ElfFile.Add(_symtab);
        }

        public void CreateRelTables()
        {
            Guard.IsNotNull(_symtab);

            Dictionary<(string, bool), ElfRelocationTable> relTables = [];

            foreach (var section in Module.Sections.Values)
            {
                foreach (var relocation in section.Relocations)
                {
                    var location = relocation.Location;
                    Guard.IsNotNull(location.Section);

                    var isRela = relocation.Addend is not 0;
                    if (!relTables.TryGetValue((location.Section.Name, isRela), out var table))
                    {
                        table = new ElfRelocationTable(isRela);
                        relTables[(location.Section.Name, isRela)] = table;
                    }

                    var offset = relocation.Location.Offset;
                    var symbolIndex = _symtab.Entries.FindIndex(x => x.Name.Value == relocation.SymbolName);
                    Guard.IsNotEqualTo(symbolIndex, -1);

                    var type = new ElfRelocationType(ElfArchEx.MIPS, relocation.Type);
                    var relItem = new ElfRelocation((ulong)location.Offset, type, (uint)symbolIndex, relocation.Addend);

                    table.Entries.Add(relItem);
                }
            }

            foreach (var ((section, isRela), table) in relTables)
            {
                table.Name = isRela ? $".rela{section}" : $".rel{section}";
                table.Info = new ElfSectionLink(ElfFile.Sections.First(x => x.Name == section));
                ElfFile.Add(table);
            }
        }

        public void SetEntry()
        {
            // Ensure there is an entry point
            var entry = Module.EntryPoint;
            if (entry is null || !entry.IsDefined)
                return;

            // Fetch the section the entry point belongs to
            var entrySection = ElfFile.Sections.FirstOrDefault(x => x.Name == (entry.Address.Section?.Name ?? ""));
            if (entrySection is null)
                return;

            // Calculate and set the entry point address as an offset of the section's virual address
            var entryPoint = entrySection.VirtualAddress + (uint)entry.Address.Offset;
            ElfFile.EntryPointAddress = entryPoint;
        }
    }

    /// <inheritdoc/>
    public static ElfModule? Create(Module module, ElfConfig config)
    {
        var context = new ElfBuildContext(module);

        context.CreateSections();
        context.CreateSymTable();
        context.CreateRelTables();
        context.SetEntry();

        return new ElfModule(context.Module.Name, context.ElfFile);
    }
}
