// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Linker.Config;
using Zarem.Linker.Enums;
using Zarem.Linker.Extensions;
using Zarem.Linker.Handlers;
using Zarem.Models;
using Zarem.Models.Tables;
using Zarem.Models.Tables.Enums;

namespace Zarem.Linker;

/// <summary>
/// This is za linker.
/// </summary>
public sealed class ZaLinker
{
    private readonly LinkerConfig _config;
    private readonly ILogger _logger;
    private readonly ILinkerHandler _handler;

    /// <summary>
    /// Initializes a new instance of <see cref="ZaLinker"/> class.
    /// </summary>
    private ZaLinker(LinkerConfig config, ILinkerHandler handler, ILogger logger)
    {
        _config = config;
        _handler = handler;
        _logger = logger;

        Module = new Module("MIPS"); // TODO: Determine architecture.
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
        logger ??= new Logger();
        var linker = new ZaLinker(config, handler, logger);
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
            foreach (var section in module.Sections.Values)
            {
                var linkedSection = Module.GetOrCreateSection(section.Name);
                linkedSection.Append(section.Stream);
            }
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
                // Drop local symbols
                if (symbol.Binding is SymbolBinding.Local)
                    continue;

                if (Module.Symbols.TryGetValue(symbol.Name, out var existing))
                {
                    // TODO: Weak symbols

                    // TODO: Track and log source defining modules
                    _logger?.Log(Severity.Error, LogId.DuplicateSymbolDefinition, module.Name ?? "", "ConflictingSymbolDefinitions", symbol.Name);
                    continue;
                }

                // Note sure how we got here
                Guard.IsNotNull(symbol.Address.Section);

                // Translate the symbol address within the section
                var linkedSection = Module.GetOrCreateSection(symbol.Address.Section.Name);
                ulong sectionDelta = linkedSection.VirtualAddress - symbol.Address.Section.VirtualAddress;
                long finalAddress = (long)sectionDelta + symbol.Address.Offset;

                var newSymbol = Module.GetOrCreateSymbol(symbol.Name);
                newSymbol.Address = new Address(linkedSection, finalAddress);
                newSymbol.Binding = symbol.Binding;
                newSymbol.Type = symbol.Type;
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

                    ulong symbolAddress = (ulong)symbol.Address.Offset;
                    ulong place = section.VirtualAddress + (ulong)relocation.Location.Offset;

                    _handler.PatchRelocation(linkedSection, relocation, symbolAddress, place);
                }
            } 
        }
    }
}
