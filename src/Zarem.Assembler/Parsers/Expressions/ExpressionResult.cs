// Avishai Dernis 2025

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Parsers.Expressions;

/// <summary>
/// A struct for expression the results of parsing an expression.
/// </summary>
public readonly struct ExpressionResult<T>
    where T : unmanaged, IBinaryNumber<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionResult{T}"/> struct.
    /// </summary>
    public ExpressionResult(T value, SymbolNode? reference = null)
    {
        Addend = value;
        SymbolNode = reference;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionResult{T}"/> struct.
    /// </summary>
    public ExpressionResult(SymbolNode reference)
    {
        SymbolNode = reference;
        Addend = default;
    }

    /// <summary>
    /// Gets the value of the parsed expression.
    /// </summary>
    public T Addend { get; }

    /// <summary>
    /// Gets the symbol node referenced in the expression.
    /// </summary>
    public SymbolNode? SymbolNode { get; }

    /// <summary>
    /// Gets the symbol referenced in the expression.
    /// </summary>
    public Symbol? Symbol => SymbolNode?.Value;

    /// <summary>
    /// Gets whether or not the expression is absolute.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Symbol))]
    [MemberNotNullWhen(false, nameof(SymbolNode))]
    public bool IsAbsolute => Symbol is null;

    /// <summary>
    /// Gets whether or not the expression is symbolic.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Symbol))]
    [MemberNotNullWhen(true, nameof(SymbolNode))]
    public bool IsSymbolic => Symbol is not null;
}
