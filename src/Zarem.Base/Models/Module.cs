// Adam Dernis 2024

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Zarem.Models.Tables;

namespace Zarem.Models;

/// <summary>
/// A modifiable object module.
/// </summary>
public sealed class Module
{
    private readonly Dictionary<string, Section> _sections = [];
    private readonly Dictionary<string, Symbol> _symbols = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Module"/> class.
    /// </summary>
    public Module(string architecture)
    {
        Architecture = architecture;
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the module architecture.
    /// </summary>
    public string Architecture { get; }

    /// <summary>
    /// Gets or sets the module format.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the symbol for the entry point.
    /// </summary>
    public Symbol? EntryPoint { get; set; }

    /// <summary>
    /// Gets the symbol dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, Symbol> Symbols => _symbols;

    /// <summary>
    /// Gets the module sections.
    /// </summary>
    public IReadOnlyDictionary<string, Section> Sections => _sections;
    
    /// <summary>
    /// Gets or creates a section in the module.
    /// </summary>
    /// <param name="name">The name of the section.</param>
    /// <param name="alignment">The section's byte alignment.</param>
    /// <param name="stream">The section's stream.</param>
    /// <returns>A section with the requested name.</returns>
    public Section GetOrCreateSection(string name, uint alignment = 1, Stream? stream = null)
    {
        if (!_sections.TryGetValue(name, out var section))
        {
            section = new Section(name, alignment,stream);
            _sections[name] = section;
        }

        return section;
    }

    /// <summary>
    /// Gets or creates a symbol in the module.
    /// </summary>
    /// <param name="name">The name of the symbol.</param>
    /// <returns>A symbol with the requested name.</returns>
    public Symbol GetOrCreateSymbol(string name)
    {
        if (!_symbols.TryGetValue(name, out var symbol))
        {
            symbol = new Symbol(name);
            _symbols[name] = symbol;
        }

        return symbol;
    }

    /// <summary>
    /// Attempts to get a symbol in the module.
    /// </summary>
    /// <param name="name">The name of the symbol to retrieve.</param>
    /// <param name="symbol">The symbol, if it exists. <see langword="null"/> otherwise</param>
    /// <returns><see langword="true"/> if the symbol exists, <see langword="false"/> otherwise.</returns>
    public bool TryGetSymbol(string name, [NotNullWhen(true)] out Symbol? symbol)
        => _symbols.TryGetValue(name, out symbol);
}
