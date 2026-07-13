// Avishai Dernis 2024

using CommunityToolkit.Diagnostics;
using Zarem.Assembler.Parsers.Expressions.Abstract;
using Zarem.Assembler.Parsers.Expressions.Enums;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Assembler.Parsers.Expressions;

/// <summary>
/// A class for a binary operator in an expression tree.
/// </summary>
public class BinaryOperNode : OperNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryOperNode"/> class.
    /// </summary>
    public BinaryOperNode(Token token, Operation operation) : base(token, operation)
    {
    }

    /// <summary>
    /// Gets or sets the left hand child of the <see cref="BinaryOperNode"/>.
    /// </summary>
    public required ExpNode LeftChild { get; init; }

    /// <summary>
    /// Gets or sets the right hand child of the <see cref="BinaryOperNode"/>.
    /// </summary>
    public required ExpNode RightChild { get; init; }

    /// <inheritdoc/>
    /// <remarks>
    /// Determined by its children. Invalid if they don't match.
    /// </remarks>
    public override ExpressionType Type
    {
        get
        {
            // Check if types match and children aren't null
            if (LeftChild.Type == RightChild.Type)
            {
                // Return matched type, or invalid if match was null.
                return LeftChild.Type;
            }

            // Types don't match, it's an invalid expression
            return ExpressionType.Invalid;
        }
    }

    /// <inheritdoc/>
    public override bool TryEvaluate<T>(Evaluator<T> evaluator, out ExpressionResult<T> result)
    {
        result = default;

        // Evaluate children, and return false if either fails.
        // They log their own errors, so no need to log another.
        if ((!(LeftChild?.TryEvaluate(evaluator, out var left) ?? false)) ||
            (!(RightChild?.TryEvaluate(evaluator, out var right) ?? false)))
            return false;

        return Operation switch
        {
            // Arithmetic
            Operation.Addition => evaluator.TryAdd(this, left, right, out result),
            Operation.Subtraction => evaluator.TrySubtract(this, left, right, out result),
            Operation.Multiplication => evaluator.TryMultiply(this, left, right, out result),
            Operation.Division => evaluator.TryDivide(this, left, right, out result),
            Operation.Modulus => evaluator.TryMod(this, left, right, out result),

            // Logical
            Operation.And => evaluator.TryAnd(this, left, right, out result),
            Operation.Or => evaluator.TryOr(this, left, right, out result),
            Operation.Xor => evaluator.TryXor(this, left, right, out result),

            // Shift
            Operation.LeftShift => evaluator.TrySll(this, left, right, out result),
            Operation.RightShift => evaluator.TrySrl(this, left, right, out result),

            _ => ThrowHelper.ThrowArgumentException<bool>(),
        };
    }
}
