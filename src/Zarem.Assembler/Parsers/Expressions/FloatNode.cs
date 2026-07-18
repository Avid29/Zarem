// Avishai Dernis 2026

using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Parsers.Expressions.Abstract;
using Zarem.Assembler.Parsers.Expressions.Enums;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Assembler.Parsers.Expressions;

/// <summary>
/// A class for an float-point number node on an expression tree.
/// </summary>
public class FloatNode : ValueNode<double>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerNode"/> class.
    /// </summary>
    public FloatNode(Token token, double value) : base(token, value)
    {
    }

    /// <inheritdoc/>
    public override ExpressionType Type => ExpressionType.Integer;

    /// <inheritdoc/>
    public override bool TryEvaluate<T>(Evaluator<T> evaluator, out ExpressionResult<T> result)
    {
        result = default;

        if (typeof(T) == typeof(long))
        {
            evaluator.Logger?.Log(
                Severity.Error,
                LogId.InvalidCast,
                ExpressionToken,
                "FloatToIntegerConversionError",
                Value,
                typeof(T).Name
            );
            return false;
        }

        // T.CreateSaturating is safe here as we've validated the "integerness" 
        // if necessary. For float -> float conversions, it handles Infinity/NaN.
        result = new ExpressionResult<T>(T.CreateSaturating(Value));
        return true;
    }
}
