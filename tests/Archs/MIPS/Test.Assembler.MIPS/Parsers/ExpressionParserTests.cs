// Avishai Dernis 2024

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization;
using Zarem.Assembler.Tokenization.Models.Enums;

namespace Test.Assembler.MIPS.Parsers;

[TestClass]
public class ExpressionParserTests
{
    public static IEnumerable<object[]> ExpressionSuccessTestsList =>
    [
        ["10", 10],
        ["-10", -10],
        ["10", 10],
        ["25 - 10", 25 - 10],
        ["4 * 4", 4 * 4],
        ["8 / 2", 8 / 2],
        ["8 % 3", 8 % 3],
        ["9 & 3", 9 & 3],
        ["9 | 3", 9 | 3],
        ["9 ^ 3", 9 ^ 3],
        ["~10", ~10],
        ["10 * -10", 10 * -10],
        ["0b1010", 0b1010],
        ["0o12", 10], // C# Doesn't support oct
        ["0xa", 0xa],
        ["4 * 2 + 2", 4 * 2 + 2],
        ["4 + 2 * 2", 4 + 2 * 2],
        ["(4 + 2) * 2", (4 + 2) * 2],
        ["'a'", 'a'],
        [@"'\n'", '\n'],
        ["'a' + 10", 'a' + 10],
    ];

    public static IEnumerable<object[]> ExpressionFailureTestsList =>
    [
        ["+"],
        ["*10"],
        ["10-"],
        ["-*10"],
        ["10 10"],
        ["0b102"],
        ["4 + 2) * 2"],
        ["(4 + 2 * 2"],
        ["'abc'"],
        [@"'\x'"],
    ];

    [DataTestMethod]
    [DynamicData(nameof(ExpressionSuccessTestsList))]
    public void ExpressionSuccessTests(string input, int expected)
        => RunTest(input, expected);

    [DataTestMethod]
    [DynamicData(nameof(ExpressionFailureTestsList))]
    public void ExpressionFailureTests(string input)
        => RunTest(input);

    private static void RunTest(string input, long? expected = null)
    {
        var line = Tokenizer.TokenizeLine(input, nameof(RunTest), TokenizerMode.Expression)[0];
        bool success = ExpressionParser.TryParse<long>(line.Tokens, out var actual, null, null);
        Assert.AreEqual(success, expected.HasValue);
        if (expected.HasValue)
        {
            Assert.AreEqual(expected.Value, actual.Addend);
        }
    }
}
