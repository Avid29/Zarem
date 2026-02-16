// Adam Dernis 2024

using Zarem.Models.Tables.Enums;

namespace Zarem.Models.Tables;

/// <summary>
/// An entry in the load module's symbol table.
/// </summary>
public class Symbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Symbol"/> class.
    /// </summary>
    public Symbol(string name, SymbolType type = SymbolType.Label, SymbolBinding binding = SymbolBinding.Local)
    {
        Name = name;
        Type = type;
        Binding = binding;
    }

    /// <summary>
    /// Gets the symbol name of the entry.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the symbol binding type.
    /// </summary>
    public SymbolBinding Binding { get; set; }

    /// <summary>
    /// Gets or sets the symbol's type.
    /// </summary>
    public SymbolType Type { get; set; }

    /// <summary>
    /// Gets the address of the symbol.
    /// </summary>
    public Address Address { get; set; }

    /// <summary>
    /// Gets whether or not the symbol is defined.
    /// </summary>
    public bool IsDefined => Address.Section is not null;
}
