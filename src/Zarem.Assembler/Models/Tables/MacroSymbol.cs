// Avishai Dernis 2026

using System.Collections.Generic;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Models.Tables;
using Zarem.Models.Tables.Enums;

namespace Zarem.Assembler.Models.Tables;

/// <summary>
/// A symbol with macro data.
/// </summary>
public class MacroSymbol : Symbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MacroSymbol"/> class.
    /// </summary>
    public MacroSymbol(string name, IReadOnlyList<Token> expression) : base(name, SymbolType.Macro)
    {
        Expression = expression;
    }

    /// <summary>
    /// Gets the macro expression.
    /// </summary>
    public IReadOnlyList<Token> Expression { get; }
}
