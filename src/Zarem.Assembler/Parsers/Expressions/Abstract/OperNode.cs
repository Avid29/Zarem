// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using Zarem.Assembler.Parsers.Expressions.Enums;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Assembler.Parsers.Expressions.Abstract;

/// <summary>
/// A class an operator in an expression tree.
/// </summary>
public abstract class OperNode : ExpNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OperNode"/> class.
    /// </summary>
    public OperNode(Token token, Operation operation) : base(token)
    {
        Operation = operation;
    }

    /// <summary>
    /// Gets or sets the operation of the <see cref="BinaryOperNode"/>.
    /// </summary>
    public Operation Operation { get; }

    /// <summary>
    /// Gets the priority of the operation by order of operations.
    /// </summary>
    /// <remarks>
    /// Lower is lower on the tree and therefore executed earlier.
    /// </remarks>
    public int Priority => Operation switch
    {
        Operation.Addition => 4,
        Operation.Subtraction => 4,
        Operation.Multiplication => 3,
        Operation.Division => 3,
        Operation.Modulus => 3,
        Operation.And => 2,
        Operation.Or => 1,
        Operation.Xor => 1,

        Operation.Negation => 1,
        Operation.UnaryPlus => 1,
        Operation.Not => 1,
        Operation.Function => 0,
        _ => ThrowHelper.ThrowArgumentException<int>("Cannot assess priority of invalid operation.")
    };
}
