// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Linker.Config;
using Zarem.Linker.Enums;
using Zarem.Linker.Handlers;
using Zarem.Linker.Logging;
using Zarem.Models;

namespace Zarem.Linker;

/// <summary>
/// This is za linker.
/// </summary>
public sealed class ZaLinker
{
    private readonly Dictionary<Module, Dictionary<string, ulong>> _moduleSectionOffsets = [];

    private readonly LinkerConfig _config;
    private readonly LinkerLogger _logger;
    private readonly ILinkerHandler _handler;

    /// <summary>
    /// Initializes a new instance of <see cref="ZaLinker"/> class.
    /// </summary>
    private ZaLinker(LinkerConfig config, ILinkerHandler handler, ILogger logger)
    {
        _config = config;
        _handler = handler;
        _logger = new LinkerLogger(logger);

        Module = new Module(_handler.GetArchitectureName());
    }

    /// <summary>
    /// Gets the 
    /// </summary>
    public Module Module { get; }

    /// <summary>
    /// Links a collection of modules together.
    /// </summary>
    /// <param name="config">The linker configuration.</param>
    /// <param name="handler">The architecture specific linking handler.</param>
    /// <param name="logger">The logger for error handling.</param>
    /// <param name="modules">The collection of object modules to link.</param>
    /// <returns>A linked module.</returns>
    public static Module Link(LinkerConfig config, ILinkerHandler handler, ILogger? logger = null, params Module[] modules)
    {
        var linker = new ZaLinker(config, handler, logger ?? new Logger());
        linker.Link(modules);
        return linker.Module;
    }

    private Module Link(params Module[] modules)
    {
        LayoutSections(modules);
        BuildSymbolTable(modules);
        ResolveRelocations(modules);

        return Module;
    }

    private void LayoutSections(Module[] modules)
    {
        foreach (var module in modules)
        {
            var offsets = new Dictionary<string, ulong>();
            foreach (var section in module.Sections.Values)
            {
                var linkedSection = Module.GetOrCreateSection(section.Name);

                // TODO: Get alignment info from config
                linkedSection.Align(4);

                offsets[section.Name] = (ulong)linkedSection.Size;
                linkedSection.Append(section.Stream);
            }

            _moduleSectionOffsets.Add(module, offsets);
        }

        // TODO: Support non-zero base address
        ulong address = 0;

        foreach(var section in Module.Sections.Values)
        {
            section.VirtualAddress = address;
            address += (ulong)section.Size;
        }
    }

    private void BuildSymbolTable(Module[] modules)
    {
        foreach(var module in modules)
        {
            foreach (var symbol in module.Symbols.Values)
            {
                //// TODO: Manage local symbols vs global symbols
                //if (symbol.Binding is SymbolBinding.Local)
                //    continue;

                if (Module.Symbols.TryGetValue(symbol.Name, out var existing))
                {
                    // TODO: Weak symbols
                    // TODO: Track and log source defining modules
                    _logger?.Log(Severity.Error, LogId.DuplicateSymbolDefinition, module.Name ?? "", "ConflictingSymbolDefinitions", symbol.Name, module.Name);
                    continue;
                }

                var newSymbol = Module.GetOrCreateSymbol(symbol.Name);
                newSymbol.Binding = symbol.Binding;
                newSymbol.Type = symbol.Type;

                if (symbol.IsDefined)
                {
                    Guard.IsNotNull(symbol.Address.Section);

                    // Translate the symbol address within the section
                    var sectionName = symbol.Address.Section.Name;
                    var linkedSection = Module.GetOrCreateSection(sectionName);
                    long finalAddress = (long)(linkedSection.VirtualAddress + _moduleSectionOffsets[module][sectionName]) + symbol.Address.Offset;
                    newSymbol.Address = new Address(linkedSection, finalAddress);
                }
                else if (_config.LinkMode is LinkMode.Executable)
                {
                    // TODO: Log declared symbol never defined
                }
            }
        }
    }

    private void ResolveRelocations(Module[] modules)
    {
        foreach (var module in modules)
        {
            foreach (var section in module.Sections.Values)
            {
                // Get delta between section and linked section
                var linkedSection = Module.GetOrCreateSection(section.Name);
                ulong sectionBaseInLinked = _moduleSectionOffsets[module][section.Name];

                foreach (var relocation in section.Relocations)
                {
                    if (!Module.Symbols.TryGetValue(relocation.SymbolName, out var symbol))
                    {
                        if (_config.LinkMode is LinkMode.Executable)
                        {
                            _logger.Log(Severity.Error, LogId.UndefinedSymbol, module.Name ?? "", "SymbolNeverDefined", relocation.SymbolName);
                        }

                        linkedSection.AddRelocation(relocation);
                        continue;
                    }

                    Guard.IsNotNull(symbol.Address.Section);

                    // The absolute virtual address of the symbol
                    ulong symbolVirtual = Module.GetOrCreateSection(symbol.Address.Section.Name).VirtualAddress + sectionBaseInLinked;

                    // The virtual address of the instruction being patched
                    ulong patchVirtual = linkedSection.VirtualAddress + sectionBaseInLinked + (ulong)relocation.Location.Offset;

                    // The offset of the instruction within the stream
                    ulong streamPatchOffset = section.VirtualAddress + (ulong)relocation.Location.Offset;

                    _handler.PatchRelocation(linkedSection, relocation, streamPatchOffset, symbolVirtual, patchVirtual, _logger.Parent);
                }
            } 
        }
    }
}
