// Adam Dernis 2024


// Adam Dernis 2024

using Zarem.Assembler.Models.Directives.Abstract;

namespace Zarem.Assembler.Models.Directives;

/// <summary>
/// A <see cref="Directive"/> for global references.
/// </summary>
public class GlobalDirective(string symbol) : Directive
{
    /// <summary>
    /// Gets the symbol referenced.
    /// </summary>
    public string Symbol { get; } = symbol;
}
