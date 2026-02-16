// Adam Dernis 2024

using Zarem.Assembler.Parsers.Expressions.Abstract;
using Zarem.Assembler.Parsers.Expressions.Enums;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Assembler.Parsers.Expressions;

/// <summary>
/// A class for an integer node on an expression tree.
/// </summary>
public class AbsoluteNode : ValueNode<long>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbsoluteNode"/> class.
    /// </summary>
    public AbsoluteNode(Token token, long value) : base(token, value)
    {
    }

    /// <inheritdoc/>
    public override ExpressionType Type => ExpressionType.Integer;

    /// <inheritdoc/>
    public override bool TryEvaluate(Evaluator evaluator, out ExpressionResult result)
    {
        result = new ExpressionResult(Value);
        return true;
    }
}
