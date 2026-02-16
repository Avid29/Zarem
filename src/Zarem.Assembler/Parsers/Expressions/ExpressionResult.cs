// Avishai Dernis 2025

using Zarem.Models;
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
    public ExpressionResult(Address value, Symbol? reference = null)
    {
        Value = value;
        Reference = reference;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionResult"/> struct.
    /// </summary>
    public ExpressionResult(Symbol reference)
    {
        Reference = reference;
    }

    /// <summary>
    /// Gets the value of the parsed expression.
    /// </summary>
    public Address Value { get; }

    /// <summary>
    /// Gets the reference information for any tracked reference made in the in expression.
    /// </summary>
    public Symbol? Reference { get; }

    /// <summary>
    /// Gets whether or not the expression is relocatable.
    /// </summary>
    public bool IsRelocatable => Reference is not null;
}
