// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using Zarem.Assembler.Extensions.System;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Parsers.Expressions;
using Zarem.Assembler.Parsers.Expressions.Abstract;
using Zarem.Assembler.Parsers.Expressions.Enums;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Parsers;

/// <summary>
/// Parses expressions
/// </summary>
public readonly partial struct ExpressionParser
{
    // Chars: 'a' or '\n' 
    [GeneratedRegex(@"^'(?<char>.*)'$")]
    private static partial Regex CharRegex();

    // Floats: 1.0, -0.5, 1e10
    // Prefixed Ints: 0x, 0b, 0o
    // Standard Ints: 123
    [GeneratedRegex(@"^(?:(?<float>[-+]?\d+\.\d+(?:[eE][-+]?\d+)?) | 0x(?<hex>[0-9a-fA-F]+) | 0b(?<bin>[01]+) | 0o(?<oct>[0-7]+) | (?<int>[-+]?\d+))$", RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex NumberRegex();

    private readonly AssemblerLogger? _logger;
    private readonly IReadOnlyDictionary<string, Symbol>? _symbols;
    private readonly List<Symbol> _references; 

    private ExpressionParser(IReadOnlyDictionary<string, Symbol>? symbols, ILogger? logger)
    {
        _symbols = symbols;
        _references = [];

        if (logger is not null)
        {
            _logger = new AssemblerLogger(logger);
        }
    }

    /// <summary>
    /// Parses a set of tokens as an expression.
    /// </summary>
    /// <param name="expression">The tokens to parse as an expression.</param>
    /// <param name="result">The expression parsing results.</param>
    /// <param name="symbols">The assembler context containing declared symbols, if desired.</param>
    /// <param name="logger">The logger to log errors or warnings, if desired.</param>
    /// <returns>Whether or not the expression could be parsed.</returns>
    public static bool TryParse<T>(ReadOnlySpan<Token> expression, out ExpressionResult<T> result, IReadOnlyDictionary<string, Symbol>? symbols, ILogger? logger)
        where T : unmanaged, IBinaryNumber<T>
    {
        result = default;

        // Parse expression tree
        var parser = new ExpressionParser(symbols, logger);
        var node = parser.ParsePrecedence(ref expression, 0);

        // Expression tree could not be parsed
        // Error already logged
        if (node is null)
            return false;

        if (!expression.IsEmpty)
        {
            parser._logger?.Log(Severity.Error, LogId.UnexpectedToken, expression, "UnexpectedToken", expression[0]);
            return false;
        }

        // Evaluate the address
        var eval = new Evaluator<T>(symbols, logger);
        if (!node.TryEvaluate(eval, out result))
            return false;

        return true;
    }

    private ExpNode? ParsePrecedence(ref ReadOnlySpan<Token> tokens, int minBindingPower)
    {
        // NOTE: "token" is consumed as a side effect of Next
        var token = tokens.Next();
        if (token is null)
            return null;

        var node = NullDenotation(ref tokens, token);

        // Left node could not be parsed.
        // Error already logged
        if (node is null)
            return null;

        while (!tokens.IsEmpty && tokens[0].Type is TokenType.Operator &&
            TryGetBinaryOperator(tokens[0].Source, out var op) &&
            TryGetBindingPowers(op, out var leftBindingPower, out var rightBindingPower) &&
            leftBindingPower >= minBindingPower)
        {
            // Consume operator token
            var opToken = tokens.Next();
            Guard.IsNotNull(opToken);

            node = LeftDenotation(ref tokens, node, opToken, op, rightBindingPower);

            // Left node could not be parsed.
            // Error already logged
            if (node is null)
                return null;
        }

        return node;
    }

    private ExpNode? NullDenotation(ref ReadOnlySpan<Token> tokens, Token token)
    {
        ExpNode? result = null;

        bool success = token.Type switch
        {
            TokenType.Immediate => TryParseImmediate(token, out result),
            TokenType.Reference => TryParseReference(token, out result),
            TokenType.OpenParenthesis => TryParseParenthesis(ref tokens, token, out result),

            TokenType.Operator when TryGetUnaryOperator(token.Source, out var op)
                => TryParseUnaryOperator(ref tokens, token, op, out result),

            _ => _logger?.Log(Severity.Error, LogId.UnexpectedToken, token, "UnexpectedToken", token) ?? false,
        };

        if (!success)
        {
            return null;
        }

        return result;
    }

    private bool TryParseImmediate(Token token, [NotNullWhen(true)] out ExpNode? result)
    {
        result = null;

        var charMatch = CharRegex().Match(token.Source);
        if (charMatch.Success)
        {
            if (StringParser.TryParseChar(token, out char c, _logger?.Parent))
            {
                result = new IntegerNode(token, c);
                return true;
            }
            return false;
        }

        string cleanSource = token.Source.Replace("_", "");
        var numMatch = NumberRegex().Match(cleanSource);
        if (numMatch.Success)
        {
            try
            {
                // Handle Floats separately due to double vs long
                if (numMatch.Groups["float"].Success)
                {
                    result = new FloatNode(token, double.Parse(numMatch.Groups["float"].Value, CultureInfo.InvariantCulture));
                    return true;
                }

                // Handle all Integer bases (hex, bin, oct, int)
                var (groupName, radix) = numMatch.Groups.Values
                    .Where(g => g.Success && g.Name != "0") // "0" is the full match
                    .Select(g => (g.Name, Radix: g.Name switch
                    {
                        "hex" => 16,
                        "bin" => 2,
                        "oct" => 8,
                        _ => 10
                    }))
                    .First();

                long value = Convert.ToInt64(numMatch.Groups[groupName].Value, radix);
                result = new IntegerNode(token, value);
                return true;
            }
            catch (OverflowException)
            {
                _logger?.Log(Severity.Error, LogId.IntegerTruncated, token, "ImmediateOverflow", token);
                return false;
            }
        }

        _logger?.Log(Severity.Error, LogId.UnparsableExpression, token, "UnparsableImmediate", token);
        return false;
    }

    private bool TryParseReference(Token token, [NotNullWhen(true)] out ExpNode? result)
    {
        result = null;

        if (_symbols?.TryGetValue(token.Source, out var symbol) is not true)
        {

            _logger?.Log(Severity.Error, LogId.UndeclaredSymbolReferenced, token, "UndeclaredSymbolReferenced", token);
            return false;
        }

        _references.Add(symbol);
        result = new SymbolNode(token, symbol);
        return true;
    }

    private bool TryParseParenthesis(ref ReadOnlySpan<Token> tokens, Token token, [NotNullWhen(true)] out ExpNode? result)
    {
        result = null;

        var inner = ParsePrecedence(ref tokens, 0);

        // Child node could not be parsed.
        // Error already logged
        if (inner is null)
            return false;

        // Check for a closing parenthesis
        if (tokens.IsEmpty || tokens[0].Type != TokenType.CloseParenthesis)
        {
            _logger?.Log(Severity.Error, LogId.UnparsableExpression, token, "ExpectedClosingParenthesis");
            return false;
        }

        // Consume the closing parenthesis
        tokens.Next();
        result = inner;
        return true;
    }

    private bool TryParseUnaryOperator(ref ReadOnlySpan<Token> tokens, Token opToken, Operation op, out ExpNode? result)
    {
        result = null;

        if (!TryGetBindingPower(op, out var bindingPower))
            return false;

        if (tokens.IsEmpty)
        {
            _logger?.Log(Severity.Error, LogId.UnparsableExpression, opToken, "MissingOperand", opToken);
            return false;
        }

        var child = ParsePrecedence(ref tokens, bindingPower);
        if (child is null)
            return false;

        result = new UnaryOperNode(opToken, op)
        {
            Child = child
        };
        return true;
    }

    private BinaryOperNode? LeftDenotation(ref ReadOnlySpan<Token> tokens, ExpNode left, Token opToken, Operation op, int rightBindingPower)
    {
        if (tokens.IsEmpty)
        {
            _logger?.Log(Severity.Error, LogId.UnparsableExpression, opToken, "MissingOperand", opToken);
            return null;
        }

        var right = ParsePrecedence(ref tokens, rightBindingPower);
        if (right is null)
            return null;

        return new BinaryOperNode(opToken, op)
        {
            LeftChild = left,
            RightChild = right
        };
    }
    
    private static bool TryGetUnaryOperator(string s, out Operation op)
    {
        op = s switch
        {
            "+" => Operation.UnaryPlus,
            "-" => Operation.Negation,
            "~" => Operation.Not, // Accept "Binary not"
            _ => (Operation)(-1),
        };

        return op is not (Operation)(-1);
    }

    private static bool TryGetBinaryOperator(string s, out Operation op)
    {
        op = s switch
        {
            "+" => Operation.Addition,
            "-" => Operation.Subtraction,
            "*" => Operation.Multiplication,
            "/" => Operation.Division,
            "%" => Operation.Modulus,
            "&" => Operation.And,
            "|" => Operation.Or,
            "^" => Operation.Xor,
            "<<" => Operation.LeftShift,
            ">>" => Operation.RightShift,
            _ => (Operation)(-1),
        };

        return op is not (Operation)(-1);
    }

    private static bool TryGetBindingPower(Operation op, out int bindingPower)
    {
        if (!TryGetBindingPowers(op, out bindingPower, out var right))
            return false;

        // Expected a unary operation
        if (right is not -1)
            return false;

        return true;
    }

    private static bool TryGetBindingPowers(Operation op, out int leftBindingPower, out int rightBindingPower)
    {
        (leftBindingPower, rightBindingPower) = op switch
        {
            // Binary operations
            Operation.Multiplication or 
            Operation.Division or
            Operation.Modulus => (60, 61),

            Operation.Addition or
            Operation.Subtraction => (50, 51),
            
            Operation.And => (40, 41),
            Operation.Xor => (35, 36),
            Operation.Or => (30, 31),
            
            Operation.LeftShift or
            Operation.RightShift => (45, 46),
            
            // Unary operations
            Operation.UnaryPlus => (80, -1),
            Operation.Negation => (80, -1),
            Operation.Not => (80, -1),

            _ => (-1, -1),
        };

        if (leftBindingPower is -1)
            return false;

        return true;
    }
}
