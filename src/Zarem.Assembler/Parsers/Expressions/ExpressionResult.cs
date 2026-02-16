// Avishai Dernis 2025

using System.Diagnostics.CodeAnalysis;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Parsers.Expressions;

/// <summary>
/// A struct for expression the results of parsing an expression.
/// </summary>
public readonly struct ExpressionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionResult"/> struct.
    /// </summary>
    public ExpressionResult(long value, Symbol? reference = null)
    {
        Addend = value;
        Symbol = reference;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionResult"/> struct.
    /// </summary>
    public ExpressionResult(Symbol reference)
    {
        Symbol = reference;
        Addend = default;
    }

    /// <summary>
    /// Gets the value of the parsed expression.
    /// </summary>
    public long Addend { get; }

    /// <summary>
    /// Gets the symbol referenced in the expression.
    /// </summary>
    public Symbol? Symbol { get; }

    /// <summary>
    /// Gets whether or not the expression is absolute.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Symbol))]
    public bool IsAbsolute => Symbol is null;

    /// <summary>
    /// Gets whether or not the expression is symbolic.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Symbol))]
    public bool IsSymbolic => Symbol is not null;
}
