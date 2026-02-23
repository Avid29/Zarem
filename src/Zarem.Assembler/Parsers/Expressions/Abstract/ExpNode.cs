// Avishai Dernis 2024

using Zarem.Assembler.Parsers.Expressions.Enums;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Assembler.Parsers.Expressions.Abstract;

/// <summary>
/// A class for nodes in an expression tree.
/// </summary>
public abstract class ExpNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpNode"/> class.
    /// </summary>
    protected ExpNode(Token token)
    {
        ExpressionToken = token;
    }

    /// <summary>
    /// Gets the node's parent in the expression tree.
    /// </summary>
    public OperNode? Parent { get; set; }

    /// <summary>
    /// Gets the type of the expression node.
    /// </summary>
    public abstract ExpressionType Type { get; }

    /// <summary>
    /// Gets the token that make up this expression node.
    /// </summary>
    public Token ExpressionToken { get; init; }

    /// <summary>
    /// Evaluate the expression tree as a type.
    /// </summary>
    /// <param name="evaluator">The <see cref="Evaluator"/> to use in evaluating the tree.</param>
    /// <param name="result">The evaluated expression tree.</param>
    /// <returns><see langword="true"/> when tree was successfully evaluated. <see langword="false"/> otherwise.</returns>
    public abstract bool TryEvaluate(Evaluator evaluator, out ExpressionResult result);
}
