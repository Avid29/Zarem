// Adam Dernis 2024

using Zarem.Assembler.Parsers.Expressions.Abstract;
using Zarem.Assembler.Parsers.Expressions.Enums;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Models;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Parsers.Expressions;

/// <summary>
/// A class for an integer node on an expression tree.
/// </summary>
public class AddressNode : ValueNode<Address>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddressNode"/> class.
    /// </summary>
    public AddressNode(Token token, long value, Section? section = null) : this(token, new Address(section, value))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddressNode"/> class.
    /// </summary>
    public AddressNode(Token token, Address value) : base(token, value)
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
