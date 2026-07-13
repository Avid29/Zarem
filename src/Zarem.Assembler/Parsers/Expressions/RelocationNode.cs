// Avishai Dernis 2026

using Zarem.Assembler.Parsers.Expressions.Enums;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Assembler.Parsers.Expressions;

/// <summary>
/// A class for a relocation operator in an expression tree, which is a specific type of unary operator.
/// </summary>
public class RelocationNode : UnaryOperNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelocationNode"/> class.
    /// </summary>
    public RelocationNode(Token token) : base(token, Operation.Function)
    {
    }

    /// <summary>
    /// Gets the type of relocation operation represented by this node.
    /// </summary>
    public string RelocationType => ExpressionToken.Source;

    /// <inheritdoc/>
    public override bool TryEvaluate<T>(Evaluator<T> evaluator, out ExpressionResult<T> result)
    {
        result = default;

        // Evaluate child first
        if (!(Child?.TryEvaluate(evaluator, out var child) ?? false))
            return false;

        // Attempt to perform relocation operation using the evaluator
        return evaluator.TryRelocation(this, child, out result);
    }
}
