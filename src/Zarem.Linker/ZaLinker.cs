// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Linker.Config;
using Zarem.Linker.Enums;
using Zarem.Linker.Handlers;
using Zarem.Linker.Logging;
using Zarem.Models;
using Zarem.Models.Tables;
using Zarem.Models.Tables.Enums;

namespace Zarem.Linker;

/// <summary>
/// This is za linker.
/// </summary>
public sealed class ZaLinker
{
    private readonly Dictionary<Module, Dictionary<string, ulong>> _moduleSectionOffsets = [];
    private readonly Dictionary<Symbol, string> _symbolMapping = [];        // Lookup the name reassigned to local symbols
    private readonly Dictionary<string, Module> _symbolOriginLookup = [];   // Lookup the origin module of a defined global symbol

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
        logger?.Flush();

        var linker = new ZaLinker(config, handler, logger ?? new Logger());
        linker.Link(modules);
        return linker.Module;
    }

    private Module Link(params Module[] modules)
    {
        LayoutSections(modules);
        BuildSymbolTable(modules);
        RelocateDebugInfo(modules);
        ResolveRelocations(modules);

        if (_config.LinkMode is LinkMode.Executable)
        {
            // The global entry symbol was not found
            if (!Module.TryGetSymbol(_config.EntryPoint, out var symbol))
            {
                // Find all local symbols named entry
                var entrySymbols = Module.Symbols.Values.Where(x => x.Name.EndsWith($":{_config.EntryPoint}"));

                var count = entrySymbols.Count();

                Module? sourceModule = null;
                if (count is 1)
                {
                    sourceModule = _symbolOriginLookup[entrySymbols.First().Name];
                }

                _ = count switch
                {
                    // No symbol with the entry point name was found
                    0 => _logger.Log(Severity.Error, LogId.MissingEntryPoint, null, "EntryPointNotFound", _config.EntryPoint),

                    // One local symbol with the entry point name was found, assume that's the intended entry point
                    1 => _logger.Log(Severity.Error, LogId.MissingEntryPoint, sourceModule?.Identity, "EntryPointNotGlobal", _config.EntryPoint),

                    // Many local symbols with the entry point name were found. Just say the entry needs to be global
                    _ => _logger.Log(Severity.Error, LogId.MissingEntryPoint, null, "EntryPointNotGlobal", _config.EntryPoint)
                };
            }

            Module.EntryPoint = symbol;
        }

        return Module;
    }

    private void LayoutSections(Module[] modules)
    {
        foreach (var module in modules)
        {
            if (module.Architecture != _handler.GetArchitectureName())
            {
                // TODO: Decide. Is this a warning or an error?
                // Currently this is a warning and the linker will attempt to link without the target file.
                _logger.Log(Severity.Warning, LogId.WrongArchitecture, module.Identity, "ModuleHasWrongArchitecture", module.Identity, module.Architecture);
                continue;
            }

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
            // Skip wrong-architecture modules (log already handled)
            if (module.Architecture != _handler.GetArchitectureName())
                continue;

            foreach (var symbol in module.Symbols.Values)
            {
                // TODO: Handle weak symbols

                // Translate the symbol name into a linked symbol id
                var symbolId = symbol.Binding switch
                {
                    SymbolBinding.Local => $"local:{module.Identity}:{symbol.Name}",
                    SymbolBinding.Global or _ => symbol.Name,
                };

                // Track the mapping from the original symbol object
                _symbolMapping[symbol] = symbolId;

                // Log an error if a conflicting symbol is defined
                if (symbol.IsDefined && _symbolOriginLookup.TryGetValue(symbolId, out var originModule))
                {
                    _logger.Log(Severity.Error, LogId.DuplicateSymbolDefinition, module.Identity, "ConflictingSymbolDefinitions", symbol.Name, module.Identity, originModule.Identity);
                    continue;
                }

                var newSymbol = Module.GetOrCreateSymbol(symbolId);
                newSymbol.Binding = symbol.Binding;
                if (newSymbol.Type is SymbolType.Unknown)
                    newSymbol.Type = symbol.Type;

                if (symbol.Address.IsRelocatable)
                {
                    // Translate the symbol address within the section
                    var sectionName = symbol.Address.Section.Name;
                    var linkedSection = Module.GetOrCreateSection(sectionName);
                    long finalAddress = (long)_moduleSectionOffsets[module][sectionName] + symbol.Address.Offset;
                    newSymbol.Address = new Address(linkedSection, finalAddress);

                    // Track the origin to where the symbol is defined
                    _symbolOriginLookup[symbolId] = module;
                }

                // NOTE: Currently there is no check to ensure local symbols are defined. It is currently impossible
                // for a local symbol to not be defined, but this may change in the future
            }
        }

        if (_config.LinkMode is LinkMode.Executable)
        {
            foreach (var symbol in Module.Symbols.Values)
            {
                if (!symbol.IsDefined)
                {
                    _logger.Log(Severity.Error, LogId.UndefinedSymbol, Module.Identity, "SymbolNeverDefined", symbol.Name);
                }
            }
        }
    }

    private void RelocateDebugInfo(Module[] modules)
    {
        foreach (var module in modules)
        {
            // Skip incompatible modules
            if (module.Architecture != _handler.GetArchitectureName())
                continue;

            // If the module has no debug info, skip it
            if (module.DebugLines is null || module.DebugLines.Count is 0)
                continue;

            foreach (var entry in module.DebugLines)
            {
                var sourceAddr = entry.Address;
                var sectionName = sourceAddr.Section?.Name;

                // Find where this section was moved to in the linked module
                if (sectionName is not null &&
                    _moduleSectionOffsets.TryGetValue(module, out var offsets) &&
                    offsets.TryGetValue(sectionName, out ulong sectionOffsetInLinked))
                {
                    var linkedSection = Module.GetOrCreateSection(sectionName);

                    // Calculate the new offset within the combined section
                    uint newOffset = (uint)((long)sectionOffsetInLinked + sourceAddr.Offset);
                    var linkedAddr = new Address(linkedSection, newOffset);

                    // Add the relocated entry to the final module
                    Module.AddLineEntry(linkedAddr, entry.Location);
                }
            }
        }
    }

    private void ResolveRelocations(Module[] modules)
    {
        foreach (var module in modules)
        {
            // Skip wrong-architecture modules (log already handled)
            if (module.Architecture != _handler.GetArchitectureName())
                continue;

            foreach (var section in module.Sections.Values)
            {
                // Get delta between section and linked section
                var linkedSection = Module.GetOrCreateSection(section.Name);
                ulong sectionBaseInLinked = _moduleSectionOffsets[module][section.Name];

                foreach (var relocation in section.Relocations)
                {
                    if(!module.TryGetSymbol(relocation.SymbolName, out var sourceSymbol))
                        throw new Exception();

                    // Get the symbol's name, or mapped symbol name if local
                    var symbolName = _symbolMapping[sourceSymbol];

                    if (!Module.Symbols.TryGetValue(symbolName, out var symbol))
                    {
                        _logger.Log(Severity.Error, LogId.UndeclaredSymbolReferenced, module.Identity, "RelocationSymbolDoesNotExist", relocation.SymbolName);
                        continue;
                    }

                    linkedSection.AddRelocation(new RelocationEntry(symbolName, relocation.Location, relocation.Type, relocation.Addend));

                    if (!symbol.Address.IsRelocatable)
                        continue;

                    // The symbol is defined, so it should have a section
                    Guard.IsNotNull(symbol.Address.Section);

                    // The absolute virtual address of the symbol
                    ulong symbolVirtual = Module.GetOrCreateSection(symbol.Address.Section.Name).VirtualAddress + (ulong)symbol.Address.Offset;

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
