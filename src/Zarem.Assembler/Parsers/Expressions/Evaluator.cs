// Adam Dernis 2024

using CommunityToolkit.Diagnostics;
using System.Collections.Generic;
using Zarem.Assembler.Extensions;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models;
using Zarem.Assembler.Parsers.Expressions.Abstract;
using Zarem.Models;
using Zarem.Models.Tables;
using Zarem.Models.Tables.Enums;

namespace Zarem.Assembler.Parsers.Expressions;

/// <summary>
/// A struct for applying operations.
/// </summary>
public readonly struct Evaluator
{
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Evaluator"/> struct.
    /// </summary>
    public Evaluator(IReadOnlyDictionary<string, Symbol>? symbols, ILogger? logger)
    {
        _logger = logger;
        Symbols = symbols;
    }

    /// <summary>
    /// Gets the assembler content to use by the evaluator.
    /// </summary>
    public IReadOnlyDictionary<string, Symbol>? Symbols { get; }

    /// <summary>
    /// Add <paramref name="left"/> and <paramref name="right"/>.
    /// </summary>
    /// <param name="node">The expression node being evaluated.</param>
    /// <param name="left">The left-hand child.</param>
    /// <param name="right">The right-hand child.</param>
    /// <param name="result">The sum of <paramref name="left"/> and <paramref name="right"/>.</param>
    /// <returns>Whether or not the sum of the items could be taken.</returns>
    public readonly bool TryAdd(BinaryOperNode node, ExpressionResult left, ExpressionResult right, out ExpressionResult result)
    {
        result = default;

        // If both address are relocatable
        if (left.IsSymbolic && right.IsSymbolic)
        {
            _logger?.Log(Severity.Error, LogId.InvalidExpressionOperation, node.ExpressionToken, "CantAddRelocatables");
            return false;
        }

        var symbol = left.Symbol ?? right.Symbol;
        result = new(left.Addend + right.Addend, symbol);
        return true;
    }

    /// <summary>
    /// Subtract <paramref name="right"/> from <paramref name="left"/>.
    /// </summary>
    /// <param name="node">The expression node being evaluated.</param>
    /// <param name="left">The left-hand child.</param>
    /// <param name="right">The right-hand child.</param>
    /// <param name="result">The difference between <paramref name="left"/> and <paramref name="right"/></param>
    /// <returns>Whether or not the difference of the items could be taken.</returns>
    public readonly bool TrySubtract(BinaryOperNode node, ExpressionResult left, ExpressionResult right, out ExpressionResult result)
    {
        result = default;

        if (right.IsSymbolic)
        {
            // Absolute - Symbolic
            if (left.IsAbsolute)
            {
                _logger?.Log(Severity.Error, LogId.InvalidExpressionOperation, node.ExpressionToken, "CantSubtractRelocatable");
                return false;
            }

            // Symbolic in different sections, or undefined
            if (left.Symbol.Address.Section != right.Symbol.Address.Section &&
                left.Symbol.IsDefined && right.Symbol.IsDefined)
            {
                // TODO: Improve error message
                _logger?.Log(Severity.Error, LogId.InvalidExpressionOperation, node.ExpressionToken, "CantSubtractRelocatable");
                return false;
            }

            // Symbolic - Symbolic in the same section
            // The result is an absolute
            result = new ExpressionResult(left.Addend - right.Addend);
            return true;
        }

        // This works for both
        // Symbolic - Constant
        // Constant - Constant
        result = new ExpressionResult(left.Addend - right.Addend, left.Symbol);
        return true;
    }

    /// <summary>
    /// Multiply <paramref name="left"/> and <paramref name="right"/>.
    /// </summary>
    /// <param name="node">The expression node being evaluated.</param>
    /// <param name="left">The left-hand child.</param>
    /// <param name="right">The right-hand child.</param>
    /// <param name="result">The product of <paramref name="left"/> and <paramref name="right"/>.</param>
    /// <returns>Whether or not the product of the items could be taken.</returns>
    public readonly bool TryMultiply(BinaryOperNode node, ExpressionResult left, ExpressionResult right, out ExpressionResult result)
    {
        result = default;

        // Cannot multiply relocatable addressing 
        if (CheckRelocatable(node, left, right, "Multiply"))
            return false;

        result = new(left.Addend * right.Addend);
        return true;
    }

    /// <summary>
    /// Divide <paramref name="left"/> by <paramref name="right"/>.
    /// </summary>
    /// <param name="node">The expression node being evaluated.</param>
    /// <param name="left">The left-hand child.</param>
    /// <param name="right">The right-hand child.</param>
    /// <param name="result">The quotient of <paramref name="left"/> divided by <paramref name="right"/>.</param>
    /// <returns>Whether or not the quotient of the items could be taken.</returns>
    public readonly bool TryDivide(BinaryOperNode node, ExpressionResult left, ExpressionResult right, out ExpressionResult result)
    {
        result = default;

        // Cannot divide relocatable addressing
        if (CheckRelocatable(node, left, right, "Divide"))
            return false;

        result = new(left.Addend / right.Addend);
        return true;
    }

    /// <summary>
    /// Modulus of <paramref name="left"/> divided by <paramref name="right"/>.
    /// </summary>
    /// <param name="node">The expression node being evaluated.</param>
    /// <param name="left">The left-hand child.</param>
    /// <param name="right">The right-hand child.</param>
    /// <param name="result">The remainder of <paramref name="left"/> divided by <paramref name="right"/>.</param>
    /// <returns>Whether or not the remainder of dividing the items could be taken.</returns>
    public readonly bool TryMod(BinaryOperNode node, ExpressionResult left, ExpressionResult right, out ExpressionResult result)
    {
        result = default;

        // Cannot mod relocatable addressing
        if (CheckRelocatable(node, left, right, "Modulus"))
            return false;

        result = new(left.Addend % right.Addend);
        return true;
    }

    /// <summary>
    /// Apply a unary plus to <paramref name="value"/>.
    /// </summary>
    /// <param name="node">The expression node being evaluated.</param>
    /// <param name="value">The child.</param>
    /// <param name="result">The result of a unary plus on <paramref name="value"/>.</param>
    /// <returns>Whether or not a unary plus of the child could be taken </returns>
    public readonly bool TryUnaryPlus(UnaryOperNode node, ExpressionResult value, out ExpressionResult result)
    {
        result = value;
        return true;
    }

    /// <summary>
    /// Negate <paramref name="value"/>.
    /// </summary>
    /// <param name="node">The expression node being evaluated.</param>
    /// <param name="value">The child.</param>
    /// <param name="result">Negation of <paramref name="value"/>.</param>
    /// <returns>Whether or not the negation of the child could be taken.</returns>
    public readonly bool TryNegate(UnaryOperNode node, ExpressionResult value, out ExpressionResult result)
    {
        result = default;

        // Cannot negate relocatable addressing
        if (CheckRelocatable(node, value, "Negate"))
            return false;

        result = new(-value.Addend);
        return true;
    }

    /// <summary>
    /// Logical AND of <paramref name="left"/> and <paramref name="right"/>.
    /// </summary>
    /// <param name="node">The expression node being evaluated.</param>
    /// <param name="left">The left-hand child.</param>
    /// <param name="right">The right-hand child.</param>
    /// <param name="result">Logical AND of <paramref name="left"/> and <paramref name="right"/>.</param>
    /// <returns>Whether or not the Logical AND of the items could be taken.</returns>
    public readonly bool TryAnd(BinaryOperNode node, ExpressionResult left, ExpressionResult right, out ExpressionResult result)
    {
        result = default;

        // Cannot AND relocatable addressing
        if (CheckRelocatable(node, left, right, "AND"))
            return false;

        result = new(left.Addend & right.Addend);
        return true;
    }

    /// <summary>
    /// Logical OR of <paramref name="left"/> and <paramref name="right"/>.
    /// </summary>
    /// <param name="node">The expression node being evaluated.</param>
    /// <param name="left">The left-hand child.</param>
    /// <param name="right">The right-hand child.</param>
    /// <param name="result">Logical OR of <paramref name="left"/> and <paramref name="right"/>.</param>
    /// <returns>Whether or not the Logical OR of the items could be taken.</returns>
    public readonly bool TryOr(BinaryOperNode node, ExpressionResult left, ExpressionResult right, out ExpressionResult result)
    {
        result = default;

        // Cannot OR relocatable addressing
        if (CheckRelocatable(node, left, right, "OR"))
            return false;

        result = new(left.Addend | right.Addend);
        return true;
    }

    /// <summary>
    /// Logical XOR of <paramref name="left"/> and <paramref name="right"/>.
    /// </summary>
    /// <param name="node">The expression node being evaluated.</param>
    /// <param name="left">The left-hand child.</param>
    /// <param name="right">The right-hand child.</param>
    /// <param name="result">Logical XOR of <paramref name="left"/> and <paramref name="right"/>.</param>
    /// <returns>Whether or not the Logical XOR of the items could be taken.</returns>
    public readonly bool TryXor(BinaryOperNode node, ExpressionResult left, ExpressionResult right, out ExpressionResult result)
    {
        result = default;

        // Cannot XOR relocatable addressing
        if (CheckRelocatable(node, left, right, "XOR"))
            return false;

        result = new(left.Addend ^ right.Addend);
        return true;
    }

    /// <summary>
    /// Logical NOT of <paramref name="value"/>.
    /// </summary>
    /// <param name="node">The expression node being evaluated.</param>
    /// <param name="value">The child.</param>
    /// <param name="result">Logical NOT of <paramref name="value"/>.</param>
    /// <returns>Whether or not the logical NOT of the child could be taken.</returns>
    public readonly bool TryNot(UnaryOperNode node, ExpressionResult value, out ExpressionResult result)
    {
        result = default;

        // Cannot NOT relocatable addressing
        if (CheckRelocatable(node, value, "NOT"))
            return false;

        result = new(~value.Addend);
        return true;
    }

    private readonly bool CheckRelocatable(BinaryOperNode node, ExpressionResult left, ExpressionResult right, string operation)
    {
        return CheckRelocatable(node.LeftChild, left, operation) || CheckRelocatable(node.RightChild, right, operation);
    }

    private readonly bool CheckRelocatable(ExpNode? node, ExpressionResult value, string operation)
    {
        Guard.IsNotNull(node);

        if (value.IsSymbolic)
        {
            _logger?.Log(Severity.Error, LogId.InvalidExpressionOperation, node.ExpressionToken, $"Cant{operation}Relocatable");
            return true;
        }

        return false;
    }
}
