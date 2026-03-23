// Avishai Dernis 2026

using Zarem.Assembler.Models.Directives.Abstract;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Assembler.Models.Directives;

/// <summary>
/// A <see cref="Directive"/> for defining constant symbols.
/// </summary>
public class DefineDirective : Directive
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefineDirective"/> class.
    /// </summary>
    public DefineDirective(Token name, long value)
    {
        Name = name;
        Value = value;
    }

    /// <summary>
    /// Gets the name of the symbol to define.
    /// </summary>
    public Token Name { get; }

    /// <summary>
    /// Gets the value of the symbol to define.
    /// </summary>
    public long Value { get; }
}
