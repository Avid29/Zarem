// Avishai Dernis 2025

using Zarem.Assembler.Parsers.Expressions.Abstract;
using Zarem.Assembler.Parsers.Expressions.Enums;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Parsers.Expressions;

/// <summary>
/// A node for a symbol reference in an expression tree.
/// </summary>
public class SymbolNode : ValueNode<Symbol>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SymbolNode"/> class.
    /// </summary>
    public SymbolNode(Token token, Symbol symbolEntry) : base(token, symbolEntry)
    {
    }

    /// <inheritdoc/>
    public override ExpressionType Type => ExpressionType.Integer;

    /// <inheritdoc/>
    public override bool TryEvaluate(Evaluator evaluator, out ExpressionResult result)
    {
        result = new ExpressionResult(this);
        return true;
    }
}
